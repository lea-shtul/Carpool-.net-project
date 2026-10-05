import { AlertCircle } from 'lucide-react'
import { useEffect, useState } from 'react'
import { searchRides } from '../api/rides'
import { Card } from '../components/common/Card'
import { EmptyState, ErrorState, LoadingState } from '../components/common/States'
import { RideCard } from '../components/rides/RideCard'
import { useAuth } from '../hooks/useAuth'
import styles from './MyBookingsPage.module.css'

const PAGE_SIZE = 100
const MAX_PAGES = 5 // bounds the workaround below to at most 500 scanned rides

/**
 * FRONTEND WORKAROUND (documented per the client spec, §11): the backend has no
 * GET /api/rides/my endpoint and none should be invented. Instead this fetches pages of
 * GET /api/rides (the only ride-listing endpoint that exists) and keeps only the ones
 * whose driver is the current user. This only sees rides within the first
 * PAGE_SIZE * MAX_PAGES results overall, so on a very large dataset it could miss some of
 * a driver's older rides — a real limitation of working around a missing endpoint, not a
 * bug. The proper fix is a backend GET /api/rides/my (or a driverId filter) endpoint.
 */
async function fetchMyRidesWorkaround(driverId) {
  const mine = []
  for (let page = 1; page <= MAX_PAGES; page += 1) {
    const result = await searchRides({ page, pageSize: PAGE_SIZE, sortBy: 'departureTime', sortDirection: 'desc' })
    mine.push(...result.items.filter((r) => r.driver.id === driverId))
    if (result.items.length < PAGE_SIZE || page * PAGE_SIZE >= result.totalCount) break
  }
  return mine
}

const TABS = [
  { key: 'Scheduled', label: 'Upcoming' },
  { key: 'Completed', label: 'Completed' },
  { key: 'Cancelled', label: 'Cancelled' },
]

export function MyRidesPage() {
  const { user } = useAuth()
  const [rides, setRides] = useState(null)
  const [error, setError] = useState(null)
  const [tab, setTab] = useState('Scheduled')

  function load() {
    setError(null)
    fetchMyRidesWorkaround(user.id)
      .then(setRides)
      .catch((err) => setError(err))
  }

  useEffect(load, [user.id])

  const filtered = rides?.filter((r) => r.status === tab || (tab === 'Scheduled' && r.status === 'InProgress')) ?? []

  return (
    <div>
      <h1 style={{ fontSize: 22, fontWeight: 800, marginBottom: 'var(--space-2)' }}>My Rides</h1>
      <p
        style={{
          display: 'flex',
          alignItems: 'center',
          gap: 6,
          fontSize: 12,
          color: 'var(--color-text-faint)',
          marginBottom: 'var(--space-5)',
        }}
      >
        <AlertCircle size={13} />
        The API has no dedicated "my rides" endpoint, so this list is built by scanning
        recent rides for ones you're driving — a documented frontend workaround.
      </p>

      <div className={styles.tabs}>
        {TABS.map(({ key, label }) => (
          <button
            key={key}
            type="button"
            className={`${styles.tab} ${tab === key ? styles.tabActive : ''}`}
            onClick={() => setTab(key)}
          >
            {label}
          </button>
        ))}
      </div>

      <Card padded={false} style={{ padding: '0 var(--space-5)' }}>
        {rides === null && !error && (
          <div style={{ padding: 'var(--space-5) 0' }}>
            <LoadingState count={3} />
          </div>
        )}
        {error && <ErrorState message={error.message} correlationId={error.correlationId} onRetry={load} />}
        {rides && filtered.length === 0 && (
          <EmptyState title="No rides here" subtitle="Rides you offer as a driver show up in this list." />
        )}
        {filtered.map((ride) => (
          <RideCard key={ride.id} ride={ride} />
        ))}
      </Card>
    </div>
  )
}
