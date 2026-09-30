import { PageHeader } from '@/components/PageHeader'

export function AlertsPage() {
  return (
    <>
      <PageHeader title="Safety Alerts" subtitle="Admin & staff alerts — Phase 8G." />
      <div className="card">
        <p style={{ color: 'var(--color-text-muted)' }}>
          Alert management will be built here.
        </p>
      </div>
    </>
  )
}