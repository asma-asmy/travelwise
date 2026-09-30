import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link, Navigate, useNavigate } from 'react-router'
import { useAuthStore } from '@/stores/authStore'

export function RegisterPage() {
  const navigate = useNavigate()
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const register = useAuthStore((s) => s.register)

  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [fullName, setFullName] = useState('')
  const [phoneNumber, setPhoneNumber] = useState('')
  const [error, setError] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)

  if (isAuthenticated) return <Navigate to="/dashboard" replace />

  async function handleSubmit(e: FormEvent) {
    e.preventDefault()
    setError(null)
    setSubmitting(true)
    try {
      await register({
        email: email.trim(),
        password,
        fullName: fullName.trim(),
        phoneNumber: phoneNumber.trim() || null,
      })
      navigate('/dashboard', { replace: true })
    } catch (err: unknown) {
      const status = (err as { response?: { status?: number; data?: { title?: string } } })?.response?.status
      const title = (err as { response?: { data?: { title?: string } } })?.response?.data?.title
      if (status === 409) setError('An account with this email already exists.')
      else if (title) setError(title)
      else setError('Registration failed. Please check the inputs and try again.')
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit}>
      <h2 style={{ marginBottom: '1rem', fontSize: '1.25rem' }}>Create account</h2>

      {error && (
        <div
          className="badge badge-danger"
          style={{ display: 'block', marginBottom: '1rem', padding: '0.5rem' }}
        >
          {error}
        </div>
      )}

      <Field label="Full name" id="fullName" value={fullName} onChange={setFullName} required />
      <Field label="Email" id="email" type="email" value={email} onChange={setEmail} required />
      <Field
        label="Password"
        id="password"
        type="password"
        value={password}
        onChange={setPassword}
        required
        hint="At least 8 characters, with uppercase, lowercase, and a digit."
      />
      <Field label="Phone (optional)" id="phone" value={phoneNumber} onChange={setPhoneNumber} />

      <button
        type="submit"
        className="btn btn-primary"
        disabled={submitting}
        style={{ width: '100%', justifyContent: 'center' }}
      >
        {submitting ? 'Creating…' : 'Create account'}
      </button>

      <p style={{ marginTop: '1rem', textAlign: 'center', fontSize: '0.875rem' }}>
        Have an account? <Link to="/login">Sign in</Link>
      </p>
    </form>
  )
}

function Field({
  label,
  id,
  type = 'text',
  value,
  onChange,
  required,
  hint,
}: {
  label: string
  id: string
  type?: string
  value: string
  onChange: (v: string) => void
  required?: boolean
  hint?: string
}) {
  return (
    <div style={{ marginBottom: '1rem' }}>
      <label htmlFor={id} style={{ display: 'block', marginBottom: '0.25rem' }}>
        {label}
      </label>
      <input
        id={id}
        type={type}
        value={value}
        required={required}
        onChange={(e) => onChange(e.target.value)}
        style={{
          width: '100%',
          padding: '0.5rem',
          border: '1px solid var(--color-border)',
          borderRadius: 'var(--radius-sm)',
        }}
      />
      {hint && <small style={{ color: 'var(--color-text-muted)' }}>{hint}</small>}
    </div>
  )
}