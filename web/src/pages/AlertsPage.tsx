import { useState } from 'react'
import { Plus, Trash2 } from 'lucide-react'
import { PageHeader } from '@/components/PageHeader'
import { LoadingState } from '@/components/LoadingState'
import { ErrorState } from '@/components/ErrorState'
import { DataTable } from '@/components/DataTable'
import { Pagination } from '@/components/Pagination'
import { Modal } from '@/components/Modal'
import { FormField, FormSelect, FormTextarea } from '@/components/FormField'
import { ConfirmDialog } from '@/components/ConfirmDialog'
import { StatusBadge } from '@/components/StatusBadge'
import { useSafetyAlerts, useCreateSafetyAlert, useDeleteSafetyAlert } from '@/hooks/useRisk'
import { useToastStore } from '@/stores/toastStore'
import { formatDateTime } from '@/utils/format'
import type { SafetyAlert } from '@/types/risk'

const CATEGORIES = ['Weather', 'Health', 'Transport', 'Safety', 'Other']
const SEVERITIES = [
  { value: 0, label: 'Low' },
  { value: 1, label: 'Medium' },
  { value: 2, label: 'High' },
  { value: 3, label: 'Critical' },
]

export function AlertsPage() {
  const [page, setPage] = useState(1)
  const push = useToastStore((s) => s.push)

  const { data, isLoading, isError, refetch } = useSafetyAlerts({
    page,
    pageSize: 20,
    isActive: true,
  })
  const createMutation = useCreateSafetyAlert()
  const deleteMutation = useDeleteSafetyAlert()

  const [creating, setCreating] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<SafetyAlert | null>(null)

  const [form, setForm] = useState({
    destination: '',
    category: 'Weather',
    severity: '1',
    message: '',
    validFrom: new Date().toISOString().slice(0, 16),
    validTo: new Date(Date.now() + 7 * 864e5).toISOString().slice(0, 16),
  })

  async function handleCreate() {
    try {
      await createMutation.mutateAsync({
        destination: form.destination.trim(),
        category: form.category,
        severity: Number(form.severity),
        message: form.message.trim(),
        validFrom: new Date(form.validFrom).toISOString(),
        validTo: new Date(form.validTo).toISOString(),
      })
      push('success', 'Alert created.')
      setCreating(false)
      setForm({ ...form, destination: '', message: '' })
    } catch {
      push('error', 'Failed to create alert.')
    }
  }

  async function handleDelete() {
    if (!deleteTarget) return
    try {
      await deleteMutation.mutateAsync(deleteTarget.id)
      push('success', 'Alert deleted.')
      setDeleteTarget(null)
    } catch {
      push('error', 'Failed to delete alert.')
    }
  }

  return (
    <>
      <PageHeader
        title="Safety Alerts"
        subtitle="Manage destination alerts shown to travellers and the risk agents."
        actions={
          <button className="btn btn-primary" onClick={() => setCreating(true)}>
            <Plus size={16} /> New alert
          </button>
        }
      />

      {isLoading && <LoadingState />}
      {isError && <ErrorState message="Failed to load alerts." onRetry={() => refetch()} />}
      {data && (
        <>
          <DataTable<SafetyAlert>
            rows={data.items}
            getRowKey={(a) => a.id}
            emptyMessage="No alerts found."
            columns={[
              {
                key: 'destination',
                header: 'Destination',
                render: (a) => <strong>{a.destination}</strong>,
              },
              { key: 'category', header: 'Category', render: (a) => a.category },
              {
                key: 'severity',
                header: 'Severity',
                render: (a) => <StatusBadge status={a.severityName} />,
              },
              { key: 'message', header: 'Message', render: (a) => a.message },
              {
                key: 'valid',
                header: 'Valid',
                render: (a) => (
                  <span style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
                    {formatDateTime(a.validFrom)} → {formatDateTime(a.validTo)}
                  </span>
                ),
              },
              {
                key: 'actions',
                header: '',
                align: 'right',
                render: (a) => (
                  <button
                    className="btn btn-secondary"
                    onClick={() => setDeleteTarget(a)}
                    aria-label="Delete"
                  >
                    <Trash2 size={14} />
                  </button>
                ),
              },
            ]}
          />
          <Pagination
            page={data.page}
            totalPages={data.totalPages}
            totalCount={data.totalCount}
            onPageChange={setPage}
          />
        </>
      )}

      <Modal
        open={creating}
        title="New safety alert"
        onClose={() => setCreating(false)}
        footer={
          <>
            <button className="btn btn-secondary" onClick={() => setCreating(false)}>
              Cancel
            </button>
            <button
              className="btn btn-primary"
              onClick={handleCreate}
              disabled={createMutation.isPending}
            >
              {createMutation.isPending ? 'Creating…' : 'Create'}
            </button>
          </>
        }
      >
        <FormField
          label="Destination"
          id="alertDestination"
          value={form.destination}
          onChange={(v) => setForm({ ...form, destination: v })}
          required
        />
        <FormSelect
          label="Category"
          id="alertCategory"
          value={form.category}
          onChange={(v) => setForm({ ...form, category: v })}
          options={CATEGORIES.map((c) => ({ value: c, label: c }))}
        />
        <FormSelect
          label="Severity"
          id="alertSeverity"
          value={form.severity}
          onChange={(v) => setForm({ ...form, severity: v })}
          options={SEVERITIES.map((s) => ({ value: s.value, label: s.label }))}
        />
        <FormTextarea
          label="Message"
          id="alertMessage"
          value={form.message}
          onChange={(v) => setForm({ ...form, message: v })}
          maxLength={500}
          required
        />
        <FormField
          label="Valid from"
          id="alertFrom"
          type="datetime-local"
          value={form.validFrom}
          onChange={(v) => setForm({ ...form, validFrom: v })}
        />
        <FormField
          label="Valid to"
          id="alertTo"
          type="datetime-local"
          value={form.validTo}
          onChange={(v) => setForm({ ...form, validTo: v })}
        />
      </Modal>

      <ConfirmDialog
        open={deleteTarget !== null}
        title="Delete this alert?"
        description="It will be deactivated and hidden from travellers and agents."
        confirmLabel="Delete"
        danger
        submitting={deleteMutation.isPending}
        onConfirm={handleDelete}
        onClose={() => setDeleteTarget(null)}
      />
    </>
  )
}