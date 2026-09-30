import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import * as workflowsApi from '@/api/workflows'
import { queryKeys } from '@/hooks/queryKeys'
import type { StartWorkflowRequest, WorkflowListQuery } from '@/types/workflows'

export function useWorkflows(query: WorkflowListQuery = {}) {
  return useQuery({
    queryKey: queryKeys.workflows.list(query),
    queryFn: () => workflowsApi.listWorkflows(query),
    // Refresh the approval queue every 10 seconds while on the page
    refetchInterval: 10_000,
  })
}

export function useWorkflow(id: string | undefined) {
  return useQuery({
    queryKey: id ? queryKeys.workflows.detail(id) : ['workflows', 'none'],
    queryFn: () => workflowsApi.getWorkflow(id!),
    enabled: Boolean(id),
    refetchInterval: (query) => {
      const status = (query.state.data as { status?: string } | undefined)?.status
      // Poll every 5s while the workflow is in a running state
      const runningStates = ['Created', 'Planning', 'Delegating', 'AgentsRunning', 'Validating', 'Committing']
      return status && runningStates.includes(status) ? 5_000 : false
    },
  })
}

export function useStartWorkflow(tripId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: StartWorkflowRequest) => workflowsApi.startWorkflow(tripId, payload),
    onSuccess: (data) => {
      qc.setQueryData(queryKeys.workflows.detail(data.id), data)
      qc.invalidateQueries({ queryKey: ['workflows', 'list'] })
    },
  })
}

export function useApproveWorkflow() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason?: string | null }) =>
      workflowsApi.approveWorkflow(id, { reason }),
    onSuccess: (data) => {
      qc.setQueryData(queryKeys.workflows.detail(data.id), data)
      qc.invalidateQueries({ queryKey: ['workflows', 'list'] })
    },
  })
}

export function useRejectWorkflow() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason?: string | null }) =>
      workflowsApi.rejectWorkflow(id, { reason }),
    onSuccess: (data) => {
      qc.setQueryData(queryKeys.workflows.detail(data.id), data)
      qc.invalidateQueries({ queryKey: ['workflows', 'list'] })
    },
  })
}

export function useRequestRevision() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, reason }: { id: string; reason?: string | null }) =>
      workflowsApi.requestRevision(id, { reason }),
    onSuccess: (data) => {
      qc.setQueryData(queryKeys.workflows.detail(data.id), data)
      qc.invalidateQueries({ queryKey: ['workflows', 'list'] })
    },
  })
}