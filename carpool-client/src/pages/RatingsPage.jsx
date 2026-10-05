import { Star } from 'lucide-react'
import { useEffect, useState } from 'react'
import { getRatingsForUser } from '../api/ratings'
import { Card } from '../components/common/Card'
import { EmptyState, ErrorState, LoadingState } from '../components/common/States'
import { RatingCard } from '../components/ratings/RatingCard'
import { useAuth } from '../hooks/useAuth'

export function RatingsPage() {
  const { user } = useAuth()
  const [ratings, setRatings] = useState(null)
  const [error, setError] = useState(null)

  function load() {
    setError(null)
    getRatingsForUser(user.id)
      .then(setRatings)
      .catch((err) => setError(err))
  }

  useEffect(load, [user.id])

  const average =
    ratings && ratings.length > 0 ? (ratings.reduce((sum, r) => sum + r.score, 0) / ratings.length).toFixed(1) : null

  return (
    <div>
      <h1 style={{ fontSize: 22, fontWeight: 800, marginBottom: 'var(--space-2)' }}>Ratings</h1>
      <p style={{ fontSize: 13.5, color: 'var(--color-text-muted)', marginBottom: 'var(--space-5)' }}>
        Ratings you've received as a driver.
      </p>

      {average && (
        <Card style={{ marginBottom: 'var(--space-5)', display: 'flex', alignItems: 'center', gap: 'var(--space-3)' }}>
          <Star size={28} fill="#f5b014" color="#f5b014" />
          <div>
            <div style={{ fontSize: 22, fontWeight: 800 }}>{average}</div>
            <div style={{ fontSize: 12, color: 'var(--color-text-muted)' }}>
              average from {ratings.length} rating{ratings.length === 1 ? '' : 's'}
            </div>
          </div>
        </Card>
      )}

      <Card padded={false} style={{ padding: '0 var(--space-5)' }}>
        {ratings === null && !error && (
          <div style={{ padding: 'var(--space-5) 0' }}>
            <LoadingState count={3} />
          </div>
        )}
        {error && <ErrorState message={error.message} correlationId={error.correlationId} onRetry={load} />}
        {ratings && ratings.length === 0 && (
          <EmptyState title="No ratings yet" subtitle="Ratings passengers leave after completed rides appear here." />
        )}
        {ratings?.map((rating) => (
          <RatingCard key={rating.id} rating={rating} />
        ))}
      </Card>
    </div>
  )
}
