import { useState } from 'react'
import { Link } from 'react-router'
import { PageHeader } from '@/components/PageHeader'
import { ErrorState } from '@/components/ErrorState'
import { EmptyState } from '@/components/EmptyState'
import { WorkflowStatusPill } from '@/components/WorkflowStatusPill'
import { useWorkflows } from '@/hooks/useWorkflows'
import { formatRelative } from '@/utils/format'
import { humanizeStatus } from '@/utils/workflowStatus'
import type { WorkflowStatus } from '@/types/workflows'
import { SkeletonTable } from '@/components/SkeletonTable'

const STATUS_OPTIONS: WorkflowStatus[] = [
  'AwaitingApproval',
  'AgentsRunning',
  'Validating',
  'Completed',
  'Rejected',
  'ValidationFailed',
  'SafeFailure',
]

export function WorkflowsListPage() {
  const [status, setStatus] = useState<WorkflowStatus | ''>('AwaitingApproval')
  const [page, setPage] = useState(1)
  const [pageSize] = useState(20)

  const { data, isLoading, isError, refetch, isFetching } = useWorkflows({
    status: status || undefined,
    page,
    pageSize,
  })

  return (
    <>
      <PageHeader
        title="Workflows"
        subtitle="Recovery workflows submitted by travellers and pending your decision."
        actions={
          <button className="btn btn-secondary" onClick={() => refetch()} disabled={isFetching}>
            {isFetching ? 'Refreshing…' : 'Refresh'}
          </button>
        }
      />

      <div className="card" style={{ marginBottom: '1rem' }}>
        <div style={{ display: 'flex', gap: '0.75rem', alignItems: 'center', flexWrap: 'wrap' }}>
          <label htmlFor="status-filter" style={{ fontSize: '0.875rem' }}>
            Status
          </label>
          <select
            id="status-filter"
            value={status}
            onChange={(e) => {
              setStatus(e.target.value as WorkflowStatus | '')
              setPage(1)
            }}
            style={{
              padding: '0.375rem 0.5rem',
              border: '1px solid var(--color-border)',
              borderRadius: 'var(--radius-sm)',
            }}
          >
            <option value="">All statuses</option>
            {STATUS_OPTIONS.map((s) => (
              <option key={s} value={s}>
                {humanizeStatus(s)}
              </option>
            ))}
          </select>
        </div>
      </div>

      {isLoading && <SkeletonTable rows={5} columns={4} />}
      {isError && <ErrorState message="Failed to load workflows." onRetry={() => refetch()} />}
      {data && data.items.length === 0 && (
        <EmptyState
          title="No workflows match this filter"
          description="Try a different status, or check back later."
        />
      )}
      {data && data.items.length > 0 && (
        <>
          <div className="card" style={{ padding: 0, overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.875rem' }}>
              <thead>
                <tr style={{ textAlign: 'left', background: '#f8fafc' }}>
                  <th style={{ padding: '0.5rem 0.75rem', borderBottom: '1px solid var(--color-border)' }}>Objective</th>
                  <th style={{ padding: '0.5rem 0.75rem', borderBottom: '1px solid var(--color-border)' }}>Status</th>
                  <th style={{ padding: '0.5rem 0.75rem', borderBottom: '1px solid var(--color-border)' }}>Created</th>
                  <th style={{ padding: '0.5rem 0.75rem', borderBottom: '1px solid var(--color-border)' }}></th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((w) => (
                  <tr key={w.id} style={{ borderBottom: '1px solid var(--color-border)' }}>
                    <td style={{ padding: '0.625rem 0.75rem' }}>{w.objective}</td>
                    <td style={{ padding: '0.625rem 0.75rem' }}>
                      <WorkflowStatusPill status={w.status} />
                    </td>
                    <td style={{ padding: '0.625rem 0.75rem', color: 'var(--color-text-muted)' }}>
                      {formatRelative(w.createdAt)}
                    </td>
                    <td style={{ padding: '0.625rem 0.75rem', textAlign: 'right' }}>
                      <Link to={`/workflows/${w.id}`} className="btn btn-secondary">
                        Open
                      </Link>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          <div
            style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              marginTop: '0.75rem',
              fontSize: '0.875rem',
            }}
          >
            <div style={{ color: 'var(--color-text-muted)' }}>
              Page {data.page} of {data.totalPages || 1} — {data.totalCount} total
            </div>
            <div style={{ display: 'flex', gap: '0.5rem' }}>
              <button
                className="btn btn-secondary"
                disabled={page <= 1}
                onClick={() => setPage((p) => Math.max(1, p - 1))}
              >
                Previous
              </button>
              <button
                className="btn btn-secondary"
                disabled={page >= (data.totalPages || 1)}
                onClick={() => setPage((p) => p + 1)}
              >
                Next
              </button>
            </div>
          </div>
        </>
      )}
    </>
  )
}