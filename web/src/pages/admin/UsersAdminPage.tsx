import { PageHeader } from '@/components/PageHeader'

export function UsersAdminPage() {
  return (
    <>
      <PageHeader title="Users" subtitle="Admin — Phase 8G." />
      <div className="card">
        <p style={{ color: 'var(--color-text-muted)' }}>
          User and role management will be built here.
        </p>
      </div>
    </>
  )
}