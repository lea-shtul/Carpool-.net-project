import { Mail, Phone, ShieldCheck, Calendar } from 'lucide-react'
import { Avatar } from '../components/common/Avatar'
import { Badge } from '../components/common/Badge'
import { Card } from '../components/common/Card'
import { useAuth } from '../hooks/useAuth'
import { formatDate } from '../utils/format'

function Row({ icon: Icon, label, value }) {
  return (
    <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-3)', padding: 'var(--space-3) 0' }}>
      <Icon size={16} color="var(--color-text-faint)" />
      <div>
        <div style={{ fontSize: 11, color: 'var(--color-text-faint)', textTransform: 'uppercase' }}>{label}</div>
        <div style={{ fontSize: 14, fontWeight: 600 }}>{value}</div>
      </div>
    </div>
  )
}

export function ProfilePage() {
  const { user } = useAuth()

  return (
    <div>
      <h1 style={{ fontSize: 22, fontWeight: 800, marginBottom: 'var(--space-5)' }}>Profile</h1>

      <Card style={{ maxWidth: 480 }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-4)', marginBottom: 'var(--space-5)' }}>
          <Avatar firstName={user.firstName} lastName={user.lastName} size={56} />
          <div>
            <div style={{ fontSize: 18, fontWeight: 800 }}>
              {user.firstName} {user.lastName}
            </div>
            <div style={{ display: 'flex', gap: 'var(--space-2)', marginTop: 4 }}>
              <Badge tone={user.role === 'Admin' ? 'info' : 'neutral'}>{user.role}</Badge>
              <Badge tone={user.isActive ? 'success' : 'danger'}>{user.isActive ? 'Active' : 'Inactive'}</Badge>
            </div>
          </div>
        </div>

        <Row icon={Mail} label="Email" value={user.email} />
        <Row icon={Phone} label="Phone" value={user.phoneNumber} />
        <Row icon={ShieldCheck} label="Role" value={user.role} />
        <Row icon={Calendar} label="Member since" value={formatDate(user.createdAt)} />
      </Card>
    </div>
  )
}
