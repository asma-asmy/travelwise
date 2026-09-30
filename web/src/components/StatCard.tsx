import type { LucideIcon } from 'lucide-react'

interface Props {
  label: string
  value: number | string
  icon: LucideIcon
  tone?: 'default' | 'success' | 'warning' | 'danger' | 'info'
  hint?: string
}

const TONES: Record<NonNullable<Props['tone']>, { bg: string; fg: string }> = {
  default: { bg: '#f1f5f9', fg: '#334155' },
  success: { bg: '#dcfce7', fg: '#166534' },
  warning: { bg: '#ffedd5', fg: '#9a3412' },
  danger:  { bg: '#fee2e2', fg: '#991b1b' },
  info:    { bg: '#cffafe', fg: '#155e75' },
}

export function StatCard({ label, value, icon: Icon, tone = 'default', hint }: Props) {
  const palette = TONES[tone]
  return (
    <div className="card" style={{ display: 'flex', alignItems: 'center', gap: '1rem' }}>
      <div
        style={{
          width: '44px',
          height: '44px',
          borderRadius: 'var(--radius-md)',
          background: palette.bg,
          color: palette.fg,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          flexShrink: 0,
        }}
      >
        <Icon size={20} />
      </div>
      <div style={{ minWidth: 0 }}>
        <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)', textTransform: 'uppercase', letterSpacing: '0.05em' }}>
          {label}
        </div>
        <div style={{ fontSize: '1.5rem', fontWeight: 700, lineHeight: 1.1 }}>{value}</div>
        {hint && (
          <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>{hint}</div>
        )}
      </div>
    </div>
  )
}