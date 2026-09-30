import { useState } from 'react'
import { X } from 'lucide-react'

interface Props {
  open: boolean
  action: 'APPROVE' | 'REJECT' | 'REVISION_REQUIRED'
  onClose: () => void
  onConfirm: (reason: string) => void
  submitting: boolean
}

const TITLES = {
  APPROVE: 'Approve workflow',
  REJECT: 'Reject workflow',
  REVISION_REQUIRED: 'Request revision',
}

const DESCRIPTIONS = {
  APPROVE: 'Approving will commit the proposed changes to the traveller\'s itinerary.',
  REJECT: 'Rejecting will close this workflow without applying any changes.',
  REVISION_REQUIRED: 'Requesting revision will send the workflow back to the agents for a new proposal.',
}

export function ApprovalActionDialog({ open, action, onClose, onConfirm, submitting }: Props) {
  const [reason, setReason] = useState('')

  if (!open) return null

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault()
    onConfirm(reason.trim())
  }

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
      }}
      onClick={onClose}
    >
      <div
        className="card"
        style={{ maxWidth: '480px', width: '100%', padding: '1.25rem' }}
        onClick={(e) => e.stopPropagation()}
      >
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem' }}>
          <h3 style={{ fontSize: '1.125rem', fontWeight: 600 }}>{TITLES[action]}</h3>
          <button
            onClick={onClose}
            aria-label="Close"
            className="btn btn-secondary"
            style={{ padding: '0.25rem' }}
          >
            <X size={16} />
          </button>
        </div>

        <p style={{ fontSize: '0.875rem', color: 'var(--color-text-muted)', marginBottom: '1rem' }}>
          {DESCRIPTIONS[action]}
        </p>

        <form onSubmit={handleSubmit}>
          <label htmlFor="reason" style={{ display: 'block', fontSize: '0.875rem', marginBottom: '0.25rem' }}>
            Reason {action === 'APPROVE' ? '(optional)' : '(recommended)'}
          </label>
          <textarea
            id="reason"
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            rows={3}
            maxLength={1000}
            style={{
              width: '100%',
              padding: '0.5rem',
              border: '1px solid var(--color-border)',
              borderRadius: 'var(--radius-sm)',
              fontFamily: 'inherit',
              fontSize: '0.875rem',
            }}
          />

          <div style={{ display: 'flex', gap: '0.5rem', justifyContent: 'flex-end', marginTop: '1rem' }}>
            <button type="button" className="btn btn-secondary" onClick={onClose} disabled={submitting}>
              Cancel
            </button>
            <button
              type="submit"
              className={
                action === 'APPROVE' ? 'btn btn-primary' : action === 'REJECT' ? 'btn btn-danger' : 'btn btn-primary'
              }
              disabled={submitting}
            >
              {submitting ? 'Submitting…' : TITLES[action]}
            </button>
          </div>
        </form>
      </div>
    </div>
  )
}