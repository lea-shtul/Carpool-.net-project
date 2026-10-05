import { useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import { AuthLayout } from './AuthLayout'
import { Button } from '../components/common/Button'
import { FormField, TextInput } from '../components/common/FormField'
import { useAuth } from '../hooks/useAuth'
import { useToast } from '../hooks/useToast'

const EMPTY_FORM = { firstName: '', lastName: '', email: '', password: '', phoneNumber: '' }

export function RegisterPage() {
  const { register } = useAuth()
  const { showToast } = useToast()
  const navigate = useNavigate()
  const [form, setForm] = useState(EMPTY_FORM)
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  function set(key, value) {
    setForm((f) => ({ ...f, [key]: value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    setIsSubmitting(true)
    try {
      await register(form)
      showToast({ type: 'success', message: 'Account created — log in to continue.' })
      navigate('/login', { replace: true })
    } catch (err) {
      setError(err.message)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <AuthLayout title="Create your account" subtitle="Join Carpool to start sharing rides.">
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
        <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: 'var(--space-4)' }}>
          <FormField label="First name" htmlFor="reg-first">
            <TextInput
              id="reg-first"
              required
              maxLength={50}
              value={form.firstName}
              onChange={(e) => set('firstName', e.target.value)}
            />
          </FormField>
          <FormField label="Last name" htmlFor="reg-last">
            <TextInput
              id="reg-last"
              required
              maxLength={50}
              value={form.lastName}
              onChange={(e) => set('lastName', e.target.value)}
            />
          </FormField>
        </div>

        <FormField label="Email" htmlFor="reg-email">
          <TextInput
            id="reg-email"
            type="email"
            required
            autoComplete="email"
            value={form.email}
            onChange={(e) => set('email', e.target.value)}
          />
        </FormField>

        <FormField label="Phone number" htmlFor="reg-phone">
          <TextInput
            id="reg-phone"
            type="tel"
            required
            value={form.phoneNumber}
            onChange={(e) => set('phoneNumber', e.target.value)}
          />
        </FormField>

        <FormField label="Password" htmlFor="reg-password">
          <TextInput
            id="reg-password"
            type="password"
            required
            minLength={8}
            autoComplete="new-password"
            value={form.password}
            onChange={(e) => set('password', e.target.value)}
          />
        </FormField>

        {error && <p style={{ color: 'var(--color-danger-text)', fontSize: 13 }}>{error}</p>}

        <Button type="submit" fullWidth isLoading={isSubmitting}>
          Create Account
        </Button>
      </form>

      <p style={{ marginTop: 'var(--space-5)', fontSize: 13, color: 'var(--color-text-muted)' }}>
        Already have an account?{' '}
        <Link to="/login" style={{ color: 'var(--color-primary-600)', fontWeight: 600 }}>
          Log in
        </Link>
      </p>
    </AuthLayout>
  )
}
