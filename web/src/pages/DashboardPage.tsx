import { PageHeader } from '@/components/PageHeader'
import { useAuthStore } from '@/stores/authStore'

export function DashboardPage() {
  const fullName = useAuthStore((s) => s.fullName)
  const roles = useAuthStore((s) => s.roles)

  return (
    <>
      <PageHeader
        title={`Welcome, ${fullName}`}
        subtitle={`Signed in as ${roles.join(', ')}. Dashboard widgets coming in Phase 8E.`}
      />
      <div className="card">
        <p style={{ color: 'var(--color-text-muted)' }}>
          Counts, alerts, and pending approvals will appear here.
        </p>
      </div>
    </>
  )
}