import { PageHeader } from '@/components/PageHeader'

export function SettingsAdminPage() {
  return (
    <>
      <PageHeader title="Settings" subtitle="Admin — Phase 8G." />
      <div className="card">
        <p style={{ color: 'var(--color-text-muted)' }}>
          System settings will be configured here.
        </p>
      </div>
    </>
  )
}