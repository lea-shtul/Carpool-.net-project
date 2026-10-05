import styles from './FormField.module.css'

export function FormField({ label, error, children, htmlFor }) {
  return (
    <div className={styles.field}>
      {label && (
        <label className={styles.label} htmlFor={htmlFor}>
          {label}
        </label>
      )}
      {children}
      {error && <span className={styles.error}>{error}</span>}
    </div>
  )
}

export function TextInput({ error, className = '', ...rest }) {
  return (
    <input className={`${styles.input} ${error ? styles.inputError : ''} ${className}`} {...rest} />
  )
}

export function Select({ error, className = '', children, ...rest }) {
  return (
    <select className={`${styles.select} ${error ? styles.inputError : ''} ${className}`} {...rest}>
      {children}
    </select>
  )
}

export function Textarea({ error, className = '', ...rest }) {
  return (
    <textarea className={`${styles.textarea} ${error ? styles.inputError : ''} ${className}`} {...rest} />
  )
}
