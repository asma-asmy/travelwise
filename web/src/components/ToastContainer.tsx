import { CheckCircle2, AlertTriangle, Info, X } from 'lucide-react'
import { useToastStore, type ToastKind } from '@/stores/toastStore'

const ICONS = {
  success: CheckCircle2,
  error: AlertTriangle,
  info: Info,
} as const

const COLOURS: Record<ToastKind, { bg: string; border: string; fg: string }> = {
  success: { bg: '#f0fdf4', border: '#86efac', fg: '#166534' },
  error: { bg: '#fef2f2', border: '#fca5a5', fg: '#991b1b' },
  info: { bg: '#eff6ff', border: '#93c5fd', fg: '#1e40af' },
}

export function ToastContainer() {
  const toasts = useToastStore((s) => s.toasts)
  const dismiss = useToastStore((s) => s.dismiss)

  if (toasts.length === 0) return null

  return (
    <div
      style={{
        position: 'fixed',
        top: '1rem',
        right: '1rem',
        zIndex: 9999,
        display: 'flex',
        flexDirection: 'column',
        gap: '0.5rem',
        maxWidth: '380px',
      }}
      role="region"
      aria-label="Notifications"
      aria-live="polite"
    >
      {toasts.map((t) => {
        const Icon = ICONS[t.kind]
        const palette = COLOURS[t.kind]
        return (
          <div
            key={t.id}
            role="status"
            style={{
              display: 'flex',
              alignItems: 'flex-start',
              gap: '0.625rem',
              padding: '0.75rem 0.875rem',
              background: palette.bg,
              border: `1px solid ${palette.border}`,
              color: palette.fg,
              borderRadius: 'var(--radius-md)',
              boxShadow: 'var(--shadow-md)',
              animation: 'toast-in 0.18s ease-out',
            }}
          >
            <Icon size={18} strokeWidth={2} style={{ flexShrink: 0, marginTop: 1 }} aria-hidden="true" />
            <div style={{ flex: 1, fontSize: '0.875rem' }}>{t.message}</div>
            <button
              onClick={() => dismiss(t.id)}
              aria-label="Dismiss notification"
              style={{
                background: 'transparent',
                border: 'none',
                cursor: 'pointer',
                color: 'inherit',
                padding: 0,
                opacity: 0.6,
              }}
            >
              <X size={16} />
            </button>
          </div>
        )
      })}
    </div>
  )
}