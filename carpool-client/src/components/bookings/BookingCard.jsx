import { useCallback, useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { getRide } from '../../api/rides'
import { BookingStatusBadge, RideStatusBadge } from '../common/Badge'
import { Button } from '../common/Button'
import { useInterval } from '../../hooks/useInterval'
import { formatDate, formatDateTime } from '../../utils/format'
import styles from './BookingCard.module.css'

// Mirrors the Dashboard's ride-status polling (DashboardPage.jsx) — faster than the
// backend's own 30s reconciliation cycle, so e.g. Scheduled -> InProgress shows up here
// on its own instead of needing a navigate-away-and-back to refetch.
const RIDE_POLL_INTERVAL_MS = 15000

export function BookingCard({ booking, onCancel }) {
  const navigate = useNavigate()
  const [ride, setRide] = useState(null)

  const loadRide = useCallback(
    (isBackgroundRefresh = false) => {
      getRide(booking.rideId)
        .then(setRide)
        .catch(() => {
          // A failed background refresh keeps showing the last good ride info rather
          // than flashing "Ride #N" — only the very first load falls back to that.
          if (!isBackgroundRefresh) setRide(false)
        })
    },
    [booking.rideId],
  )

  useEffect(() => {
    loadRide(false)
  }, [loadRide])

  useInterval(() => loadRide(true), RIDE_POLL_INTERVAL_MS)

  useEffect(() => {
    function onVisible() {
      if (document.visibilityState === 'visible') loadRide(true)
    }
    document.addEventListener('visibilitychange', onVisible)
    return () => document.removeEventListener('visibilitychange', onVisible)
  }, [loadRide])

  return (
    <div className={styles.card}>
      <div className={styles.main}>
        {ride === null && <div className={styles.skeleton} />}
        {ride === false && <div className={styles.route}>Ride #{booking.rideId}</div>}
        {ride && (
          <>
            <div className={styles.route}>
              {ride.origin} → {ride.destination}
            </div>
            <div className={styles.meta}>{formatDateTime(ride.departureTime)}</div>
          </>
        )}
        <div className={styles.meta}>Booked {formatDate(booking.createdAt)}</div>
      </div>

      <div className={styles.col}>
        <div className={styles.colLabel}>Seats</div>
        <div className={styles.colValue}>{booking.numberOfSeats}</div>
      </div>

      <div className={styles.col}>
        <div className={styles.colLabel}>Booking</div>
        <BookingStatusBadge status={booking.status} />
        {ride && (
          <>
            <div className={styles.colLabel} style={{ marginTop: 6 }}>
              Trip
            </div>
            <RideStatusBadge status={ride.status} />
          </>
        )}
      </div>

      <div className={styles.actions}>
        {ride && (
          <Button size="sm" variant="secondary" onClick={() => navigate(`/rides/${booking.rideId}`)}>
            View Ride
          </Button>
        )}
        {/* The backend only allows cancelling while the ride is still Scheduled (not once
            it's InProgress) — gating on ride.status too, not just booking.status, avoids
            offering a Cancel that the server would reject. */}
        {booking.status === 'Active' && ride && ride.status === 'Scheduled' && (
          <Button size="sm" variant="danger" onClick={() => onCancel(booking)}>
            Cancel
          </Button>
        )}
        {booking.status === 'Completed' && (
          <Button size="sm" variant="secondary" onClick={() => navigate(`/rides/${booking.rideId}`)}>
            Rate ride
          </Button>
        )}
      </div>
    </div>
  )
}
