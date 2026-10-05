import { LogOut, Menu } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import { Avatar } from '../common/Avatar'
import { useAuth } from '../../hooks/useAuth'
import styles from './TopBar.module.css'

export function TopBar({ onMenuClick }) {
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  function handleLogout() {
    logout()
    navigate('/login', { replace: true })
  }

  return (
    <header className={styles.topbar}>
      <button type="button" className={styles.menuBtn} onClick={onMenuClick} aria-label="Open menu">
        <Menu size={20} />
      </button>

      {user && (
        <div className={styles.userCluster}>
          <div className={styles.userInfo}>
            <div className={styles.userName}>
              {user.firstName} {user.lastName}
            </div>
            <div className={styles.userRole}>{user.role}</div>
          </div>
          <Avatar firstName={user.firstName} lastName={user.lastName} />
          <button type="button" className={styles.logoutBtn} onClick={handleLogout} aria-label="Log out">
            <LogOut size={16} />
          </button>
        </div>
      )}
    </header>
  )
}
