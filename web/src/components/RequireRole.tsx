import { useAuthStore } from '@/stores/authStore'
import type { UserRole } from '@/types/auth'

interface Props {
  requireAnyRole?: UserRole[]
  children: React.ReactNode
  fallback?: React.ReactNode
}

/**
 * Renders children only if the current user has at least one of the required roles.
 * If requireAnyRole is undefined or empty, always renders children.
 */
export function RequireRole({ requireAnyRole, children, fallback = null }: Props) {
  const roles = useAuthStore((s) => s.roles)

  if (!requireAnyRole || requireAnyRole.length === 0) return <>{children}</>
  const allowed = requireAnyRole.some((r) => roles.includes(r))
  return allowed ? <>{children}</> : <>{fallback}</>
}