import styles from './Badge.module.css'

/** tone: 'success' | 'warning' | 'danger' | 'info' | 'neutral' | 'global' | 'private' */
export function Badge({ tone = 'neutral', icon: Icon, children, className = '' }) {
  return (
    <span className={`${styles.badge} ${styles[tone]} ${className}`}>
      {Icon && <Icon size={12} />}
      {children}
    </span>
  )
}

const RIDE_STATUS_TONE = {
  Scheduled: 'info',
  InProgress: 'warning',
  Completed: 'success',
  Cancelled: 'danger',
}

const BOOKING_STATUS_TONE = {
  Active: 'success',
  Cancelled: 'danger',
  Completed: 'neutral',
}

export function RideStatusBadge({ status }) {
  return <Badge tone={RIDE_STATUS_TONE[status] || 'neutral'}>{status}</Badge>
}

export function BookingStatusBadge({ status }) {
  return <Badge tone={BOOKING_STATUS_TONE[status] || 'neutral'}>{status}</Badge>
}
