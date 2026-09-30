import { apiClient } from '@/api/client'
import type { Trip } from '@/types/trips'

export async function listTrips(): Promise<Trip[]> {
  const { data } = await apiClient.get<Trip[]>('/api/v1/trips')
  return data
}

export async function getTrip(id: string): Promise<Trip> {
  const { data } = await apiClient.get<Trip>(`/api/v1/trips/${id}`)
  return data
}