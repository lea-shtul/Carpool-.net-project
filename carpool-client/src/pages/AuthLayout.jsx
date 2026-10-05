import { Car } from 'lucide-react'
import styles from './AuthLayout.module.css'

export function AuthLayout({ title, subtitle, children }) {
  return (
    <div className={styles.shell}>
      <div className={styles.brandPanel}>
        <div className={styles.brand}>
          <div className={styles.brandIconWrap}>
            <Car size={24} />
          </div>
          <span className={styles.brandName}>Carpool</span>
        </div>
        <div className={styles.heroTitle}>Share a ride. Save money. Meet people.</div>
        <div className={styles.heroSubtitle}>
          Travel together, reduce costs, and build connections — find or offer a ride in
          minutes.
        </div>
      </div>
      <div className={styles.formPanel}>
        <div className={styles.formCard}>
          <div className={styles.title}>{title}</div>
          {subtitle && <div className={styles.subtitle}>{subtitle}</div>}
          {children}
        </div>
      </div>
    </div>
  )
}
