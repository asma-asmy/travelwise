interface Props {
  message?: string
  onRetry?: () => void
}

export function ErrorState({ message = 'Something went wrong.', onRetry }: Props) {
  return (
    <div className="card" style={{ padding: '1.5rem', textAlign: 'center' }}>
      <p style={{ color: 'var(--color-danger)', marginBottom: '0.75rem' }}>{message}</p>
      {onRetry && (
        <button className="btn btn-secondary" onClick={onRetry}>
          Retry
        </button>
      )}
    </div>
  )
}