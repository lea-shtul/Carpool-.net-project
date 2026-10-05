import { Card } from '../components/common/Card'
import { CreateRideForm } from '../components/rides/CreateRideForm'

export function CreateRidePage() {
  return (
    <div>
      <h1 style={{ fontSize: 22, fontWeight: 800, marginBottom: 'var(--space-2)' }}>Offer a Ride</h1>
      <p style={{ fontSize: 13.5, color: 'var(--color-text-muted)', marginBottom: 'var(--space-5)' }}>
        Share your trip with passengers heading the same way.
      </p>
      <Card>
        <CreateRideForm />
      </Card>
    </div>
  )
}
