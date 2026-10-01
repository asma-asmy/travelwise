import { useState } from 'react'
import { Plus, Pencil, Trash2 } from 'lucide-react'
import { PageHeader } from '@/components/PageHeader'
import { ErrorState } from '@/components/ErrorState'
import { DataTable } from '@/components/DataTable'
import { Pagination } from '@/components/Pagination'
import { Modal } from '@/components/Modal'
import { FormField, FormSelect } from '@/components/FormField'
import { ConfirmDialog } from '@/components/ConfirmDialog'
import {
  useMyDocuments,
  useAddMyDocument,
  useUpdateMyDocument,
  useDeleteMyDocument,
} from '@/hooks/useReadiness'
import { useToastStore } from '@/stores/toastStore'
import type { TravelDocument, DocumentType } from '@/types/readiness'
import { SkeletonTable } from '@/components/SkeletonTable'

const DOC_TYPES: { value: number; label: DocumentType }[] = [
  { value: 1, label: 'Passport' },
  { value: 2, label: 'Visa' },
  { value: 3, label: 'Insurance' },
  { value: 4, label: 'Vaccination' },
  { value: 5, label: 'Permit' },
  { value: 6, label: 'NationalId' },
  { value: 7, label: 'DriverLicense' },
]

export function MyDocumentsPage() {
  const push = useToastStore((s) => s.push)
  const [page, setPage] = useState(1)
  const { data, isLoading, isError, refetch } = useMyDocuments({ page, pageSize: 20 })

  const add = useAddMyDocument()
  const update = useUpdateMyDocument()
  const remove = useDeleteMyDocument()

  const [editing, setEditing] = useState<TravelDocument | null>(null)
  const [creating, setCreating] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<TravelDocument | null>(null)

  const [form, setForm] = useState({
    type: '1',
    number: '',
    issuedDate: '',
    expiryDate: '',
    notes: '',
  })

  function resetForm() {
    setForm({ type: '1', number: '', issuedDate: '', expiryDate: '', notes: '' })
  }

  function startEdit(d: TravelDocument) {
    const typeNum = DOC_TYPES.find((t) => t.label === d.type)
    setForm({
      type: String(typeNum?.value ?? 1),
      number: d.number,
      issuedDate: d.issuedDate ?? '',
      expiryDate: d.expiryDate ?? '',
      notes: d.notes ?? '',
    })
    setEditing(d)
  }

  async function handleSubmit() {
    const payload = {
      type: Number(form.type),
      number: form.number.trim(),
      issuedDate: form.issuedDate || null,
      expiryDate: form.expiryDate || null,
      notes: form.notes.trim() || null,
    }

    try {
      if (editing) {
        await update.mutateAsync({ id: editing.id, payload })
        push('success', 'Document updated.')
        setEditing(null)
      } else {
        await add.mutateAsync(payload)
        push('success', 'Document added.')
        setCreating(false)
      }
      resetForm()
    } catch {
      push('error', 'Failed to save document.')
    }
  }

  async function handleDelete() {
    if (!deleteTarget) return
    try {
      await remove.mutateAsync(deleteTarget.id)
      push('success', 'Document deleted.')
      setDeleteTarget(null)
    } catch {
      push('error', 'Failed to delete document.')
    }
  }

  return (
    <>
      <PageHeader
        title="My Documents"
        subtitle="Your passports, visas, insurance, and other travel documents."
        actions={
          <button
            className="btn btn-primary"
            onClick={() => {
              resetForm()
              setCreating(true)
            }}
          >
            <Plus size={16} /> Add document
          </button>
        }
      />

      {isLoading && <SkeletonTable rows={5} columns={5} />}
      {isError && <ErrorState message="Failed to load documents." onRetry={() => refetch()} />}

      {data && (
        <>
          <DataTable<TravelDocument>
            rows={data.items}
            getRowKey={(d) => d.id}
            emptyMessage="No documents yet. Add your passport to get started."
            columns={[
              {
                key: 'type',
                header: 'Type',
                render: (d) => <strong>{d.typeName}</strong>,
              },
              {
                key: 'number',
                header: 'Number',
                render: (d) => d.number,
              },
              {
                key: 'issued',
                header: 'Issued',
                render: (d) => d.issuedDate ?? '—',
              },
              {
                key: 'expiry',
                header: 'Expires',
                render: (d) => {
                  if (!d.expiryDate) return '—'
                  const colour = d.isExpired
                    ? 'var(--color-danger)'
                    : d.daysUntilExpiry !== null && d.daysUntilExpiry < 90
                      ? 'var(--color-warning)'
                      : 'inherit'
                  return (
                    <span style={{ color: colour }}>
                      {d.expiryDate}
                      {d.daysUntilExpiry !== null && (
                        <span
                          style={{
                            fontSize: '0.75rem',
                            color: 'var(--color-text-muted)',
                            marginLeft: '0.5rem',
                          }}
                        >
                          ({d.daysUntilExpiry}d)
                        </span>
                      )}
                    </span>
                  )
                },
              },
              {
                key: 'notes',
                header: 'Notes',
                render: (d) => d.notes ?? '—',
              },
              {
                key: 'actions',
                header: '',
                align: 'right',
                render: (d) => (
                  <div
                    style={{
                      display: 'flex',
                      gap: '0.25rem',
                      justifyContent: 'flex-end',
                    }}
                  >
                    <button
                      className="btn btn-secondary"
                      onClick={() => startEdit(d)}
                      aria-label="Edit"
                    >
                      <Pencil size={14} />
                    </button>
                    <button
                      className="btn btn-secondary"
                      onClick={() => setDeleteTarget(d)}
                      aria-label="Delete"
                    >
                      <Trash2 size={14} />
                    </button>
                  </div>
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
        open={creating || editing !== null}
        title={editing ? 'Edit document' : 'Add document'}
        onClose={() => {
          setCreating(false)
          setEditing(null)
          resetForm()
        }}
        footer={
          <>
            <button
              className="btn btn-secondary"
              onClick={() => {
                setCreating(false)
                setEditing(null)
                resetForm()
              }}
            >
              Cancel
            </button>
            <button
              className="btn btn-primary"
              onClick={handleSubmit}
              disabled={add.isPending || update.isPending}
            >
              {add.isPending || update.isPending ? 'Saving…' : 'Save'}
            </button>
          </>
        }
      >
        <FormSelect
          label="Type"
          id="docType"
          value={form.type}
          onChange={(v) => setForm({ ...form, type: v })}
          options={DOC_TYPES.map((t) => ({ value: t.value, label: t.label }))}
        />
        <FormField
          label="Number"
          id="docNumber"
          value={form.number}
          onChange={(v) => setForm({ ...form, number: v })}
          required
        />
        <FormField
          label="Issued date"
          id="docIssued"
          type="date"
          value={form.issuedDate}
          onChange={(v) => setForm({ ...form, issuedDate: v })}
        />
        <FormField
          label="Expiry date"
          id="docExpiry"
          type="date"
          value={form.expiryDate}
          onChange={(v) => setForm({ ...form, expiryDate: v })}
        />
        <FormField
          label="Notes"
          id="docNotes"
          value={form.notes}
          onChange={(v) => setForm({ ...form, notes: v })}
        />
      </Modal>

      <ConfirmDialog
        open={deleteTarget !== null}
        title="Delete this document?"
        confirmLabel="Delete"
        danger
        submitting={remove.isPending}
        onConfirm={handleDelete}
        onClose={() => setDeleteTarget(null)}
      />
    </>
  )
}