import { Card } from '../components/common/Card'

const FAQS = [
  {
    q: 'How do I book a ride?',
    a: 'Go to Find Rides, pick a ride, open its details and choose the number of seats to book.',
  },
  {
    q: 'What happens if a seat is taken while I\'m booking?',
    a: 'Seats are a limited resource, so a last-moment conflict is possible. You\'ll see a clear message and a way to refresh the ride instead of a generic error.',
  },
  {
    q: 'How do I offer a ride?',
    a: 'Add a vehicle under My Vehicles first, then use "Offer a Ride" from the dashboard or My Rides.',
  },
  {
    q: 'Can I rate a driver?',
    a: 'After a ride is marked Completed, you can rate it once from My Bookings or the ride\'s details page.',
  },
  {
    q: 'What are private tags?',
    a: 'Beyond the standard shared tags (Quiet, Music, ...), you can create your own private tags when offering a ride. Only you can select them, but once attached they\'re visible to anyone viewing that ride.',
  },
]

export function HelpPage() {
  return (
    <div>
      <h1 style={{ fontSize: 22, fontWeight: 800, marginBottom: 'var(--space-5)' }}>Help & Support</h1>
      <Card>
        {FAQS.map((item, i) => (
          <div key={item.q} style={{ padding: 'var(--space-4) 0', borderBottom: i < FAQS.length - 1 ? '1px solid var(--color-border)' : 'none' }}>
            <div style={{ fontWeight: 700, fontSize: 14, marginBottom: 4 }}>{item.q}</div>
            <div style={{ fontSize: 13, color: 'var(--color-text-muted)' }}>{item.a}</div>
          </div>
        ))}
      </Card>
    </div>
  )
}
