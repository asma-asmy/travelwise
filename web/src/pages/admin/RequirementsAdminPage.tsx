import { PageHeader } from '@/components/PageHeader'

export function RequirementsAdminPage() {
  return (
    <>
      <PageHeader title="Document Requirements" subtitle="Admin — Phase 8G." />
      <div className="card">
        <p style={{ color: 'var(--color-text-muted)' }}>
          Destination-specific document requirements will be managed here.
        </p>
      </div>
    </>
  )
}