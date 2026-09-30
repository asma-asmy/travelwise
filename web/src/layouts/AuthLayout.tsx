import { Outlet } from 'react-router'

export function AuthLayout() {
  return (
    <div
      style={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'var(--color-bg)',
      }}
    >
      <div className="card" style={{ width: '100%', maxWidth: '420px', padding: '2rem' }}>
        <div style={{ marginBottom: '1.5rem', textAlign: 'center' }}>
          <h1 style={{ fontSize: '1.5rem' }}>TravelWise</h1>
          <p style={{ color: 'var(--color-text-muted)', fontSize: '0.875rem' }}>
            Intelligent Travel Resilience Platform
          </p>
        </div>
        <Outlet />
      </div>
    </div>
  )
}