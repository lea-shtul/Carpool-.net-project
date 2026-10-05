import { Construction } from 'lucide-react'
import { Card } from '../components/common/Card'
import { EmptyState } from '../components/common/States'

/**
 * Used for nav items the reference image includes but the backend has no API for
 * (Messages, Settings) — spec §16: keep the navigation entry, show a clean placeholder,
 * don't pretend the feature exists.
 */
export function PlaceholderPage({ title, description }) {
  return (
    <div>
      <h1 style={{ fontSize: 22, fontWeight: 800, marginBottom: 'var(--space-5)' }}>{title}</h1>
      <Card>
        <EmptyState
          icon={Construction}
          title="Not available yet"
          subtitle={description}
        />
      </Card>
    </div>
  )
}
