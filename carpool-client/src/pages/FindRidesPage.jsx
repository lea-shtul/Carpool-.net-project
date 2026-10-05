import { useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { getTags } from '../api/tags'
import { searchRides } from '../api/rides'
import { Card } from '../components/common/Card'
import { Pagination } from '../components/common/Pagination'
import { EmptyState, ErrorState, LoadingState } from '../components/common/States'
import { RideCard } from '../components/rides/RideCard'
import { RideFilters } from '../components/rides/RideFilters'

function filtersFromSearchParams(params) {
  return {
    origin: params.get('origin') || '',
    destination: params.get('destination') || '',
    tagId: params.get('tagId') || '',
    minPrice: params.get('minPrice') || '',
    maxPrice: params.get('maxPrice') || '',
    availableOnly: params.get('availableOnly') === 'true',
    sortBy: params.get('sortBy') || 'departureTime',
    sortDirection: params.get('sortDirection') || 'asc',
    page: Number(params.get('page') || 1),
    pageSize: 10,
  }
}

export function FindRidesPage() {
  const [searchParams, setSearchParams] = useSearchParams()
  const urlFilters = useMemo(() => filtersFromSearchParams(searchParams), [searchParams])

  const [formFilters, setFormFilters] = useState(urlFilters)
  const [tags, setTags] = useState([])
  const [result, setResult] = useState(null)
  const [error, setError] = useState(null)

  useEffect(() => {
    getTags().then(setTags).catch(() => setTags([]))
  }, [])

  useEffect(() => {
    setResult(null)
    setError(null)
    searchRides(urlFilters)
      .then(setResult)
      .catch((err) => setError(err))
  }, [urlFilters])

  // Keep the filter form in sync if the user navigates back/forward.
  useEffect(() => {
    setFormFilters(urlFilters)
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [searchParams])

  function applyFilters() {
    const params = new URLSearchParams()
    Object.entries(formFilters).forEach(([key, value]) => {
      if (value !== '' && value !== false && value !== null && value !== undefined) {
        params.set(key, String(value))
      }
    })
    params.set('page', '1')
    setSearchParams(params)
  }

  function goToPage(page) {
    const params = new URLSearchParams(searchParams)
    params.set('page', String(page))
    setSearchParams(params)
  }

  return (
    <div>
      <h1 style={{ fontSize: 22, fontWeight: 800, marginBottom: 'var(--space-5)' }}>Find Rides</h1>

      <Card style={{ marginBottom: 'var(--space-6)' }}>
        <RideFilters filters={formFilters} tags={tags} onChange={setFormFilters} onSubmit={applyFilters} />
      </Card>

      <Card padded={false} style={{ padding: '0 var(--space-5)' }}>
        {result === null && !error && (
          <div style={{ padding: 'var(--space-5) 0' }}>
            <LoadingState count={4} />
          </div>
        )}
        {error && <ErrorState message={error.message} correlationId={error.correlationId} />}
        {result && result.items.length === 0 && (
          <EmptyState title="No rides match your search" subtitle="Try widening your filters." />
        )}
        {result?.items.map((ride) => (
          <RideCard key={ride.id} ride={ride} />
        ))}
        {result && result.items.length > 0 && (
          <Pagination
            page={result.page}
            totalPages={result.totalPages}
            totalCount={result.totalCount}
            pageSize={result.pageSize}
            onPageChange={goToPage}
          />
        )}
      </Card>
    </div>
  )
}
