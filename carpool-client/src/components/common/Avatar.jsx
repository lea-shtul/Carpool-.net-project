import styles from './Avatar.module.css'

function initials(firstName = '', lastName = '') {
  return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase() || '?'
}

export function Avatar({ firstName, lastName, size = 40 }) {
  return (
    <div
      className={styles.avatar}
      style={{ width: size, height: size, fontSize: Math.round(size * 0.4) }}
    >
      {initials(firstName, lastName)}
    </div>
  )
}
