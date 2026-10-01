import { Inbox } from 'lucide-react'

interface Props {
  title: string
  description?: string
  action?: React.ReactNode
  icon?: React.ComponentType<{ size?: number; strokeWidth?: number }>
}

export function EmptyState({ title, description, action, icon: Icon = Inbox }: Props) {
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
      role="status"
    >
      <div
        style={{
          width: 56,
          height: 56,
          borderRadius: '50%',
          background: '#f1f5f9',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          color: 'var(--color-text-muted)',
          marginBottom: '0.25rem',
        }}
        aria-hidden="true"
      >
        <Icon size={26} strokeWidth={1.5} />
      </div>
      <h3 style={{ fontSize: '1.0625rem', fontWeight: 600, margin: 0 }}>{title}</h3>
      {description && (
        <p
          style={{
            color: 'var(--color-text-muted)',
            fontSize: '0.875rem',
            maxWidth: '420px',
            margin: 0,
          }}
        >
          {description}
        </p>
      )}
      {action && <div style={{ marginTop: '0.5rem' }}>{action}</div>}
    </div>
  )
}