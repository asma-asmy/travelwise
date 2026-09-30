import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import * as activitiesApi from '@/api/activities'
import { queryKeys } from '@/hooks/queryKeys'
import type {
  ActivityQuery,
  CreateScheduledActivityRequest,
  TravellerPreference,
  UpdateScheduledActivityRequest,
} from '@/types/activities'

export function useActivitySearch(query: ActivityQuery = {}) {
  return useQuery({
    queryKey: queryKeys.activities.search(query),
    queryFn: () => activitiesApi.searchActivities(query),
  })
}

export function useActivity(id: string | undefined) {
  return useQuery({
    queryKey: id ? queryKeys.activities.detail(id) : ['activities', 'none'],
    queryFn: () => activitiesApi.getActivity(id!),
    enabled: Boolean(id),
  })
}

export function useMyPreferences() {
  return useQuery({
    queryKey: queryKeys.activities.preferences(),
    queryFn: activitiesApi.getMyPreferences,
  })
}

export function useUpsertMyPreferences() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: Omit<TravellerPreference, 'userId'>) =>
      activitiesApi.upsertMyPreferences(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: queryKeys.activities.preferences() }),
  })
}

export function useSchedule(tripId: string | undefined) {
  return useQuery({
    queryKey: tripId ? queryKeys.activities.schedule(tripId) : ['activities', 'none', 'schedule'],
    queryFn: () => activitiesApi.listScheduledActivities(tripId!),
    enabled: Boolean(tripId),
  })
}

export function useScheduleConflicts(tripId: string | undefined) {
  return useQuery({
    queryKey: tripId ? queryKeys.activities.conflicts(tripId) : ['activities', 'none', 'conflicts'],
    queryFn: () => activitiesApi.getScheduleConflicts(tripId!),
    enabled: Boolean(tripId),
  })
}

export function useScheduleActivity(tripId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateScheduledActivityRequest) =>
      activitiesApi.scheduleActivity(tripId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.activities.schedule(tripId) })
      qc.invalidateQueries({ queryKey: queryKeys.activities.conflicts(tripId) })
    },
  })
}

export function useUpdateScheduledActivity(tripId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ scheduledId, payload }: { scheduledId: string; payload: UpdateScheduledActivityRequest }) =>
      activitiesApi.updateScheduledActivity(tripId, scheduledId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.activities.schedule(tripId) })
      qc.invalidateQueries({ queryKey: queryKeys.activities.conflicts(tripId) })
    },
  })
}

export function useDeleteScheduledActivity(tripId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (scheduledId: string) =>
      activitiesApi.deleteScheduledActivity(tripId, scheduledId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.activities.schedule(tripId) })
      qc.invalidateQueries({ queryKey: queryKeys.activities.conflicts(tripId) })
    },
  })
}

export function useOptimizeSchedule(tripId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: () => activitiesApi.optimizeSchedule(tripId),
    onSuccess: (data) => {
      qc.setQueryData(queryKeys.activities.optimization(tripId), data)
    },
  })
}