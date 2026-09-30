import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import * as riskApi from '@/api/risk'
import { queryKeys } from '@/hooks/queryKeys'
import type {
  CreateSafetyAlertRequest,
  SafetyAlertQuery,
  UpdateSafetyAlertRequest,
} from '@/types/risk'

export function useSafetyAlerts(query: SafetyAlertQuery = {}) {
  return useQuery({
    queryKey: queryKeys.risk.alerts(query),
    queryFn: () => riskApi.listSafetyAlerts(query),
  })
}

export function useCreateSafetyAlert() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateSafetyAlertRequest) => riskApi.createSafetyAlert(payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['risk', 'alerts'] }),
  })
}

export function useUpdateSafetyAlert() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ id, payload }: { id: string; payload: UpdateSafetyAlertRequest }) =>
      riskApi.updateSafetyAlert(id, payload),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['risk', 'alerts'] }),
  })
}

export function useDeleteSafetyAlert() {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (id: string) => riskApi.deleteSafetyAlert(id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['risk', 'alerts'] }),
  })
}

export function useDestinationRisk(destination: string | undefined) {
  return useQuery({
    queryKey: destination ? queryKeys.risk.destination(destination) : ['risk', 'none'],
    queryFn: () => riskApi.getDestinationRisk(destination!),
    enabled: Boolean(destination),
  })
}

export function useWeather(destination: string | undefined, date: string | undefined) {
  return useQuery({
    queryKey: destination && date ? queryKeys.risk.weather(destination, date) : ['risk', 'none', 'weather'],
    queryFn: () => riskApi.getWeather(destination!, date!),
    enabled: Boolean(destination && date),
  })
}

export function useSafetyInformation(destination: string | undefined) {
  return useQuery({
    queryKey: destination ? queryKeys.risk.safety(destination) : ['risk', 'none', 'safety'],
    queryFn: () => riskApi.getSafetyInformation(destination!),
    enabled: Boolean(destination),
  })
}

export function useTripRisk(tripId: string | undefined) {
  return useQuery({
    queryKey: tripId ? queryKeys.risk.tripAssessment(tripId) : ['risk', 'none', 'trip'],
    queryFn: () => riskApi.getLatestRiskAssessment(tripId!),
    enabled: Boolean(tripId),
  })
}

export function useAssessTripRisk(tripId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: () => riskApi.assessTrip(tripId),
    onSuccess: (data) => {
      qc.setQueryData(queryKeys.risk.tripAssessment(tripId), data)
    },
  })
}