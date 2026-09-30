import type { WorkflowResult } from '@/types/workflows'
import { formatCurrency } from '@/utils/format'

interface Props {
  result: WorkflowResult | null
}

export function ProposalPanel({ result }: Props) {
  if (!result?.proposal) {
    return <p style={{ color: 'var(--color-text-muted)' }}>No proposal generated yet.</p>
  }

  const proposal = result.proposal
  const changes = proposal.changes ?? []
  const delta = proposal.estimated_cost_delta ?? 0

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
      <div className="card">
        <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.5rem' }}>
          <div style={{ fontWeight: 600 }}>Recovery Proposal</div>
          <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
            ID: {proposal.proposal_id?.slice(0, 8)}…
          </div>
        </div>

        <p style={{ fontSize: '0.875rem', marginBottom: '0.75rem' }}>
          {proposal.rationale_summary}
        </p>

        <div
          style={{
            display: 'grid',
            gridTemplateColumns: 'repeat(auto-fit, minmax(160px, 1fr))',
            gap: '0.75rem',
          }}
        >
          <div>
            <div style={{ fontSize: '0.6875rem', color: 'var(--color-text-muted)', textTransform: 'uppercase' }}>
              Estimated cost delta
            </div>
            <div style={{ fontWeight: 600 }}>
              {delta > 0 ? '+' : ''}
              {formatCurrency(delta, 'LKR')}
            </div>
          </div>
          <div>
            <div style={{ fontSize: '0.6875rem', color: 'var(--color-text-muted)', textTransform: 'uppercase' }}>
              Risk after recovery
            </div>
            <div style={{ fontWeight: 600 }}>{proposal.estimated_risk_after ?? '—'}</div>
          </div>
          <div>
            <div style={{ fontSize: '0.6875rem', color: 'var(--color-text-muted)', textTransform: 'uppercase' }}>
              Changes
            </div>
            <div style={{ fontWeight: 600 }}>{changes.length}</div>
          </div>
        </div>
      </div>

      {changes.length === 0 ? (
        <div className="card">
          <p style={{ color: 'var(--color-text-muted)' }}>
            No changes proposed. The workflow may have found no impacted activities.
          </p>
        </div>
      ) : (
        <div className="card" style={{ padding: 0 }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.8125rem' }}>
            <thead>
              <tr style={{ textAlign: 'left', background: '#f8fafc' }}>
                <th style={{ padding: '0.5rem 0.75rem', borderBottom: '1px solid var(--color-border)' }}>Type</th>
                <th style={{ padding: '0.5rem 0.75rem', borderBottom: '1px solid var(--color-border)' }}>Activity</th>
                <th style={{ padding: '0.5rem 0.75rem', borderBottom: '1px solid var(--color-border)' }}>New activity</th>
                <th style={{ padding: '0.5rem 0.75rem', borderBottom: '1px solid var(--color-border)' }}>New date</th>
                <th style={{ padding: '0.5rem 0.75rem', borderBottom: '1px solid var(--color-border)' }}>Reason</th>
              </tr>
            </thead>
            <tbody>
              {changes.map((c, i) => (
                <tr key={i} style={{ borderBottom: '1px solid var(--color-border)' }}>
                  <td style={{ padding: '0.5rem 0.75rem' }}>
                    <span className="badge badge-info">{c.change_type}</span>
                  </td>
                  <td style={{ padding: '0.5rem 0.75rem', fontFamily: 'monospace', fontSize: '0.75rem' }}>
                    {c.activity_id?.slice(0, 8) ?? '—'}
                  </td>
                  <td style={{ padding: '0.5rem 0.75rem', fontFamily: 'monospace', fontSize: '0.75rem' }}>
                    {c.new_activity_id?.slice(0, 8) ?? '—'}
                  </td>
                  <td style={{ padding: '0.5rem 0.75rem' }}>{c.new_date ?? '—'}</td>
                  <td style={{ padding: '0.5rem 0.75rem' }}>{c.reason}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </div>
  )
}