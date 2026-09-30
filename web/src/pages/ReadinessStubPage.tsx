import { PageHeader } from '@/components/PageHeader'

export function ReadinessStubPage() {
  return (
    <>
      <PageHeader title="Readiness" subtitle="Trip-scoped in Phase 8G." />
      <div className="card">
        <p style={{ color: 'var(--color-text-muted)' }}>
          Select a trip to view its readiness assessment.
        </p>
      </div>
    </>
  )
}