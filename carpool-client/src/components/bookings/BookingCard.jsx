import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { getRide } from '../../api/rides'
import { BookingStatusBadge } from '../common/Badge'
import { Button } from '../common/Button'
import { formatDate, formatDateTime } from '../../utils/format'
import styles from './BookingCard.module.css'

export function BookingCard({ booking, onCancel }) {
  const navigate = useNavigate()
  const [ride, setRide] = useState(null)

  useEffect(() => {
    let cancelled = false
    getRide(booking.rideId)
      .then((r) => !cancelled && setRide(r))
      .catch(() => !cancelled && setRide(false))
    return () => {
      cancelled = true
    }
  }, [booking.rideId])

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
        <BookingStatusBadge status={booking.status} />
      </div>

      <div className={styles.actions}>
        {ride && (
          <Button size="sm" variant="secondary" onClick={() => navigate(`/rides/${booking.rideId}`)}>
            View Ride
          </Button>
        )}
        {booking.status === 'Active' && (
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
