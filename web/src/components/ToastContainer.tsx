import { useToastStore } from '@/stores/toastStore'

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
      }}
    >
      {toasts.map((t) => (
        <div
          key={t.id}
          className={
            t.kind === 'success'
              ? 'card badge-success'
              : t.kind === 'error'
                ? 'card badge-danger'
                : 'card badge-info'
          }
          style={{
            minWidth: '260px',
            padding: '0.75rem 1rem',
            cursor: 'pointer',
            borderRadius: 'var(--radius-md)',
          }}
          onClick={() => dismiss(t.id)}
        >
          {t.message}
        </div>
      ))}
    </div>
  )
}