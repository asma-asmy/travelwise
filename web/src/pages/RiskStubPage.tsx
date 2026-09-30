import { PageHeader } from '@/components/PageHeader'

export function RiskStubPage() {
  return (
    <>
      <PageHeader title="Risk" subtitle="Trip-scoped in Phase 8G." />
      <div className="card">
        <p style={{ color: 'var(--color-text-muted)' }}>
          Select a trip to view its risk assessment.
        </p>
      </div>
    </>
  )
}