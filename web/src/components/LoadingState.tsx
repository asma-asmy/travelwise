interface Props {
  message?: string
}

export function LoadingState({ message = 'Loading…' }: Props) {
  return (
    <div
      style={{
        padding: '3rem 1rem',
        textAlign: 'center',
        color: 'var(--color-text-muted)',
      }}
      role="status"
      aria-live="polite"
    >
      <svg
        width="28"
        height="28"
        viewBox="0 0 24 24"
        fill="none"
        style={{ animation: 'spin 1s linear infinite', marginBottom: '0.5rem' }}
        aria-hidden="true"
      >
        <circle cx="12" cy="12" r="10" stroke="#e2e8f0" strokeWidth="4" />
        <path d="M22 12a10 10 0 0 0-10-10" stroke="#2563eb" strokeWidth="4" strokeLinecap="round" />
      </svg>
      <div style={{ fontSize: '0.875rem' }}>{message}</div>
    </div>
  )
}