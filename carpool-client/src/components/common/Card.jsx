import styles from './Card.module.css'

export function Card({ padded = true, className = '', children, ...rest }) {
  return (
    <div className={`${styles.card} ${padded ? styles.padded : ''} ${className}`} {...rest}>
      {children}
    </div>
  )
}
