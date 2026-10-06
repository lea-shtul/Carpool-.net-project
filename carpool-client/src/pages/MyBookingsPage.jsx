import { useCallback, useEffect, useState } from 'react'
import { cancelBooking, getMyBookings } from '../api/bookings'
import { BookingCard } from '../components/bookings/BookingCard'
import { Card } from '../components/common/Card'
import { ConfirmDialog } from '../components/common/ConfirmDialog'
import { EmptyState, ErrorState, LoadingState } from '../components/common/States'
import { useApiErrorToast, useToast } from '../hooks/useToast'
import { useInterval } from '../hooks/useInterval'
import styles from './MyBookingsPage.module.css'

const TABS = ['All', 'Active', 'Completed', 'Cancelled']

// Mirrors the Dashboard's polling (DashboardPage.jsx) — a booking's own status also
// changes on its own (e.g. Active -> Completed once the ride finishes), not just the
// nested ride status each BookingCard polls independently.
const BOOKINGS_POLL_INTERVAL_MS = 15000

export function MyBookingsPage() {
  const { showToast } = useToast()
  const showApiError = useApiErrorToast()
  const [bookings, setBookings] = useState(null)
  const [error, setError] = useState(null)
  const [tab, setTab] = useState('All')
  const [pendingCancel, setPendingCancel] = useState(null)
  const [isCancelling, setIsCancelling] = useState(false)

  const load = useCallback((isBackgroundRefresh = false) => {
    getMyBookings()
      .then((data) => {
        setBookings(data)
        setError(null)
      })
      .catch((err) => {
        // A failed background refresh keeps showing the last good list rather than
        // replacing it with an error — only the very first load surfaces ErrorState.
        if (!isBackgroundRefresh) setError(err)
      })
  }, [])

  useEffect(() => {
    load(false)
  }, [load])

  useInterval(() => load(true), BOOKINGS_POLL_INTERVAL_MS)

  useEffect(() => {
    function onVisible() {
      if (document.visibilityState === 'visible') load(true)
    }
    document.addEventListener('visibilitychange', onVisible)
    return () => document.removeEventListener('visibilitychange', onVisible)
  }, [load])

  async function confirmCancel() {
    setIsCancelling(true)
    try {
      await cancelBooking(pendingCancel.id)
      showToast({ type: 'success', message: 'Booking cancelled — your seat(s) were released.' })
      setPendingCancel(null)
      load()
    } catch (err) {
      showApiError(err)
    } finally {
      setIsCancelling(false)
    }
  }

  const filtered = bookings?.filter((b) => tab === 'All' || b.status === tab) ?? []

  return (
    <div>
      <h1 style={{ fontSize: 22, fontWeight: 800, marginBottom: 'var(--space-5)' }}>My Bookings</h1>

      <div className={styles.tabs}>
        {TABS.map((t) => (
          <button
            key={t}
            type="button"
            className={`${styles.tab} ${tab === t ? styles.tabActive : ''}`}
            onClick={() => setTab(t)}
          >
            {t}
          </button>
        ))}
      </div>

      <Card padded={false} style={{ padding: '0 var(--space-5)' }}>
        {bookings === null && !error && (
          <div style={{ padding: 'var(--space-5) 0' }}>
            <LoadingState count={3} />
          </div>
        )}
        {error && <ErrorState message={error.message} correlationId={error.correlationId} onRetry={load} />}
        {bookings && filtered.length === 0 && (
          <EmptyState title="No bookings here" subtitle="Rides you book will show up in this list." />
        )}
        {filtered.map((booking) => (
          <BookingCard key={booking.id} booking={booking} onCancel={setPendingCancel} />
        ))}
      </Card>

      {pendingCancel && (
        <ConfirmDialog
          title="Cancel this booking?"
          message={`This will release ${pendingCancel.numberOfSeats} seat(s) back to the ride.`}
          confirmLabel="Cancel Booking"
          tone="danger"
          isLoading={isCancelling}
          onConfirm={confirmCancel}
          onCancel={() => setPendingCancel(null)}
        />
      )}
    </div>
  )
}
