import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, Navigate, useLocation, useNavigate } from 'react-router'
import { useAuthStore } from '@/stores/authStore'

export function LoginPage() {
  const navigate = useNavigate()
  const location = useLocation()
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const login = useAuthStore((s) => s.login)

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  if (isAuthenticated) {
    const dest = (location.state as { from?: string } | null)?.from ?? '/dashboard'
    return <Navigate to={dest} replace />
  }

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      await login({ email: email.trim(), password })
      navigate('/dashboard', { replace: true })
    } catch (err: unknown) {
      const status = (err as { response?: { status?: number } })?.response?.status
      if (status === 401) setError('Invalid email or password.')
      else setError('Login failed. Please try again.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <h2 style={{ marginBottom: '1rem', fontSize: '1.25rem' }}>Sign in</h2>

      {error && (
        <div
          className="badge badge-danger"
          style={{ display: 'block', marginBottom: '1rem', padding: '0.5rem' }}
        >
          {error}
        </div>
      )}

      <div style={{ marginBottom: '1rem' }}>
        <label htmlFor="email" style={{ display: 'block', marginBottom: '0.25rem' }}>
          Email
        </label>
        <input
          id="email"
          type="email"
          required
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          style={{
            width: '100%',
            padding: '0.5rem',
            border: '1px solid var(--color-border)',
            borderRadius: 'var(--radius-sm)',
          }}
        />
      </div>

      <div style={{ marginBottom: '1.5rem' }}>
        <label htmlFor="password" style={{ display: 'block', marginBottom: '0.25rem' }}>
          Password
        </label>
        <input
          id="password"
          type="password"
          required
          value={password}
          onChange={(e) => setPassword(e.target.value)}
          style={{
            width: '100%',
            padding: '0.5rem',
            border: '1px solid var(--color-border)',
            borderRadius: 'var(--radius-sm)',
          }}
        />
      </div>

      <button
        type="submit"
        className="btn btn-primary"
        disabled={submitting}
        style={{ width: '100%', justifyContent: 'center' }}
      >
        {submitting ? 'Signing in…' : 'Sign in'}
      </button>

      <p style={{ marginTop: '1rem', textAlign: 'center', fontSize: '0.875rem' }}>
        No account? <Link to="/register">Create one</Link>
      </p>
    </form>
  )
}