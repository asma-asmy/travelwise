import { PageHeader } from '@/components/PageHeader'

export function ActivitiesAdminPage() {
  return (
    <>
      <PageHeader title="Activities" subtitle="Catalogue admin — Phase 8G." />
      <div className="card">
        <p style={{ color: 'var(--color-text-muted)' }}>
          Activity catalogue CRUD will be built here.
        </p>
      </div>
    </>
  )
}