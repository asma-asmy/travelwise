import { PageHeader } from '@/components/PageHeader'

export function TripsListPage() {
  return (
    <>
      <PageHeader
        title="My Trips"
        subtitle="Trips page — full implementation in Phase 8G."
      />
      <div className="card">
        <p style={{ color: 'var(--color-text-muted)' }}>
          Trip list, filters, and details will be built here.
        </p>
      </div>
    </>
  )
}