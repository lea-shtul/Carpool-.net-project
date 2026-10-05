import { AlertTriangle, RefreshCw } from 'lucide-react'
import { useState } from 'react'
import { createBooking } from '../../api/bookings'
import { isConcurrencyConflict } from '../../api/errors'
import { Button } from '../common/Button'
import { FormField, TextInput } from '../common/FormField'
import { useToast } from '../../hooks/useToast'
import styles from './BookingForm.module.css'

/**
 * Booking form with the required 409 UX: seats are a limited resource protected by
 * backend optimistic concurrency, so a 409 here specifically means someone else just
 * booked the seat(s) first — not a generic failure. We surface that distinctly and let
 * the caller refresh the ride instead of blindly resubmitting a stale request.
 */
export function BookingForm({ ride, onBooked, onRefreshRide }) {
  const { showToast } = useToast()
  const [numberOfSeats, setNumberOfSeats] = useState(1)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [conflict, setConflict] = useState(null)
  const [error, setError] = useState('')

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    setConflict(null)
    setIsSubmitting(true)
    try {
      const booking = await createBooking(ride.id, Number(numberOfSeats))
      showToast({ type: 'success', message: `Booked ${numberOfSeats} seat(s) on this ride.` })
      onBooked?.(booking)
    } catch (err) {
      if (isConcurrencyConflict(err)) {
        setConflict(err)
      } else {
        setError(err.message)
      }
    } finally {
      setIsSubmitting(false)
    }
  }

  async function handleRefresh() {
    setConflict(null)
    await onRefreshRide?.()
  }

  return (
    <div>
      <form onSubmit={handleSubmit} className={styles.row}>
        <div className={styles.seatsField}>
          <FormField label="Number of seats">
            <TextInput
              type="number"
              min={1}
              max={ride.availableSeats}
              value={numberOfSeats}
              onChange={(e) => setNumberOfSeats(e.target.value)}
            />
          </FormField>
        </div>
        <Button type="submit" isLoading={isSubmitting} disabled={ride.availableSeats === 0}>
          Book This Ride
        </Button>
      </form>

      {error && <p style={{ color: 'var(--color-danger-text)', fontSize: 13, marginTop: 'var(--space-3)' }}>{error}</p>}

      {conflict && (
        <div className={styles.conflictBanner}>
          <AlertTriangle size={20} />
          <div style={{ flex: 1 }}>
            <div className={styles.conflictTitle}>This seat was just taken by another passenger</div>
            <div className={styles.conflictBody}>
              The ride's availability changed between your request and the server. Refresh to see the
              current seat count and try again.
            </div>
            {conflict.correlationId && (
              <div className={styles.correlationId}>Ref: {conflict.correlationId}</div>
            )}
            <div style={{ marginTop: 'var(--space-3)' }}>
              <Button size="sm" variant="secondary" icon={RefreshCw} onClick={handleRefresh}>
                Refresh ride
              </Button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
