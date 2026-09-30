import { useState } from 'react'
import { Plus, Trash2 } from 'lucide-react'
import { PageHeader } from '@/components/PageHeader'
import { LoadingState } from '@/components/LoadingState'
import { ErrorState } from '@/components/ErrorState'
import { DataTable } from '@/components/DataTable'
import { Modal } from '@/components/Modal'
import { FormField, FormSelect } from '@/components/FormField'
import { ConfirmDialog } from '@/components/ConfirmDialog'
import { apiClient } from '@/api/client'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useToastStore } from '@/stores/toastStore'
import type { DocumentRequirement } from '@/types/readiness'

const TRIP_TYPES = [
  { value: 1, label: 'Leisure' },
  { value: 2, label: 'Business' },
  { value: 3, label: 'Adventure' },
  { value: 4, label: 'Family' },
]

const DOC_TYPES = [
  { value: 1, label: 'Passport' },
  { value: 2, label: 'Visa' },
  { value: 3, label: 'Insurance' },
  { value: 4, label: 'Vaccination' },
  { value: 5, label: 'Permit' },
  { value: 6, label: 'National ID' },
  { value: 7, label: 'Driver License' },
]

export function RequirementsAdminPage() {
  const qc = useQueryClient()
  const push = useToastStore((s) => s.push)
  const [creating, setCreating] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<DocumentRequirement | null>(null)

  const [form, setForm] = useState({
    destination: '',
    tripType: '1',
    documentType: '1',
    isMandatory: true,
    notes: '',
  })

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ['readiness', 'requirements', 'all'],
    queryFn: async () => {
      const { data } = await apiClient.get<DocumentRequirement[]>('/api/v1/requirements')
      return data
    },
  })

  const createMutation = useMutation({
    mutationFn: async (payload: unknown) => {
      const { data } = await apiClient.post('/api/v1/requirements', payload)
      return data
    },
    onSuccess: () => {
      push('success', 'Requirement created.')
      qc.invalidateQueries({ queryKey: ['readiness', 'requirements'] })
      setCreating(false)
      setForm({ destination: '', tripType: '1', documentType: '1', isMandatory: true, notes: '' })
    },
    onError: (e: unknown) => {
      const title = (e as { response?: { data?: { title?: string } } })?.response?.data?.title
      push('error', title ?? 'Failed to create requirement.')
    },
  })

  const deleteMutation = useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/api/v1/requirements/${id}`)
    },
    onSuccess: () => {
      push('success', 'Requirement deleted.')
      qc.invalidateQueries({ queryKey: ['readiness', 'requirements'] })
      setDeleteTarget(null)
    },
    onError: () => push('error', 'Failed to delete requirement.'),
  })

  async function handleCreate() {
    await createMutation.mutateAsync({
      destination: form.destination.trim(),
      tripType: Number(form.tripType),
      documentType: Number(form.documentType),
      isMandatory: form.isMandatory,
      notes: form.notes.trim() || null,
    })
  }

  return (
    <>
      <PageHeader
        title="Document Requirements"
        subtitle="Required documents per destination and trip type."
        actions={
          <button className="btn btn-primary" onClick={() => setCreating(true)}>
            <Plus size={16} /> New requirement
          </button>
        }
      />

      {isLoading && <LoadingState />}
      {isError && <ErrorState message="Failed to load requirements." onRetry={() => refetch()} />}
      {data && (
        <DataTable<DocumentRequirement>
          rows={data}
          getRowKey={(r) => r.id}
          emptyMessage="No requirements configured."
          columns={[
            { key: 'destination', header: 'Destination', render: (r) => <strong>{r.destination}</strong> },
            { key: 'tripType', header: 'Trip type', render: (r) => r.tripTypeName },
            { key: 'documentType', header: 'Document', render: (r) => r.documentTypeName },
            {
              key: 'mandatory',
              header: 'Mandatory',
              render: (r) => (
                <span className={`badge ${r.isMandatory ? 'badge-danger' : 'badge-neutral'}`}>
                  {r.isMandatory ? 'Mandatory' : 'Optional'}
                </span>
              ),
            },
            { key: 'notes', header: 'Notes', render: (r) => r.notes ?? '—' },
            {
              key: 'actions',
              header: '',
              align: 'right',
              render: (r) => (
                <button className="btn btn-secondary" onClick={() => setDeleteTarget(r)} aria-label="Delete">
                  <Trash2 size={14} />
                </button>
              ),
            },
          ]}
        />
      )}

      <Modal
        open={creating}
        title="New document requirement"
        onClose={() => setCreating(false)}
        footer={
          <>
            <button className="btn btn-secondary" onClick={() => setCreating(false)}>Cancel</button>
            <button className="btn btn-primary" onClick={handleCreate} disabled={createMutation.isPending}>
              {createMutation.isPending ? 'Creating…' : 'Create'}
            </button>
          </>
        }
      >
        <FormField
          label="Destination"
          id="reqDest"
          value={form.destination}
          onChange={(v) => setForm({ ...form, destination: v })}
          required
        />
        <FormSelect
          label="Trip type"
          id="reqTripType"
          value={form.tripType}
          onChange={(v) => setForm({ ...form, tripType: v })}
          options={TRIP_TYPES}
        />
        <FormSelect
          label="Document type"
          id="reqDocType"
          value={form.documentType}
          onChange={(v) => setForm({ ...form, documentType: v })}
          options={DOC_TYPES}
        />
        <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.875rem', marginBottom: '1rem' }}>
          <input
            type="checkbox"
            checked={form.isMandatory}
            onChange={(e) => setForm({ ...form, isMandatory: e.target.checked })}
          />
          Mandatory
        </label>
        <FormField
          label="Notes"
          id="reqNotes"
          value={form.notes}
          onChange={(v) => setForm({ ...form, notes: v })}
        />
      </Modal>

      <ConfirmDialog
        open={deleteTarget !== null}
        title="Delete this requirement?"
        description="Travellers will no longer see it in their readiness checklists."
        confirmLabel="Delete"
        danger
        submitting={deleteMutation.isPending}
        onConfirm={() => deleteTarget && deleteMutation.mutate(deleteTarget.id)}
        onClose={() => setDeleteTarget(null)}
      />
    </>
  )
}