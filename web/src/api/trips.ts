import { apiClient } from '@/api/client'
import type { Trip } from '@/types/trips'

/// <summary>List trips owned by the current user.</summary>
export async function listTrips(): Promise<Trip[]> {
  const { data } = await apiClient.get<Trip[]>('/api/v1/trips')
  return data
}

/// <summary>List all trips (cross-user). Requires TravelOpsStaff or Admin role.</summary>
export async function listAllTrips(): Promise<Trip[]> {
  const { data } = await apiClient.get<Trip[]>('/api/v1/admin/trips')
  return data
}

/// <summary>Single trip by ID. Staff can access any; travellers only their own.</summary>
export async function getTrip(id: string): Promise<Trip> {
  const { data } = await apiClient.get<Trip>(`/api/v1/trips/${id}`)
  return data
}