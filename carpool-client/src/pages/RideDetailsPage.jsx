import { CarFront, CheckCircle2, Clock, MapPin, Users, UsersRound } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { useNavigate, useParams } from 'react-router-dom'
import { getMyBookings } from '../api/bookings'
import { cancelRide, completeRide, getRide } from '../api/rides'
import { BookingForm } from '../components/bookings/BookingForm'
import { RideBookingsPanel } from '../components/bookings/RideBookingsPanel'
import { Avatar } from '../components/common/Avatar'
import { RideStatusBadge } from '../components/common/Badge'
import { Button } from '../components/common/Button'
import { Card } from '../components/common/Card'
import { ErrorState, LoadingState } from '../components/common/States'
import { TagBadge } from '../components/common/TagBadge'
import { RatingForm } from '../components/ratings/RatingForm'
import { useAuth } from '../hooks/useAuth'
import { useApiErrorToast, useToast } from '../hooks/useToast'
import { formatCurrency, formatDate, formatDuration, formatTime } from '../utils/format'
import styles from './RideDetailsPage.module.css'

export function RideDetailsPage() {
  const { id } = useParams()
  const { user } = useAuth()
  const navigate = useNavigate()
  const { showToast } = useToast()
  const showApiError = useApiErrorToast()

  const [ride, setRide] = useState(null)
  const [error, setError] = useState(null)
  const [hasRated, setHasRated] = useState(false)
  const [isActing, setIsActing] = useState(false)
  // null while loading, undefined once loaded-with-no-active-booking, or the booking itself.
  const [myActiveBooking, setMyActiveBooking] = useState(null)
  const [showBookings, setShowBookings] = useState(false)

  const loadRide = useCallback(() => {
    setError(null)
    return getRide(id)
      .then(setRide)
      .catch((err) => setError(err))
  }, [id])

  useEffect(() => {
    loadRide()
  }, [loadRide])

  useEffect(() => {
    getMyBookings()
      .then((bookings) => {
        const active = bookings.find((b) => b.rideId === Number(id) && b.status === 'Active')
        setMyActiveBooking(active ?? undefined)
      })
      .catch(() => setMyActiveBooking(undefined))
  }, [id])

  if (error) return <ErrorState message={error.message} correlationId={error.correlationId} onRetry={loadRide} />
  if (!ride) return <LoadingState count={2} />

  const isDriver = ride.driver.id === user?.id
  // Also gated on myActiveBooking being resolved (not null = still loading), so the form
  // never flashes before we know whether the caller already booked this ride.
  const canBook =
    !isDriver && ride.status === 'Scheduled' && ride.availableSeats > 0 && myActiveBooking === undefined
  const canRate = !isDriver && ride.status === 'Completed' && !hasRated

  async function handleCancelRide() {
    setIsActing(true)
    try {
      const updated = await cancelRide(ride.id)
      setRide(updated)
      showToast({ type: 'success', message: 'Ride cancelled.' })
    } catch (err) {
      showApiError(err)
    } finally {
      setIsActing(false)
    }
  }

  async function handleCompleteRide() {
    setIsActing(true)
    try {
      const updated = await completeRide(ride.id)
      setRide(updated)
      showToast({ type: 'success', message: 'Ride marked as completed.' })
    } catch (err) {
      showApiError(err)
    } finally {
      setIsActing(false)
    }
  }

  return (
    <div>
      <div className={styles.routeHeader}>
        <div className={styles.routeTitle}>
          <MapPin size={20} />
          {ride.origin} → {ride.destination}
        </div>
        <RideStatusBadge status={ride.status} />
      </div>

      <div className={styles.grid}>
        <Card>
          <div className={styles.statRow}>
            <div className={styles.stat}>
              <div className={styles.statLabel}>Departure</div>
              <div className={styles.statValue}>
                {formatDate(ride.departureTime)} · {formatTime(ride.departureTime)}
              </div>
            </div>
            <div className={styles.stat}>
              <div className={styles.statLabel}>
                <Clock size={11} /> Duration
              </div>
              <div className={styles.statValue}>{formatDuration(ride.estimatedDurationMinutes)}</div>
            </div>
            <div className={styles.stat}>
              <div className={styles.statLabel}>
                <Users size={11} /> Seats
              </div>
              <div className={styles.statValue}>
                {ride.availableSeats} / {ride.totalSeats}
              </div>
            </div>
            <div className={styles.stat}>
              <div className={styles.statLabel}>Per seat</div>
              <div className={styles.statValue}>{formatCurrency(ride.pricePerSeat)}</div>
            </div>
          </div>

          <div className={styles.section}>
            <div className={styles.sectionLabel}>Driver</div>
            <div className={styles.personRow}>
              <Avatar firstName={ride.driver.firstName} lastName={ride.driver.lastName} />
              <strong>
                {ride.driver.firstName} {ride.driver.lastName}
              </strong>
            </div>
          </div>

          <div className={styles.section}>
            <div className={styles.sectionLabel}>Vehicle</div>
            <div className={styles.personRow}>
              <CarFront size={18} />
              {ride.vehicle.manufacturer} {ride.vehicle.model} · {ride.vehicle.passengerCapacity} seats
            </div>
          </div>

          {ride.tags.length > 0 && (
            <div className={styles.section}>
              <div className={styles.sectionLabel}>Tags</div>
              <div className={styles.tagRow}>
                {ride.tags.map((tag) => (
                  <TagBadge key={tag.id} tag={tag} />
                ))}
              </div>
            </div>
          )}

          {isDriver && (
            <div className={styles.section} style={{ display: 'flex', gap: 'var(--space-2)', flexWrap: 'wrap' }}>
              {ride.status === 'Scheduled' && (
                <>
                  <Button variant="secondary" isLoading={isActing} onClick={handleCompleteRide}>
                    Mark as Completed
                  </Button>
                  <Button variant="danger" isLoading={isActing} onClick={handleCancelRide}>
                    Cancel Ride
                  </Button>
                </>
              )}
              <Button
                variant="secondary"
                icon={UsersRound}
                onClick={() => setShowBookings((v) => !v)}
              >
                {showBookings ? 'Hide Bookings' : 'View Bookings'}
              </Button>
            </div>
          )}

          {isDriver && showBookings && (
            <div className={styles.section}>
              <div className={styles.sectionLabel} style={{ marginBottom: 'var(--space-3)' }}>
                Who booked this ride
              </div>
              <RideBookingsPanel rideId={ride.id} />
            </div>
          )}
        </Card>

        <div>
          {canBook && (
            <Card>
              <div className={styles.sectionLabel} style={{ marginBottom: 'var(--space-3)' }}>
                Book this ride
              </div>
              <BookingForm
                ride={ride}
                onBooked={(booking) => {
                  setMyActiveBooking(booking)
                  loadRide()
                }}
                onRefreshRide={loadRide}
              />
            </Card>
          )}

          {!isDriver && myActiveBooking && (
            <Card>
              <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', color: 'var(--color-success-text)' }}>
                <CheckCircle2 size={18} />
                <strong style={{ fontSize: 13.5 }}>You're booked on this ride</strong>
              </div>
              <p style={{ fontSize: 12.5, color: 'var(--color-text-muted)', marginTop: 'var(--space-2)' }}>
                {myActiveBooking.numberOfSeats} seat(s) reserved.
              </p>
              <div style={{ marginTop: 'var(--space-3)' }}>
                <Button size="sm" variant="secondary" onClick={() => navigate('/bookings')}>
                  View My Bookings
                </Button>
              </div>
            </Card>
          )}

          {!isDriver && !myActiveBooking && ride.status === 'Scheduled' && ride.availableSeats === 0 && (
            <Card>
              <p style={{ fontSize: 13, color: 'var(--color-text-muted)' }}>This ride is fully booked.</p>
            </Card>
          )}

          {canRate && (
            <Card style={{ marginTop: canBook ? 'var(--space-5)' : 0 }}>
              <div className={styles.sectionLabel} style={{ marginBottom: 'var(--space-3)' }}>
                Rate this ride
              </div>
              <RatingForm rideId={ride.id} onRated={() => setHasRated(true)} />
            </Card>
          )}

          {hasRated && (
            <Card style={{ marginTop: 'var(--space-5)' }}>
              <p style={{ fontSize: 13, color: 'var(--color-success-text)' }}>
                Thanks — your rating was submitted.
              </p>
            </Card>
          )}

          <div style={{ marginTop: 'var(--space-4)' }}>
            <Button variant="ghost" onClick={() => navigate(-1)}>
              ← Back
            </Button>
          </div>
        </div>
      </div>
    </div>
  )
}
