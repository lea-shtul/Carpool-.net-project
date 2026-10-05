import { AlertTriangle, Inbox } from 'lucide-react'
import { Button } from './Button'
import styles from './States.module.css'

export function EmptyState({ icon: Icon = Inbox, title, subtitle, action }) {
  return (
    <div className={styles.state}>
      <Icon size={32} className={styles.icon} />
      {title && <div className={styles.title}>{title}</div>}
      {subtitle && <div className={styles.subtitle}>{subtitle}</div>}
      {action}
    </div>
  )
}

export function ErrorState({ message = 'Something went wrong.', correlationId, onRetry }) {
  return (
    <div className={styles.state}>
      <AlertTriangle size={32} className={`${styles.icon} ${styles.errorIcon}`} />
      <div className={styles.title}>{message}</div>
      {correlationId && <div className={styles.subtitle}>Ref: {correlationId}</div>}
      {onRetry && (
        <Button variant="secondary" size="sm" onClick={onRetry}>
          Try again
        </Button>
      )}
    </div>
  )
}

/** A row of skeleton cards shown while a list is loading. */
export function LoadingState({ count = 3 }) {
  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
      {Array.from({ length: count }).map((_, i) => (
        <div className={styles.skeletonCard} key={i}>
          <div className={styles.skeletonLine} style={{ width: '30%' }} />
          <div className={styles.skeletonLine} style={{ width: '70%' }} />
          <div className={styles.skeletonLine} style={{ width: '50%' }} />
        </div>
      ))}
    </div>
  )
}
