import { LoadingState } from '@/components/LoadingState'
import { EmptyState } from '@/components/EmptyState'
import { useTripReadiness, useAssessReadiness } from '@/hooks/useReadiness'
import { useToastStore } from '@/stores/toastStore'
import { formatDateTime } from '@/utils/format'

interface Props {
  tripId: string
}

export function TripReadinessPanel({ tripId }: Props) {
  const readiness = useTripReadiness(tripId)
  const assess = useAssessReadiness(tripId)
  const push = useToastStore((s) => s.push)

  async function handleAssess() {
    try {
      await assess.mutateAsync()
      push('success', 'Readiness assessed.')
    } catch {
      push('error', 'Failed to assess readiness.')
    }
  }

  if (readiness.isLoading) return <LoadingState />

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <h3 style={{ fontSize: '1rem', fontWeight: 600 }}>Travel Readiness</h3>
        <button className="btn btn-primary" onClick={handleAssess} disabled={assess.isPending}>
          {assess.isPending ? 'Assessing…' : 'Re-assess'}
        </button>
      </div>

      {!readiness.data ? (
        <EmptyState title="No readiness assessment yet" />
      ) : (
        <div className="card">
          <div style={{ fontSize: '2rem', fontWeight: 700, marginBottom: '0.5rem' }}>
            {readiness.data.score}
            <span style={{ fontSize: '1rem', fontWeight: 400, color: 'var(--color-text-muted)' }}> / 100</span>
          </div>
          <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)', marginBottom: '0.75rem' }}>
            Assessed {formatDateTime(readiness.data.assessedAt)}
          </div>

          {readiness.data.blockingIssues.length > 0 && (
            <div style={{ marginBottom: '0.75rem' }}>
              <div style={{ fontWeight: 600, fontSize: '0.8125rem', color: 'var(--color-danger)', marginBottom: '0.25rem' }}>
                Blocking issues
              </div>
              <ul style={{ paddingLeft: '1.25rem', fontSize: '0.8125rem' }}>
                {readiness.data.blockingIssues.map((b, i) => (
                  <li key={i}>{b}</li>
                ))}
              </ul>
            </div>
          )}

          {readiness.data.warnings.length > 0 && (
            <div>
              <div style={{ fontWeight: 600, fontSize: '0.8125rem', color: 'var(--color-warning)', marginBottom: '0.25rem' }}>
                Warnings
              </div>
              <ul style={{ paddingLeft: '1.25rem', fontSize: '0.8125rem' }}>
                {readiness.data.warnings.map((w, i) => (
                  <li key={i}>{w}</li>
                ))}
              </ul>
            </div>
          )}
        </div>
      )}
    </div>
  )
}