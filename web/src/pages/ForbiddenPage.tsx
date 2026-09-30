import { Link } from 'react-router'

export function ForbiddenPage() {
  return (
    <div className="container" style={{ paddingTop: '4rem', textAlign: 'center' }}>
      <h1 style={{ fontSize: '2rem', marginBottom: '0.5rem' }}>403</h1>
      <p style={{ marginBottom: '1rem' }}>You do not have permission to view this page.</p>
      <Link to="/dashboard">Back to dashboard</Link>
    </div>
  )
}