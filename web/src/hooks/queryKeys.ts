export const queryKeys = {
  trips: {
    all: ['trips'] as const,
    allTrips: ['trips', 'all'] as const,
    detail: (id: string) => ['trips', id] as const,
  },
  budget: {
    root: (tripId: string) => ['budget', tripId] as const,
    detail: (tripId: string) => ['budget', tripId, 'detail'] as const,
    categories: (tripId: string) => ['budget', tripId, 'categories'] as const,
    expenses: (tripId: string, query?: unknown) => ['budget', tripId, 'expenses', query] as const,
    health: (tripId: string) => ['budget', tripId, 'health'] as const,
  },
  activities: {
    search: (query?: unknown) => ['activities', 'search', query] as const,
    detail: (id: string) => ['activities', id] as const,
    preferences: () => ['activities', 'preferences'] as const,
    schedule: (tripId: string) => ['activities', 'schedule', tripId] as const,
    conflicts: (tripId: string) => ['activities', 'conflicts', tripId] as const,
    optimization: (tripId: string) => ['activities', 'optimization', tripId] as const,
  },
  risk: {
    alerts: (query?: unknown) => ['risk', 'alerts', query] as const,
    destination: (destination: string) => ['risk', 'destination', destination] as const,
    weather: (destination: string, date: string) => ['risk', 'weather', destination, date] as const,
    safety: (destination: string) => ['risk', 'safety', destination] as const,
    tripAssessment: (tripId: string) => ['risk', 'trip', tripId] as const,
  },
  readiness: {
    documents: (query?: unknown) => ['readiness', 'documents', query] as const,
    requirements: (destination?: string) => ['readiness', 'requirements', destination] as const,
    tripAssessment: (tripId: string) => ['readiness', 'trip', tripId] as const,
  },
  workflows: {
    list: (query?: unknown) => ['workflows', 'list', query] as const,
    detail: (id: string) => ['workflows', 'detail', id] as const,
  },
} as const