import { Car } from 'lucide-react'
import styles from './DrivingCar.module.css'

/** Purely decorative — a little car driving back and forth along the sidebar's bottom. */
export function DrivingCar() {
  return (
    <div className={styles.road} aria-hidden="true">
      <Car size={20} className={styles.car} strokeWidth={2.25} />
    </div>
  )
}
