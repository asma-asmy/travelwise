export type TripStatus = 'Draft' | 'Planned' | 'Active' | 'Completed' | 'Cancelled'
export type TripType = 'Leisure' | 'Business' | 'Adventure' | 'Family'

export interface Trip {
  id: string
  userId: string
  title: string
  destination: string
  origin: string | null
  startDate: string          // ISO date (yyyy-MM-dd)
  endDate: string
  currency: string
  tripType: TripType
  status: TripStatus
  travellerCount: number
  createdAt: string
  updatedAt: string
}