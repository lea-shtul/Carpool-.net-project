import {
  Car,
  CarFront,
  CalendarCheck,
  HelpCircle,
  LayoutDashboard,
  MessageSquare,
  Route,
  Search,
  Settings,
  ShieldCheck,
  Star,
  UserCircle,
} from 'lucide-react'
import { NavLink } from 'react-router-dom'
import { useAuth } from '../../hooks/useAuth'
import { DrivingCar } from './DrivingCar'
import styles from './Sidebar.module.css'

const PRIMARY_LINKS = [
  { to: '/dashboard', label: 'Dashboard', icon: LayoutDashboard },
  { to: '/rides', label: 'Find Rides', icon: Search, end: true },
  { to: '/my-rides', label: 'My Rides', icon: Route },
  { to: '/bookings', label: 'My Bookings', icon: CalendarCheck },
  { to: '/vehicles', label: 'My Vehicles', icon: CarFront },
  { to: '/ratings', label: 'Ratings', icon: Star },
  { to: '/messages', label: 'Messages', icon: MessageSquare },
]

const SECONDARY_LINKS = [
  { to: '/profile', label: 'Profile', icon: UserCircle },
  { to: '/settings', label: 'Settings', icon: Settings },
  { to: '/help', label: 'Help & Support', icon: HelpCircle },
]

export function Sidebar({ isOpen, onNavigate }) {
  const { user } = useAuth()

  return (
    <aside className={`${styles.sidebar} ${isOpen ? styles.sidebarOpen : ''}`}>
      <div className={styles.brand}>
        <div className={styles.brandIconWrap}>
          <Car size={18} />
        </div>
        <span className={styles.brandName}>Carpool</span>
      </div>
      <div className={styles.tagline}>Ride Together, Save More</div>

      <nav className={styles.nav}>
        {PRIMARY_LINKS.map(({ to, label, icon: Icon, end }) => (
          <NavLink
            key={to}
            to={to}
            end={end}
            onClick={onNavigate}
            className={({ isActive }) => `${styles.navLink} ${isActive ? styles.navLinkActive : ''}`}
          >
            <Icon size={17} />
            {label}
          </NavLink>
        ))}

        {user?.role === 'Admin' && (
          <NavLink
            to="/admin/users"
            onClick={onNavigate}
            className={({ isActive }) => `${styles.navLink} ${isActive ? styles.navLinkActive : ''}`}
          >
            <ShieldCheck size={17} />
            Admin
          </NavLink>
        )}
      </nav>

      <div className={styles.divider} />

      <nav className={styles.nav}>
        {SECONDARY_LINKS.map(({ to, label, icon: Icon }) => (
          <NavLink
            key={to}
            to={to}
            onClick={onNavigate}
            className={({ isActive }) => `${styles.navLink} ${isActive ? styles.navLinkActive : ''}`}
          >
            <Icon size={17} />
            {label}
          </NavLink>
        ))}
      </nav>

      <div className={styles.spacer} />

      <DrivingCar />

      {user && (
        <div className={styles.roleCard}>
          Your role: <span className={styles.roleValue}>{user.role}</span>
        </div>
      )}
    </aside>
  )
}
