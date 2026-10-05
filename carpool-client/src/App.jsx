import { Navigate, Route, Routes } from 'react-router-dom'
import { AppLayout } from './components/layout/AppLayout'
import { ToastViewport } from './components/common/Toast'
import { AuthProvider } from './context/AuthContext'
import { ToastProvider } from './context/ToastContext'
import { AdminUsersPage } from './pages/AdminUsersPage'
import { CreateRidePage } from './pages/CreateRidePage'
import { DashboardPage } from './pages/DashboardPage'
import { FindRidesPage } from './pages/FindRidesPage'
import { HelpPage } from './pages/HelpPage'
import { LoginPage } from './pages/LoginPage'
import { MyBookingsPage } from './pages/MyBookingsPage'
import { MyRidesPage } from './pages/MyRidesPage'
import { MyVehiclesPage } from './pages/MyVehiclesPage'
import { NotFoundPage } from './pages/NotFoundPage'
import { PlaceholderPage } from './pages/PlaceholderPage'
import { ProfilePage } from './pages/ProfilePage'
import { RatingsPage } from './pages/RatingsPage'
import { RegisterPage } from './pages/RegisterPage'
import { RideDetailsPage } from './pages/RideDetailsPage'
import { AdminRoute } from './routes/AdminRoute'
import { GuestRoute } from './routes/GuestRoute'
import { ProtectedRoute } from './routes/ProtectedRoute'

export default function App() {
  return (
    <AuthProvider>
      <ToastProvider>
        <Routes>
          <Route path="/" element={<Navigate to="/dashboard" replace />} />

          <Route element={<GuestRoute />}>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />
          </Route>

          <Route element={<ProtectedRoute />}>
            <Route element={<AppLayout />}>
              <Route path="/dashboard" element={<DashboardPage />} />
              <Route path="/rides" element={<FindRidesPage />} />
              <Route path="/rides/new" element={<CreateRidePage />} />
              <Route path="/rides/:id" element={<RideDetailsPage />} />
              <Route path="/my-rides" element={<MyRidesPage />} />
              <Route path="/bookings" element={<MyBookingsPage />} />
              <Route path="/vehicles" element={<MyVehiclesPage />} />
              <Route path="/ratings" element={<RatingsPage />} />
              <Route path="/profile" element={<ProfilePage />} />
              <Route
                path="/messages"
                element={
                  <PlaceholderPage
                    title="Messages"
                    description="Direct messaging isn't implemented in the current backend — there's no messaging API yet."
                  />
                }
              />
              <Route
                path="/settings"
                element={
                  <PlaceholderPage
                    title="Settings"
                    description="Account settings beyond your profile aren't implemented in the current backend yet."
                  />
                }
              />
              <Route path="/help" element={<HelpPage />} />

              <Route element={<AdminRoute />}>
                <Route path="/admin/users" element={<AdminUsersPage />} />
              </Route>
            </Route>
          </Route>

          <Route path="*" element={<NotFoundPage />} />
        </Routes>
        <ToastViewport />
      </ToastProvider>
    </AuthProvider>
  )
}
