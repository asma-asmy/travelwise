import { useParams } from 'react-router'
import { PageHeader } from '@/components/PageHeader'

export function WorkflowDetailPage() {
  const { id } = useParams<{ id: string }>()
  return (
    <>
      <PageHeader title="Workflow" subtitle={`ID: ${id}`} />
      <div className="card">
        <p style={{ color: 'var(--color-text-muted)' }}>
          Full workflow monitor and approval console in Phase 8F.
        </p>
      </div>
    </>
  )
}