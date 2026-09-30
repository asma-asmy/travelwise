import { PageHeader } from '@/components/PageHeader'

export function WorkflowsListPage() {
  return (
    <>
      <PageHeader
        title="Workflows"
        subtitle="Approval queue and workflow monitor — Phase 8F."
      />
      <div className="card">
        <p style={{ color: 'var(--color-text-muted)' }}>
          Workflow queue and approval console will be built here.
        </p>
      </div>
    </>
  )
}