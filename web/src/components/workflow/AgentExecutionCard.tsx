import type { AgentExecution } from '@/types/workflows'
import { formatDuration, formatDateTime, formatJson } from '@/utils/format'

interface Props {
  execution: AgentExecution
  index: number
}

export function AgentExecutionCard({ execution, index }: Props) {
  const ok = execution.status.toLowerCase() === 'succeeded'
  const failed = execution.status.toLowerCase() === 'failed'

  return (
    <div className="card" style={{ padding: '0.875rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', gap: '1rem' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', minWidth: 0 }}>
          <div
            style={{
              width: 28,
              height: 28,
              borderRadius: '50%',
              background: ok ? 'var(--color-success)' : failed ? 'var(--color-danger)' : 'var(--color-text-muted)',
              color: 'white',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontSize: '0.75rem',
              fontWeight: 700,
              flexShrink: 0,
            }}
          >
            {index + 1}
          </div>
          <div style={{ minWidth: 0 }}>
            <div style={{ fontWeight: 600, fontSize: '0.875rem' }}>{execution.agentName}</div>
            <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
              {execution.taskName}
            </div>
          </div>
        </div>
        <div style={{ textAlign: 'right', fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
          <div>{execution.status}</div>
          <div>{formatDuration(execution.durationMs)}</div>
        </div>
      </div>

      {execution.errorMessage && (
        <div
          style={{
            marginTop: '0.5rem',
            padding: '0.5rem',
            background: '#fee2e2',
            color: '#991b1b',
            fontSize: '0.75rem',
            borderRadius: 'var(--radius-sm)',
          }}
        >
          {execution.errorMessage}
        </div>
      )}

      {execution.outputJson && (
        <details style={{ marginTop: '0.5rem' }}>
          <summary style={{ cursor: 'pointer', fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
            View output
          </summary>
          <pre
            style={{
              marginTop: '0.5rem',
              background: '#0f172a',
              color: '#e2e8f0',
              padding: '0.5rem',
              borderRadius: 'var(--radius-sm)',
              fontSize: '0.6875rem',
              overflow: 'auto',
              maxHeight: '240px',
            }}
          >
            {formatJson(execution.outputJson)}
          </pre>
        </details>
      )}

      {execution.completedAt && (
        <div style={{ marginTop: '0.5rem', fontSize: '0.6875rem', color: 'var(--color-text-muted)' }}>
          Completed {formatDateTime(execution.completedAt)}
        </div>
      )}
    </div>
  )
}