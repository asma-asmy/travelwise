import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import * as readinessApi from '@/api/readiness'
import { queryKeys } from '@/hooks/queryKeys'
import type {
  CreateDocumentRequest,
  DocumentQuery,
  UpdateDocumentRequest,
} from '@/types/readiness'

export function useMyDocuments(query: DocumentQuery = {}) {
  return useQuery({
    queryKey: queryKeys.readiness.documents(query),
    queryFn: () => readinessApi.listMyDocuments(query),
  })
}

export function useAddMyDocument() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateDocumentRequest) => readinessApi.addMyDocument(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['readiness', 'documents'] }),
  })
}

export function useUpdateMyDocument() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateDocumentRequest }) =>
      readinessApi.updateMyDocument(id, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['readiness', 'documents'] }),
  })
}

export function useDeleteMyDocument() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => readinessApi.deleteMyDocument(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['readiness', 'documents'] }),
  })
}

export function useRequirements(destination?: string) {
  return useQuery({
    queryKey: queryKeys.readiness.requirements(destination),
    queryFn: () => readinessApi.listAllRequirements(destination),
  })
}

export function useTripReadiness(tripId: string | undefined) {
  return useQuery({
    queryKey: tripId ? queryKeys.readiness.tripAssessment(tripId) : ['readiness', 'none', 'trip'],
    queryFn: () => readinessApi.getLatestReadiness(tripId!),
    enabled: Boolean(tripId),
  })
}

export function useAssessReadiness(tripId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: () => readinessApi.assessReadiness(tripId),
    onSuccess: (data) => {
      qc.setQueryData(queryKeys.readiness.tripAssessment(tripId), data)
    },
  })
}