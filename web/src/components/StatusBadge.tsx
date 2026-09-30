interface Props {
  status: string
}

const MAP: Record<string, string> = {
  // Trip
  Draft: 'badge-neutral',
  Planned: 'badge-info',
  Active: 'badge-success',
  Completed: 'badge-success',
  Cancelled: 'badge-danger',

  // Workflow
  AwaitingApproval: 'badge-warning',
  ValidationFailed: 'badge-danger',
  AgentFailed: 'badge-danger',
  ExternalServiceFailed: 'badge-danger',
  SafeFailure: 'badge-danger',
  Rejected: 'badge-danger',
  RevisionRequired: 'badge-warning',
  Approved: 'badge-info',
  Committing: 'badge-info',

  // Health
  HEALTHY: 'badge-success',
  WATCH: 'badge-warning',
  AT_RISK: 'badge-danger',
  OVER: 'badge-danger',

  // Risk
  Low: 'badge-success',
  Medium: 'badge-warning',
  High: 'badge-danger',
  Critical: 'badge-danger',
}

export function StatusBadge({ status }: Props) {
  const cls = MAP[status] ?? 'badge-neutral'
  return <span className={`badge ${cls}`}>{status}</span>
}