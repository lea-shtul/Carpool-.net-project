import { Link } from 'react-router-dom'
import { Button } from '../components/common/Button'
import { EmptyState } from '../components/common/States'

export function NotFoundPage() {
  return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
      <EmptyState
        title="Page not found"
        subtitle="The page you're looking for doesn't exist."
        action={
          <Link to="/dashboard">
            <Button size="sm">Back to Dashboard</Button>
          </Link>
        }
      />
    </div>
  )
}
