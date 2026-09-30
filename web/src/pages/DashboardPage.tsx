import { useAuthStore } from '@/stores/authStore'

export function DashboardPage() {
  const fullName = useAuthStore((s) => s.fullName)
  const roles = useAuthStore((s) => s.roles)

  return (
    <div>
      <h1 style={{ fontSize: '1.5rem', marginBottom: '0.5rem' }}>
        Welcome, {fullName}
      </h1>
      <p style={{ color: 'var(--color-text-muted)' }}>
        Signed in as <strong>{roles.join(', ')}</strong>.
      </p>
      <p style={{ marginTop: '1rem' }}>Dashboard widgets coming in Phase 8E.</p>
    </div>
  )
}