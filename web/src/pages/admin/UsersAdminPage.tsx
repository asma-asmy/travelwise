import { PageHeader } from '@/components/PageHeader'

export function UsersAdminPage() {
  return (
    <>
      <PageHeader
        title="Users"
        subtitle="User and role management."
      />
      <div className="card">
        <p style={{ color: 'var(--color-text-muted)' }}>
          User listing requires a dedicated <code>GET /api/v1/admin/users</code> endpoint,
          which is not yet implemented. This page is a placeholder for future user
          management features (role assignment, activation/deactivation, password reset).
        </p>
      </div>
    </>
  )
}