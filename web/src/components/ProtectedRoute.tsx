import { Navigate, useLocation } from 'react-router'
import { useAuthStore } from '@/stores/authStore'
import type { UserRole } from '@/types/auth'

interface Props {
  children: React.ReactNode
  requireAnyRole?: UserRole[]
}

export function ProtectedRoute({ children, requireAnyRole }: Props) {
  const isAuthenticated = useAuthStore((s) => s.isAuthenticated)
  const roles = useAuthStore((s) => s.roles)
  const location = useLocation()

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location.pathname }} replace />
  }

  if (requireAnyRole && requireAnyRole.length > 0) {
    const allowed = requireAnyRole.some((r) => roles.includes(r))
    if (!allowed) {
      return <Navigate to="/forbidden" replace />
    }
  }

  return <>{children}</>
}