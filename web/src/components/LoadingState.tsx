interface Props {
  message?: string
}

export function LoadingState({ message = 'Loading…' }: Props) {
  return (
    <div style={{ padding: '2rem', textAlign: 'center', color: 'var(--color-text-muted)' }}>
      {message}
    </div>
  )
}