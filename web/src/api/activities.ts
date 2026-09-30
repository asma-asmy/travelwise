import { apiClient } from '@/api/client'
import type {
  Activity,
  ActivityQuery,
  CreateScheduledActivityRequest,
  ScheduleConflict,
  ScheduledActivity,
  ScheduleOptimization,
  TravellerPreference,
  UpdateScheduledActivityRequest,
} from '@/types/activities'
import type { PagedResult } from '@/types/common'

export async function searchActivities(
  query: ActivityQuery = {},
): Promise<PagedResult<Activity>> {
  const { data } = await apiClient.get<PagedResult<Activity>>('/api/v1/activities', {
    params: query,
  })
  return data
}

export async function getActivity(id: string): Promise<Activity> {
  const { data } = await apiClient.get<Activity>(`/api/v1/activities/${id}`)
  return data
}

export async function getMyPreferences(): Promise<TravellerPreference | null> {
  const { data, status } = await apiClient.get<TravellerPreference | ''>(
    '/api/v1/users/me/preferences',
    { validateStatus: (s) => s === 200 || s === 204 },
  )
  if (status === 204 || !data) return null
  return data as TravellerPreference
}

export async function upsertMyPreferences(
  payload: Omit<TravellerPreference, 'userId'>,
): Promise<TravellerPreference> {
  const { data } = await apiClient.put<TravellerPreference>(
    '/api/v1/users/me/preferences',
    payload,
  )
  return data
}

export async function listScheduledActivities(tripId: string): Promise<ScheduledActivity[]> {
  const { data } = await apiClient.get<ScheduledActivity[]>(
    `/api/v1/trips/${tripId}/schedule`,
  )
  return data
}

export async function scheduleActivity(
  tripId: string,
  payload: CreateScheduledActivityRequest,
): Promise<ScheduledActivity> {
  const { data } = await apiClient.post<ScheduledActivity>(
    `/api/v1/trips/${tripId}/schedule`,
    payload,
  )
  return data
}

export async function updateScheduledActivity(
  tripId: string,
  scheduledId: string,
  payload: UpdateScheduledActivityRequest,
): Promise<ScheduledActivity> {
  const { data } = await apiClient.put<ScheduledActivity>(
    `/api/v1/trips/${tripId}/schedule/${scheduledId}`,
    payload,
  )
  return data
}

export async function deleteScheduledActivity(
  tripId: string,
  scheduledId: string,
): Promise<void> {
  await apiClient.delete(`/api/v1/trips/${tripId}/schedule/${scheduledId}`)
}

export async function getScheduleConflicts(tripId: string): Promise<ScheduleConflict[]> {
  const { data } = await apiClient.get<ScheduleConflict[]>(
    `/api/v1/trips/${tripId}/schedule/conflicts`,
  )
  return data
}

export async function optimizeSchedule(tripId: string): Promise<ScheduleOptimization> {
  const { data } = await apiClient.post<ScheduleOptimization>(
    `/api/v1/trips/${tripId}/schedule/optimize`,
  )
  return data
}