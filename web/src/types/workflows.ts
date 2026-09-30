export type WorkflowStatus =
  | 'Created' | 'Planning' | 'Delegating' | 'AgentsRunning'
  | 'Validating' | 'ValidationFailed' | 'AwaitingApproval'
  | 'Approved' | 'Committing' | 'Completed'
  | 'Rejected' | 'RevisionRequired' | 'AgentFailed'
  | 'ExternalServiceFailed' | 'SafeFailure'

export interface WorkflowResponse {
  id: string
  tripId: string
  objective: string
  status: WorkflowStatus
  currentStep: string | null
  createdAt: string
  completedAt: string | null
  failureReason: string | null
  plan: unknown
  result: WorkflowResult | null
  agentExecutions: AgentExecution[]
  toolCalls: ToolCall[]
  validations: ValidationResult[]
  approvals: ApprovalDecision[]
}

export interface WorkflowResult {
  proposal?: {
    proposal_id?: string
    changes?: RecoveryChange[]
    estimated_cost_delta?: number
    estimated_risk_after?: string
    rationale_summary?: string
  }
  risk?: {
    overall_risk_level?: string
    disruptions?: Array<{ type: string; severity: string; description: string }>
    impacted_activity_ids?: string[]
  }
  budget?: {
    remaining_budget?: number
    total_budget?: number
    total_spent?: number
    health_status?: string
    fits_proposal?: boolean
  }
  readiness?: {
    readiness_score?: number
    blocking?: string[]
    eligible_for_proposal?: boolean
  }
  validation?: {
    passed?: boolean
    violations?: Array<{ rule: string; severity: string; details: string }>
    checked_rules?: string[]
  }
}

export interface RecoveryChange {
  change_type: 'RESCHEDULE' | 'SUBSTITUTE' | 'REMOVE' | 'ADD'
  activity_id?: string | null
  new_activity_id?: string | null
  new_date?: string | null
  new_start_time?: string | null
  reason: string
}

export interface AgentExecution {
  id: string
  agentName: string
  taskName: string
  status: string
  durationMs: number | null
  outputJson: string | null
  errorMessage: string | null
  startedAt: string | null
  completedAt: string | null
}

export interface ToolCall {
  id: string
  toolName: string
  inputJson: string
  outputJson: string | null
  status: string
  durationMs: number | null
  errorMessage: string | null
}

export interface ValidationResult {
  id: string
  validatorName: string
  passed: boolean
  detailsJson: string
  createdAt: string
}

export interface ApprovalDecision {
  id: string
  decidedByUserId: string
  decision: string
  reason: string | null
  decidedAt: string
}

export interface StartWorkflowRequest {
  objective: string
  disruptionHint?: string | null
}

export interface ApprovalRequest {
  decision: 'APPROVE' | 'REJECT' | 'REVISION_REQUIRED'
  reason?: string | null
}

export interface WorkflowListQuery {
  status?: WorkflowStatus
  page?: number
  pageSize?: number
}