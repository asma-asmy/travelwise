import type { WorkflowResult } from '@/types/workflows'
import { StatusBadge } from '@/components/StatusBadge'

interface Props {
  result: WorkflowResult | null
}

export function RiskBudgetReadinessSummary({ result }: Props) {
  if (!result) return null

  return (
    <div
      style={{
        display: 'grid',
        gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))',
        gap: '1rem',
      }}
    >
      {result.risk && (
        <div className="card">
          <div style={{ fontSize: '0.6875rem', textTransform: 'uppercase', color: 'var(--color-text-muted)' }}>
            Risk
          </div>
          <div style={{ marginTop: '0.25rem', marginBottom: '0.5rem' }}>
            <StatusBadge status={result.risk.overall_risk_level ?? '—'} />
          </div>
          <ul style={{ paddingLeft: '1rem', fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
            {(result.risk.disruptions ?? []).slice(0, 3).map((d, i) => (
              <li key={i}>
                [{d.type}] {d.description}
              </li>
            ))}
          </ul>
        </div>
      )}

      {result.budget && (
        <div className="card">
          <div style={{ fontSize: '0.6875rem', textTransform: 'uppercase', color: 'var(--color-text-muted)' }}>
            Budget
          </div>
          <div style={{ marginTop: '0.25rem', marginBottom: '0.5rem' }}>
            <StatusBadge status={result.budget.health_status ?? '—'} />
          </div>
          <dl style={{ fontSize: '0.8125rem', display: 'grid', gridTemplateColumns: '1fr auto', gap: '0.25rem' }}>
            <dt style={{ color: 'var(--color-text-muted)' }}>Total</dt>
            <dd style={{ margin: 0, textAlign: 'right' }}>{result.budget.total_budget ?? '—'}</dd>
            <dt style={{ color: 'var(--color-text-muted)' }}>Spent</dt>
            <dd style={{ margin: 0, textAlign: 'right' }}>{result.budget.total_spent ?? '—'}</dd>
            <dt style={{ color: 'var(--color-text-muted)' }}>Remaining</dt>
            <dd style={{ margin: 0, textAlign: 'right', fontWeight: 600 }}>{result.budget.remaining_budget ?? '—'}</dd>
          </dl>
        </div>
      )}

      {result.readiness && (
        <div className="card">
          <div style={{ fontSize: '0.6875rem', textTransform: 'uppercase', color: 'var(--color-text-muted)' }}>
            Readiness
          </div>
          <div style={{ fontSize: '1.5rem', fontWeight: 700, marginTop: '0.25rem' }}>
            {result.readiness.readiness_score ?? 0}
            <span style={{ fontSize: '0.75rem', fontWeight: 400, color: 'var(--color-text-muted)' }}> / 100</span>
          </div>
          {result.readiness.blocking && result.readiness.blocking.length > 0 && (
            <ul style={{ paddingLeft: '1rem', fontSize: '0.75rem', color: 'var(--color-danger)', marginTop: '0.25rem' }}>
              {result.readiness.blocking.map((b, i) => (
                <li key={i}>{b}</li>
              ))}
            </ul>
          )}
        </div>
      )}
    </div>
  )
}