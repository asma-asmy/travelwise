import { apiClient } from '@/api/client'
import type {
  ApprovalRequest,
  StartWorkflowRequest,
  WorkflowListQuery,
  WorkflowResponse,
} from '@/types/workflows'
import type { PagedResult } from '@/types/common'

export async function startWorkflow(
  tripId: string,
  payload: StartWorkflowRequest,
): Promise<WorkflowResponse> {
  const { data } = await apiClient.post<WorkflowResponse>(
    `/api/v1/trips/${tripId}/workflows`,
    payload,
  )
  return data
}

export async function getWorkflow(id: string): Promise<WorkflowResponse> {
  const { data } = await apiClient.get<WorkflowResponse>(`/api/v1/workflows/${id}`)
  return data
}

export async function listWorkflows(
  query: WorkflowListQuery = {},
): Promise<PagedResult<WorkflowResponse>> {
  const { data } = await apiClient.get<PagedResult<WorkflowResponse>>('/api/v1/workflows', {
    params: query,
  })
  return data
}

export async function approveWorkflow(
  id: string,
  payload: { reason?: string | null },
): Promise<WorkflowResponse> {
  const body: ApprovalRequest = { decision: 'APPROVE', reason: payload.reason ?? null }
  const { data } = await apiClient.post<WorkflowResponse>(
    `/api/v1/workflows/${id}/approve`,
    body,
  )
  return data
}

export async function rejectWorkflow(
  id: string,
  payload: { reason?: string | null },
): Promise<WorkflowResponse> {
  const body: ApprovalRequest = { decision: 'REJECT', reason: payload.reason ?? null }
  const { data } = await apiClient.post<WorkflowResponse>(
    `/api/v1/workflows/${id}/reject`,
    body,
  )
  return data
}

export async function requestRevision(
  id: string,
  payload: { reason?: string | null },
): Promise<WorkflowResponse> {
  const body: ApprovalRequest = { decision: 'REVISION_REQUIRED', reason: payload.reason ?? null }
  const { data } = await apiClient.post<WorkflowResponse>(
    `/api/v1/workflows/${id}/request-revision`,
    body,
  )
  return data
}