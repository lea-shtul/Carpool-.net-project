import { Loader2 } from 'lucide-react'
import styles from './Button.module.css'

/**
 * variant: 'primary' | 'secondary' | 'ghost' | 'danger'
 * size: 'sm' | 'md' | 'lg'
 */
export function Button({
  variant = 'primary',
  size = 'md',
  fullWidth = false,
  isLoading = false,
  icon: Icon,
  children,
  className = '',
  disabled,
  type = 'button',
  ...rest
}) {
  const sizeClass = size === 'sm' ? styles.sm : size === 'lg' ? styles.lg : ''
  return (
    <button
      type={type}
      className={`${styles.btn} ${styles[variant]} ${sizeClass} ${fullWidth ? styles.fullWidth : ''} ${className}`}
      disabled={disabled || isLoading}
      {...rest}
    >
      {isLoading ? <Loader2 size={16} className="spin" /> : Icon ? <Icon size={16} /> : null}
      {children}
    </button>
  )
}
