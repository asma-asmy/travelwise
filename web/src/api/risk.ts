import { apiClient } from '@/api/client'
import type {
  CreateSafetyAlertRequest,
  DestinationRisk,
  RiskAssessment,
  SafetyAlert,
  SafetyAlertQuery,
  SafetyInformation,
  UpdateSafetyAlertRequest,
  WeatherSnapshot,
} from '@/types/risk'
import type { PagedResult } from '@/types/common'

export async function listSafetyAlerts(
  query: SafetyAlertQuery = {},
): Promise<PagedResult<SafetyAlert>> {
  const { data } = await apiClient.get<PagedResult<SafetyAlert>>('/api/v1/alerts', {
    params: query,
  })
  return data
}

export async function createSafetyAlert(
  payload: CreateSafetyAlertRequest,
): Promise<SafetyAlert> {
  const { data } = await apiClient.post<SafetyAlert>('/api/v1/alerts', payload)
  return data
}

export async function updateSafetyAlert(
  id: string,
  payload: UpdateSafetyAlertRequest,
): Promise<SafetyAlert> {
  const { data } = await apiClient.put<SafetyAlert>(`/api/v1/alerts/${id}`, payload)
  return data
}

export async function deleteSafetyAlert(id: string): Promise<void> {
  await apiClient.delete(`/api/v1/alerts/${id}`)
}

export async function getDestinationRisk(destination: string): Promise<DestinationRisk> {
  const { data } = await apiClient.get<DestinationRisk>(
    `/api/v1/destinations/${encodeURIComponent(destination)}/risk`,
  )
  return data
}

export async function getWeather(
  destination: string,
  date: string,
): Promise<WeatherSnapshot | null> {
  const { data, status } = await apiClient.get<WeatherSnapshot | ''>(
    `/api/v1/destinations/${encodeURIComponent(destination)}/weather`,
    { params: { date }, validateStatus: (s) => s === 200 || s === 204 },
  )
  if (status === 204 || !data) return null
  return data as WeatherSnapshot
}

export async function getSafetyInformation(destination: string): Promise<SafetyInformation> {
  const { data } = await apiClient.get<SafetyInformation>(
    `/api/v1/destinations/${encodeURIComponent(destination)}/safety`,
  )
  return data
}

export async function getLatestRiskAssessment(tripId: string): Promise<RiskAssessment | null> {
  const { data, status } = await apiClient.get<RiskAssessment | ''>(
    `/api/v1/trips/${tripId}/risk`,
    { validateStatus: (s) => s === 200 || s === 204 },
  )
  if (status === 204 || !data) return null
  return data as RiskAssessment
}

export async function assessTrip(tripId: string): Promise<RiskAssessment> {
  const { data } = await apiClient.post<RiskAssessment>(`/api/v1/trips/${tripId}/risk/assess`)
  return data
}