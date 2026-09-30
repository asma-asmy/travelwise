import { apiClient } from '@/api/client'
import type {
  CreateDocumentRequest,
  CreateDocumentRequirementRequest,
  DocumentQuery,
  DocumentRequirement,
  ReadinessAssessment,
  TravelDocument,
  UpdateDocumentRequest,
} from '@/types/readiness'
import type { PagedResult } from '@/types/common'

export async function listMyDocuments(
  query: DocumentQuery = {},
): Promise<PagedResult<TravelDocument>> {
  const { data } = await apiClient.get<PagedResult<TravelDocument>>(
    '/api/v1/users/me/documents',
    { params: query },
  )
  return data
}

export async function addMyDocument(payload: CreateDocumentRequest): Promise<TravelDocument> {
  const { data } = await apiClient.post<TravelDocument>('/api/v1/users/me/documents', payload)
  return data
}

export async function updateMyDocument(
  id: string,
  payload: UpdateDocumentRequest,
): Promise<TravelDocument> {
  const { data } = await apiClient.put<TravelDocument>(
    `/api/v1/users/me/documents/${id}`,
    payload,
  )
  return data
}

export async function deleteMyDocument(id: string): Promise<void> {
  await apiClient.delete(`/api/v1/users/me/documents/${id}`)
}

export async function listRequirementsForDestination(
  destination: string,
): Promise<DocumentRequirement[]> {
  const { data } = await apiClient.get<DocumentRequirement[]>(
    `/api/v1/destinations/${encodeURIComponent(destination)}/requirements`,
  )
  return data
}

export async function listAllRequirements(destination?: string): Promise<DocumentRequirement[]> {
  const { data } = await apiClient.get<DocumentRequirement[]>('/api/v1/requirements', {
    params: destination ? { destination } : undefined,
  })
  return data
}

export async function createRequirement(
  payload: CreateDocumentRequirementRequest,
): Promise<DocumentRequirement> {
  const { data } = await apiClient.post<DocumentRequirement>('/api/v1/requirements', payload)
  return data
}

export async function deleteRequirement(id: string): Promise<void> {
  await apiClient.delete(`/api/v1/requirements/${id}`)
}

export async function getLatestReadiness(tripId: string): Promise<ReadinessAssessment | null> {
  const { data, status } = await apiClient.get<ReadinessAssessment | ''>(
    `/api/v1/trips/${tripId}/readiness`,
    { validateStatus: (s) => s === 200 || s === 204 },
  )
  if (status === 204 || !data) return null
  return data as ReadinessAssessment
}

export async function assessReadiness(tripId: string): Promise<ReadinessAssessment> {
  const { data } = await apiClient.post<ReadinessAssessment>(
    `/api/v1/trips/${tripId}/readiness/assess`,
  )
  return data
}