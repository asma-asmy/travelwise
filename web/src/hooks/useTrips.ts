import { useQuery } from '@tanstack/react-query'
import * as tripsApi from '@/api/trips'
import { queryKeys } from '@/hooks/queryKeys'
import { useAuthStore } from '@/stores/authStore'

/// <summary>Trips owned by the current user.</summary>
export function useTrips() {
  return useQuery({
    queryKey: queryKeys.trips.all,
    queryFn: tripsApi.listTrips,
    staleTime: 30_000,
  })
}

/// <summary>Cross-user list for staff. Only runs for users with staff roles.</summary>
export function useAllTrips() {
  const roles = useAuthStore((s) => s.roles)
  const isStaff = roles.includes('TravelOpsStaff') || roles.includes('Admin')

  return useQuery({
    queryKey: queryKeys.trips.allTrips,
    queryFn: tripsApi.listAllTrips,
    enabled: isStaff,
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