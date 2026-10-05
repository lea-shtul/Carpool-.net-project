import { CheckCircle2, X, XCircle, Info } from 'lucide-react'
import { useToast } from '../../hooks/useToast'
import styles from './Toast.module.css'

const ICONS = {
  success: CheckCircle2,
  error: XCircle,
  info: Info,
}

export function ToastViewport() {
  const { toasts, dismissToast } = useToast()

  if (toasts.length === 0) return null

  return (
    <div className={styles.viewport} role="status" aria-live="polite">
      {toasts.map((toast) => {
        const Icon = ICONS[toast.type] || Info
        return (
          <div key={toast.id} className={`${styles.toast} ${styles[toast.type]}`}>
            <Icon size={18} className={styles.icon} />
            <div className={styles.body}>
              <div className={styles.message}>{toast.message}</div>
              {toast.correlationId && (
                <div className={styles.correlationId}>Ref: {toast.correlationId}</div>
              )}
            </div>
            <button
              type="button"
              className={styles.closeBtn}
              onClick={() => dismissToast(toast.id)}
              aria-label="Dismiss"
            >
              <X size={14} />
            </button>
          </div>
        )
      })}
    </div>
  )
}
