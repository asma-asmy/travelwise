import { useAuthStore } from '@/stores/authStore'
import { useTrips, useAllTrips } from '@/hooks/useTrips'
import { useWorkflows } from '@/hooks/useWorkflows'
import { useSafetyAlerts } from '@/hooks/useRisk'

export function useDashboardData() {
  const roles = useAuthStore((s) => s.roles)
  const isStaff = roles.includes('TravelOpsStaff') || roles.includes('Admin')

  // Traveller's own trips — used when not staff.
  const myTrips = useTrips()

  // Cross-user trips — enabled only for staff.
  const allTrips = useAllTrips()

  // Whichever list applies to the current user.
  const trips = isStaff ? allTrips : myTrips

  const pendingApprovals = useWorkflows(
    isStaff ? { status: 'AwaitingApproval', page: 1, pageSize: 5 } : {}
  )
  const alerts = useSafetyAlerts({ isActive: true, page: 1, pageSize: 5 })

  return {
    isStaff,
    trips,
    myTrips,
    allTrips,
    pendingApprovals: isStaff ? pendingApprovals : null,
    alerts,
  }
}