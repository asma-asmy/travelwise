export type ActivityCategory =
  | 'Adventure' | 'Nature' | 'Culture' | 'Food'
  | 'Relaxation' | 'Sightseeing' | 'Shopping' | 'Nightlife'

export interface Activity {
  id: string
  name: string
  category: ActivityCategory
  categoryName: string
  description: string
  durationMinutes: number
  estimatedCost: number
  currency: string
  location: string
  latitude: number | null
  longitude: number | null
  isIndoor: boolean
  difficulty: 'Easy' | 'Moderate' | 'Hard'
  isActive: boolean
  createdAt: string
  updatedAt: string
}

export interface ScheduledActivity {
  id: string
  tripId: string
  activityId: string
  activityName: string
  activityCategory: string
  location: string
  isIndoor: boolean
  durationMinutes: number
  estimatedCost: number
  scheduledDate: string
  startTime: string
  endTime: string
  status: 'Planned' | 'Completed' | 'Cancelled' | 'Recovered'
  notes: string | null
}

export interface TravellerPreference {
  userId: string
  interests: string[]
  pace: 'Relaxed' | 'Moderate' | 'Packed'
  mobilityNotes: string | null
  dietary: string[]
  budgetTier: 'Budget' | 'Mid' | 'Luxury'
}

export interface ScheduleConflict {
  conflictingScheduledActivityId: string
  conflictingActivityId: string
  conflictingActivityName: string
  scheduledDate: string
  startTime: string
  endTime: string
  reason: string
}

export interface ActivityRecommendation {
  activityId: string
  name: string
  category: string
  estimatedCost: number
  durationMinutes: number
  preferenceScore: number
  isIndoor: boolean
  location: string
}

export interface ScheduleOptimization {
  tripId: string
  totalActivitiesScheduled: number
  totalMinutesScheduled: number
  totalEstimatedCost: number
  preferenceMatchCount: number
  conflicts: ScheduleConflict[]
  recommendations: ActivityRecommendation[]
  warnings: string[]
}

export interface ActivityQuery {
  search?: string
  category?: ActivityCategory
  location?: string
  isIndoor?: boolean
  difficulty?: string
  minCost?: number
  maxCost?: number
  maxDurationMinutes?: number
  sortBy?: 'Name' | 'Cost' | 'Duration'
  sortDir?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export interface CreateScheduledActivityRequest {
  activityId: string
  scheduledDate: string
  startTime: string
  notes?: string | null
}

export interface UpdateScheduledActivityRequest {
  scheduledDate: string
  startTime: string
  status: 'Planned' | 'Completed' | 'Cancelled' | 'Recovered'
  notes?: string | null
}