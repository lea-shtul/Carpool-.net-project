import { useState } from 'react'
import { Link, useLocation, useNavigate } from 'react-router-dom'
import { AuthLayout } from './AuthLayout'
import { Button } from '../components/common/Button'
import { FormField, TextInput } from '../components/common/FormField'
import { useAuth } from '../hooks/useAuth'

export function LoginPage() {
  const { login } = useAuth()
  const navigate = useNavigate()
  const location = useLocation()
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    setIsSubmitting(true)
    try {
      await login(email, password)
      const redirectTo = location.state?.from?.pathname || '/dashboard'
      navigate(redirectTo, { replace: true })
    } catch (err) {
      setError(err.message)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthLayout title="Welcome back" subtitle="Log in to find or offer a ride.">
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
        <FormField label="Email" htmlFor="login-email">
          <TextInput
            id="login-email"
            type="email"
            required
            autoComplete="email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
        </FormField>
        <FormField label="Password" htmlFor="login-password">
          <TextInput
            id="login-password"
            type="password"
            required
            autoComplete="current-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
        </FormField>

        {error && <p style={{ color: 'var(--color-danger-text)', fontSize: 13 }}>{error}</p>}

        <Button type="submit" fullWidth isLoading={isSubmitting}>
          Log In
        </Button>
      </form>

      <p style={{ marginTop: 'var(--space-5)', fontSize: 13, color: 'var(--color-text-muted)' }}>
        Don't have an account?{' '}
        <Link to="/register" style={{ color: 'var(--color-primary-600)', fontWeight: 600 }}>
          Create one
        </Link>
      </p>
    </AuthLayout>
  )
}
