import { useEffect, useState } from 'react'
import { getBookingsForRide } from '../../api/bookings'
import { Avatar } from '../common/Avatar'
import { BookingStatusBadge } from '../common/Badge'
import { EmptyState, ErrorState, LoadingState } from '../common/States'
import { formatDate } from '../../utils/format'

/**
 * Driver-only view of who booked a ride (spec extension — GET /api/rides/{rideId}/bookings).
 * The backend already enforces that only the ride's driver (or an Admin) can call this
 * endpoint; this panel is additionally only ever rendered for the driver (see
 * RideDetailsPage), so a passenger never even sees the "View Bookings" button.
 */
export function RideBookingsPanel({ rideId }) {
  const [bookings, setBookings] = useState(null)
  const [error, setError] = useState(null)

  function load() {
    setError(null)
    getBookingsForRide(rideId)
      .then(setBookings)
      .catch((err) => setError(err))
  }

  useEffect(load, [rideId])

  if (bookings === null && !error) return <LoadingState count={2} />
  if (error) return <ErrorState message={error.message} correlationId={error.correlationId} onRetry={load} />
  if (bookings.length === 0) {
    return <EmptyState title="No bookings yet" subtitle="Bookings passengers make on this ride will show up here." />
  }

  return (
    <div>
      {bookings.map((b) => (
        <div
          key={b.id}
          style={{
            display: 'flex',
            alignItems: 'center',
            gap: 'var(--space-3)',
            padding: 'var(--space-3) 0',
            borderBottom: '1px solid var(--color-border)',
          }}
        >
          <Avatar firstName={b.passenger.firstName} lastName={b.passenger.lastName} size={34} />
          <div style={{ flex: 1 }}>
            <div style={{ fontSize: 13, fontWeight: 600 }}>
              {b.passenger.firstName} {b.passenger.lastName}
            </div>
            <div style={{ fontSize: 11.5, color: 'var(--color-text-muted)' }}>
              {b.numberOfSeats} seat(s) · booked {formatDate(b.createdAt)}
              {b.cancelledAt && ` · cancelled ${formatDate(b.cancelledAt)}`}
            </div>
          </div>
          <BookingStatusBadge status={b.status} />
        </div>
      ))}
    </div>
  )
}
