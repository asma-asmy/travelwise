import { useQuery } from '@tanstack/react-query'
import * as tripsApi from '@/api/trips'
import { queryKeys } from '@/hooks/queryKeys'

export function useTrips() {
  return useQuery({
    queryKey: queryKeys.trips.all,
    queryFn: tripsApi.listTrips,
    staleTime: 30_000,
  })
}

export function useTrip(id: string | undefined) {
  return useQuery({
    queryKey: id ? queryKeys.trips.detail(id) : ['trips', 'none'],
    queryFn: () => tripsApi.getTrip(id!),
    enabled: Boolean(id),
  })
}