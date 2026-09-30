import { Link } from 'react-router'

export function NotFoundPage() {
  return (
    <div className="container" style={{ paddingTop: '4rem', textAlign: 'center' }}>
      <h1 style={{ fontSize: '2rem', marginBottom: '0.5rem' }}>404</h1>
      <p style={{ marginBottom: '1rem' }}>Page not found.</p>
      <Link to="/dashboard">Back to dashboard</Link>
    </div>
  )
}