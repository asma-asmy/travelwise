import { humanizeStatus, isInProgress } from '@/utils/workflowStatus'
import type { WorkflowStatus } from '@/types/workflows'

interface Props {
  status: WorkflowStatus
}

export function WorkflowStatusPill({ status }: Props) {
  const cls =
    status === 'AwaitingApproval' || status === 'RevisionRequired'
      ? 'badge-warning'
      : status === 'Completed' || status === 'Approved'
        ? 'badge-success'
        : status === 'ValidationFailed' ||
          status === 'AgentFailed' ||
          status === 'ExternalServiceFailed' ||
          status === 'SafeFailure' ||
          status === 'Rejected'
          ? 'badge-danger'
          : isInProgress(status)
            ? 'badge-info'
            : 'badge-neutral'

  return (
    <span className={`badge ${cls}`} style={{ display: 'inline-flex', alignItems: 'center', gap: '0.375rem' }}>
      {isInProgress(status) && (
        <span
          style={{
            width: 6,
            height: 6,
            borderRadius: '50%',
            background: 'currentColor',
            display: 'inline-block',
            animation: 'pulse 1.5s ease-in-out infinite',
          }}
        />
      )}
      {humanizeStatus(status)}
    </span>
  )
}