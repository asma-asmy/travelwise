interface Props {
  title: string
  description?: string
  action?: React.ReactNode
}

export function EmptyState({ title, description, action }: Props) {
  return (
    <div className="card" style={{ padding: '2rem', textAlign: 'center' }}>
      <h3 style={{ fontSize: '1.125rem', marginBottom: '0.25rem' }}>{title}</h3>
      {description && (
        <p style={{ color: 'var(--color-text-muted)', marginBottom: '1rem', fontSize: '0.875rem' }}>
          {description}
        </p>
      )}
      {action}
    </div>
  )
}