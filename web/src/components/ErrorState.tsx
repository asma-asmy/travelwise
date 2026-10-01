import { AlertTriangle, RefreshCw } from 'lucide-react'

interface Props {
  message?: string
  onRetry?: () => void
}

export function ErrorState({ message = 'Something went wrong.', onRetry }: Props) {
  return (
    <div
      className="card"
      style={{
        padding: '3rem 1.5rem',
        textAlign: 'center',
        display: 'flex',
        flexDirection: 'column',
        alignItems: 'center',
        gap: '0.75rem',
      }}
      role="alert"
    >
      <div
        style={{
          width: 56,
          height: 56,
          borderRadius: '50%',
          background: '#fee2e2',
          color: 'var(--color-danger)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
        }}
        aria-hidden="true"
      >
        <AlertTriangle size={26} strokeWidth={1.5} />
      </div>
      <h3 style={{ fontSize: '1.0625rem', fontWeight: 600, margin: 0 }}>Something went wrong</h3>
      <p
        style={{
          color: 'var(--color-text-muted)',
          fontSize: '0.875rem',
          maxWidth: '420px',
          margin: 0,
        }}
      >
        {message}
      </p>
      {onRetry && (
        <button
          className="btn btn-secondary"
          onClick={onRetry}
          style={{ marginTop: '0.5rem' }}
        >
          <RefreshCw size={14} />
          Try again
        </button>
      )}
    </div>
  )
}