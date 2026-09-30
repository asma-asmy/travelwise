import type { WorkflowStatus } from '@/types/workflows'

export const TERMINAL_STATUSES: WorkflowStatus[] = [
  'Completed',
  'Rejected',
  'SafeFailure',
  'ValidationFailed',
  'AgentFailed',
  'ExternalServiceFailed',
]

export const IN_PROGRESS_STATUSES: WorkflowStatus[] = [
  'Created',
  'Planning',
  'Delegating',
  'AgentsRunning',
  'Validating',
  'Approved',
  'Committing',
]

export function isTerminal(status: WorkflowStatus): boolean {
  return TERMINAL_STATUSES.includes(status)
}

export function isInProgress(status: WorkflowStatus): boolean {
  return IN_PROGRESS_STATUSES.includes(status)
}

export function humanizeStatus(status: WorkflowStatus): string {
  const map: Record<WorkflowStatus, string> = {
    Created: 'Created',
    Planning: 'Planning',
    Delegating: 'Delegating',
    AgentsRunning: 'Agents Running',
    Validating: 'Validating',
    ValidationFailed: 'Validation Failed',
    AwaitingApproval: 'Awaiting Approval',
    Approved: 'Approved',
    Committing: 'Committing',
    Completed: 'Completed',
    Rejected: 'Rejected',
    RevisionRequired: 'Revision Required',
    AgentFailed: 'Agent Failed',
    ExternalServiceFailed: 'External Service Failed',
    SafeFailure: 'Safe Failure',
  }
  return map[status] ?? status
}