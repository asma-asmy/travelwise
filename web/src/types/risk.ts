export type RiskLevel = 'Low' | 'Medium' | 'High' | 'Critical'

export interface RiskAssessment {
  id: string
  tripId: string
  overallLevel: RiskLevel
  overallLevelName: string
  assessedAt: string
  factors: RiskFactor[]
  impactedActivityIds: string[]
  recommendations: string[]
}

export interface RiskFactor {
  type: 'DESTINATION' | 'WEATHER' | 'ALERT' | 'ACTIVITY'
  severity: RiskLevel
  description: string
  relatedActivityId: string | null
}

export interface SafetyAlert {
  id: string
  destination: string
  category: string
  severity: RiskLevel
  severityName: string
  message: string
  validFrom: string
  validTo: string
  isActive: boolean
}

export interface DestinationRisk {
  destination: string
  baseLevel: RiskLevel
  baseLevelName: string
  knownRisks: string[]
  description: string | null
}

export interface WeatherSnapshot {
  destination: string
  date: string
  minTemperatureC: number | null
  maxTemperatureC: number | null
  precipitationMm: number | null
  windSpeedKph: number | null
  condition: string | null
  fromCache: boolean
  fetchedAt: string
}

export interface SafetyInformation {
  destination: string
  emergencyNumber: string | null
  hospitalInfo: string | null
  localAdvisory: string | null
  activeAlerts: SafetyAlert[]
}

export interface SafetyAlertQuery {
  destination?: string
  category?: string
  minSeverity?: RiskLevel
  isActive?: boolean
  sortBy?: 'ValidFrom' | 'Severity'
  sortDir?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}

export interface CreateSafetyAlertRequest {
  destination: string
  category: string
  severity: number      // 0=Low, 1=Medium, 2=High, 3=Critical
  message: string
  validFrom: string
  validTo: string
}

export interface UpdateSafetyAlertRequest {
  category: string
  severity: number
  message: string
  validFrom: string
  validTo: string
  isActive: boolean
}