import { useState } from 'react'
import { Outlet } from 'react-router-dom'
import { Sidebar } from './Sidebar'
import { TopBar } from './TopBar'
import styles from './AppLayout.module.css'

export function AppLayout() {
  const [isMobileNavOpen, setIsMobileNavOpen] = useState(false)

  return (
    <div className={styles.shell}>
      <Sidebar isOpen={isMobileNavOpen} onNavigate={() => setIsMobileNavOpen(false)} />
      {isMobileNavOpen && (
        <div
          className={`${styles.backdrop} ${styles.backdropVisible}`}
          onClick={() => setIsMobileNavOpen(false)}
        />
      )}
      <div className={styles.mainColumn}>
        <TopBar onMenuClick={() => setIsMobileNavOpen((v) => !v)} />
        <main className={styles.content}>
          <Outlet />
        </main>
      </div>
    </div>
  )
}
