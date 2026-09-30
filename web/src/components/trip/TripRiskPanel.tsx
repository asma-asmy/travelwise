import { LoadingState } from '@/components/LoadingState'
import { EmptyState } from '@/components/EmptyState'
import { StatusBadge } from '@/components/StatusBadge'
import { useTripRisk, useAssessTripRisk, useSafetyInformation } from '@/hooks/useRisk'
import { useToastStore } from '@/stores/toastStore'
import { formatDateTime } from '@/utils/format'

interface Props {
  tripId: string
  destination: string
}

export function TripRiskPanel({ tripId, destination }: Props) {
  const risk = useTripRisk(tripId)
  const safety = useSafetyInformation(destination)
  const assess = useAssessTripRisk(tripId)
  const push = useToastStore((s) => s.push)

  async function handleAssess() {
    try {
      await assess.mutateAsync()
      push('success', 'Risk assessment updated.')
    } catch {
      push('error', 'Failed to assess risk.')
    }
  }

  if (risk.isLoading) return <LoadingState />

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h3 style={{ fontSize: '1rem', fontWeight: 600 }}>Risk Assessment</h3>
        <button className="btn btn-primary" onClick={handleAssess} disabled={assess.isPending}>
          {assess.isPending ? 'Assessing…' : 'Re-assess'}
        </button>
      </div>

      {!risk.data ? (
        <EmptyState title="No risk assessment yet" description="Run an assessment to see current risk." />
      ) : (
        <div className="card">
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '0.75rem' }}>
            <StatusBadge status={risk.data.overallLevelName} />
            <span style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
              Assessed {formatDateTime(risk.data.assessedAt)}
            </span>
          </div>

          {risk.data.factors.length > 0 && (
            <ul style={{ paddingLeft: '1.25rem', fontSize: '0.8125rem', marginBottom: '0.75rem' }}>
              {risk.data.factors.map((f, i) => (
                <li key={i}>
                  [{f.type}] {f.description}
                </li>
              ))}
            </ul>
          )}

          {risk.data.recommendations.length > 0 && (
            <div>
              <div style={{ fontWeight: 600, fontSize: '0.8125rem', marginBottom: '0.25rem' }}>Recommendations</div>
              <ul style={{ paddingLeft: '1.25rem', fontSize: '0.8125rem', color: 'var(--color-warning)' }}>
                {risk.data.recommendations.map((r, i) => (
                  <li key={i}>{r}</li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}

      {safety.data && safety.data.activeAlerts.length > 0 && (
        <div className="card">
          <h4 style={{ fontSize: '0.875rem', fontWeight: 600, marginBottom: '0.5rem' }}>Active Alerts</h4>
          <ul style={{ listStyle: 'none', margin: 0, padding: 0 }}>
            {safety.data.activeAlerts.map((a) => (
              <li
                key={a.id}
                style={{
                  padding: '0.5rem 0',
                  borderBottom: '1px solid var(--color-border)',
                  display: 'flex',
                  justifyContent: 'space-between',
                  alignItems: 'center',
                  gap: '1rem',
                }}
              >
                <div>
                  <div style={{ fontSize: '0.875rem' }}>{a.message}</div>
                  <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>{a.category}</div>
                </div>
                <StatusBadge status={a.severityName} />
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  )
}