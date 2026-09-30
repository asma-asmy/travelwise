import { X } from 'lucide-react'

interface Props {
  open: boolean
  title: string
  onClose: () => void
  children: React.ReactNode
  footer?: React.ReactNode
  maxWidth?: string
}

export function Modal({ open, title, onClose, children, footer, maxWidth = '520px' }: Props) {
  if (!open) return null
  return (
    <div
      style={{
        position: 'fixed',
        inset: 0,
        background: 'rgba(15, 23, 42, 0.5)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        zIndex: 200,
        padding: '1rem',
        overflow: 'auto',
      }}
      onClick={onClose}
    >
      <div
        className="card"
        style={{ maxWidth, width: '100%', padding: '1.25rem', maxHeight: '90vh', overflow: 'auto' }}
        onClick={(e) => e.stopPropagation()}
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
          <h3 style={{ fontSize: '1.125rem', fontWeight: 600 }}>{title}</h3>
          <button onClick={onClose} className="btn btn-secondary" style={{ padding: '0.25rem' }} aria-label="Close">
            <X size={16} />
          </button>
        </div>
        {children}
        {footer && <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end', marginTop: '1rem' }}>{footer}</div>}
      </div>
    </div>
  )
}