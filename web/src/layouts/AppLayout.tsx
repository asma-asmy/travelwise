import { Link, Outlet, useNavigate } from 'react-router'
import { useAuthStore } from '@/stores/authStore'

export function AppLayout() {
  const navigate = useNavigate()
  const fullName = useAuthStore((s) => s.fullName)
  const roles = useAuthStore((s) => s.roles)
  const logout = useAuthStore((s) => s.logout)

  const isStaff = roles.includes('TravelOpsStaff') || roles.includes('Admin')

  async function handleLogout() {
    await logout()
    navigate('/login', { replace: true })
  }

  return (
    <div style={{ minHeight: '100vh', display: 'flex', flexDirection: 'column' }}>
      <header
        style={{
          background: 'var(--color-surface)',
          borderBottom: '1px solid var(--color-border)',
          padding: '0.75rem 1rem',
        }}
      >
        <div
          className="container"
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
          }}
        >
          <Link to="/dashboard" style={{ fontWeight: 600, fontSize: '1.125rem' }}>
            TravelWise
          </Link>
          <nav style={{ display: 'flex', gap: '1rem', alignItems: 'center' }}>
            <Link to="/dashboard">Dashboard</Link>
            {isStaff && <Link to="/workflows">Workflows</Link>}
            <span style={{ color: 'var(--color-text-muted)', fontSize: '0.875rem' }}>
              {fullName} · {roles.join(', ')}
            </span>
            <button className="btn btn-secondary" onClick={handleLogout}>
              Logout
            </button>
          </nav>
        </div>
      </header>
      <main style={{ flex: 1, padding: '1.5rem 0' }}>
        <div className="container">
          <Outlet />
        </div>
      </main>
    </div>
  )
}