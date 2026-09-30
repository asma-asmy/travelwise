import { PageHeader } from '@/components/PageHeader'

export function BudgetStubPage() {
  return (
    <>
      <PageHeader title="Budget" subtitle="Trip-scoped in Phase 8G." />
      <div className="card">
        <p style={{ color: 'var(--color-text-muted)' }}>
          Select a trip to view and edit its budget.
        </p>
      </div>
    </>
  )
}