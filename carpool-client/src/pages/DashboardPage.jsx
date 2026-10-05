import { CalendarCheck, HelpCircle, PlusCircle, Star } from 'lucide-react'
import { useCallback, useEffect, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { getMyBookings } from '../api/bookings'
import { getRatingsForUser } from '../api/ratings'
import { getRide, searchRides } from '../api/rides'
import { Button } from '../components/common/Button'
import { Card } from '../components/common/Card'
import { BookingStatusBadge } from '../components/common/Badge'
import { EmptyState, ErrorState, LoadingState } from '../components/common/States'
import { RatingStars } from '../components/common/RatingStars'
import { RoadTripIllustration } from '../components/common/RoadTripIllustration'
import { WavingHand } from '../components/common/WavingHand'
import { RideCard } from '../components/rides/RideCard'
import { SearchPanel } from '../components/rides/SearchPanel'
import { useAuth } from '../hooks/useAuth'
import { useInterval } from '../hooks/useInterval'
import { formatDate, formatDateTime } from '../utils/format'
import styles from './DashboardPage.module.css'

// The backend's RideStatusBackgroundService reconciles ride status every 30s (e.g.
// Scheduled -> InProgress once DepartureTime passes). Polling a little faster than that
// keeps "Available Rides" from showing a ride that has in fact already started, without
// needing a push mechanism (WebSockets/SignalR) the base spec excludes.
const RIDES_POLL_INTERVAL_MS = 15000

function greetingForNow() {
  const hour = new Date().getHours()
  if (hour < 12) return 'Good morning'
  if (hour < 18) return 'Good afternoon'
  return 'Good evening'
}

export function DashboardPage() {
  const { user } = useAuth()
  const navigate = useNavigate()

  const [rides, setRides] = useState(null)
  const [ridesError, setRidesError] = useState(null)

  const [upcoming, setUpcoming] = useState(null)
  const [recentBookings, setRecentBookings] = useState(null)
  const [recentRatings, setRecentRatings] = useState(null)

  const loadRides = useCallback((isBackgroundRefresh = false) => {
    searchRides({ page: 1, pageSize: 5, sortBy: 'departureTime', sortDirection: 'asc', availableOnly: true })
      .then((data) => {
        setRides(data.items)
        setRidesError(null)
      })
      .catch((err) => {
        // A failed background refresh keeps showing the last good list rather than
        // replacing it with an error — only the very first load surfaces ErrorState.
        if (!isBackgroundRefresh) setRidesError(err)
      })
  }, [])

  useEffect(() => {
    loadRides(false)
  }, [loadRides])

  useInterval(() => loadRides(true), RIDES_POLL_INTERVAL_MS)

  // Catches the "switched tabs away and back" case immediately, instead of waiting for
  // the next poll tick.
  useEffect(() => {
    function onVisible() {
      if (document.visibilityState === 'visible') loadRides(true)
    }
    document.addEventListener('visibilitychange', onVisible)
    return () => document.removeEventListener('visibilitychange', onVisible)
  }, [loadRides])

  useEffect(() => {
    getMyBookings()
      .then(async (bookings) => {
        const sorted = [...bookings].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
        setRecentBookings(sorted.slice(0, 3))

        // Small, bounded preview (<=2 rides) — fetching each ride's details here is fine,
        // unlike a full list page where that would mean an unbounded N+1 fan-out.
        const activeBookings = sorted.filter((b) => b.status === 'Active').slice(0, 2)
        const withRides = await Promise.all(
          activeBookings.map(async (b) => ({ booking: b, ride: await getRide(b.rideId).catch(() => null) })),
        )
        setUpcoming(withRides.filter((x) => x.ride))
      })
      .catch(() => {
        setRecentBookings([])
        setUpcoming([])
      })
  }, [])

  useEffect(() => {
    if (!user) return
    getRatingsForUser(user.id)
      .then((ratings) => {
        const sorted = [...ratings].sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt))
        setRecentRatings(sorted.slice(0, 3))
      })
      .catch(() => setRecentRatings([]))
  }, [user])

  return (
    <div>
      <div className={styles.greeting} style={{ marginBottom: 'var(--space-6)' }}>
        <h1 className={styles.greetingTitle}>
          {greetingForNow()}, {user?.firstName}! <WavingHand />
        </h1>
        <p className={styles.greetingSubtitle}>Where are you going today?</p>
      </div>

      <div className={styles.layout}>
        <div className={styles.main}>
          <Card>
            <SearchPanel />
          </Card>

          <div className={styles.hero}>
            <div style={{ position: 'relative', zIndex: 1 }}>
              <div className={styles.heroTitle}>
                Share a ride.
                <br />
                Save money.
                <br />
                Meet people.
              </div>
              <div className={styles.heroSubtitle}>
                Travel together, reduce costs and build connections!
              </div>
            </div>
            <Button
              icon={PlusCircle}
              onClick={() => navigate('/rides/new')}
              style={{ position: 'relative', zIndex: 1 }}
            >
              Offer a Ride
            </Button>
            <RoadTripIllustration className={styles.heroArt} />
          </div>

          <div>
            <div className={styles.sectionHeader}>
              <div className={styles.sectionTitle}>
                Available Rides
                {rides && <span className={styles.countPill}>{rides.length} shown</span>}
              </div>
              <Link to="/rides" className={styles.viewAllLink}>
                View all
              </Link>
            </div>
            <Card padded={false} style={{ padding: '0 var(--space-5)' }}>
              {rides === null && !ridesError && <div style={{ padding: 'var(--space-5) 0' }}><LoadingState count={3} /></div>}
              {ridesError && <ErrorState message={ridesError.message} correlationId={ridesError.correlationId} />}
              {rides && rides.length === 0 && (
                <EmptyState title="No rides available right now" subtitle="Check back soon, or offer a ride yourself." />
              )}
              {rides && rides.map((ride) => <RideCard key={ride.id} ride={ride} />)}
            </Card>
          </div>
        </div>

        <div className={styles.rightRail}>
          <Card>
            <div className={styles.widgetTitle}>
              <CalendarCheck size={16} />
              Upcoming Rides
            </div>
            {upcoming === null && <LoadingState count={1} />}
            {upcoming && upcoming.length === 0 && (
              <EmptyState title="No upcoming rides" subtitle="Book a ride to see it here." />
            )}
            {upcoming?.map(({ booking, ride }) => (
              <div key={booking.id} style={{ marginBottom: 'var(--space-3)' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                  <strong style={{ fontSize: 13 }}>{formatDateTime(ride.departureTime)}</strong>
                  <BookingStatusBadge status={booking.status} />
                </div>
                <div style={{ fontSize: 12.5, color: 'var(--color-text-muted)', marginTop: 2 }}>
                  {ride.origin} → {ride.destination}
                </div>
                <div style={{ fontSize: 11.5, color: 'var(--color-text-faint)' }}>
                  Driver: {ride.driver.firstName} {ride.driver.lastName} · {booking.numberOfSeats} seat(s)
                </div>
              </div>
            ))}
            <Link to="/bookings" className={styles.viewAllLink}>
              View all
            </Link>
          </Card>

          <Card>
            <div className={styles.widgetTitle}>
              <CalendarCheck size={16} />
              My Bookings
            </div>
            {recentBookings === null && <LoadingState count={1} />}
            {recentBookings && recentBookings.length === 0 && (
              <EmptyState title="No bookings yet" subtitle="Find a ride to get started." />
            )}
            {recentBookings?.map((b) => (
              <div
                key={b.id}
                style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 'var(--space-2)' }}
              >
                <span style={{ fontSize: 12.5 }}>
                  Ride #{b.rideId} · {formatDate(b.createdAt)}
                </span>
                <BookingStatusBadge status={b.status} />
              </div>
            ))}
            <Link to="/bookings" className={styles.viewAllLink}>
              View all
            </Link>
          </Card>

          <Card>
            <div className={styles.widgetTitle}>
              <Star size={16} />
              Recent Ratings
            </div>
            {recentRatings === null && <LoadingState count={1} />}
            {recentRatings && recentRatings.length === 0 && (
              <EmptyState title="No ratings yet" subtitle="Ratings you receive as a driver appear here." />
            )}
            {recentRatings?.map((r) => (
              <div key={r.id} style={{ marginBottom: 'var(--space-3)' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                  <RatingStars score={r.score} size={13} />
                  <span style={{ fontSize: 11, color: 'var(--color-text-faint)' }}>{formatDate(r.createdAt)}</span>
                </div>
                {r.comment && (
                  <div style={{ fontSize: 12, color: 'var(--color-text-muted)', marginTop: 2 }}>"{r.comment}"</div>
                )}
              </div>
            ))}
            <Link to="/ratings" className={styles.viewAllLink}>
              View all
            </Link>
          </Card>

          <Card>
            <div className={styles.widgetTitle}>
              <HelpCircle size={16} />
              Have questions?
            </div>
            <p style={{ fontSize: 12.5, color: 'var(--color-text-muted)', marginBottom: 'var(--space-3)' }}>
              Check our Help Center for answers.
            </p>
            <Link to="/help" className={styles.viewAllLink}>
              Check our Help Center
            </Link>
          </Card>
        </div>
      </div>
    </div>
  )
}
