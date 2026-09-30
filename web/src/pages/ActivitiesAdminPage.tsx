import { useState } from 'react'
import { Plus, Pencil, PowerOff } from 'lucide-react'
import { PageHeader } from '@/components/PageHeader'
import { LoadingState } from '@/components/LoadingState'
import { ErrorState } from '@/components/ErrorState'
import { DataTable } from '@/components/DataTable'
import { Pagination } from '@/components/Pagination'
import { Modal } from '@/components/Modal'
import { FormField, FormSelect, FormTextarea } from '@/components/FormField'
import { ConfirmDialog } from '@/components/ConfirmDialog'
import { useActivitySearch } from '@/hooks/useActivities'
import { apiClient } from '@/api/client'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useToastStore } from '@/stores/toastStore'
import type { Activity, ActivityCategory } from '@/types/activities'

const CATEGORIES: { value: number; label: ActivityCategory }[] = [
  { value: 1, label: 'Adventure' },
  { value: 2, label: 'Nature' },
  { value: 3, label: 'Culture' },
  { value: 4, label: 'Food' },
  { value: 5, label: 'Relaxation' },
  { value: 6, label: 'Sightseeing' },
  { value: 7, label: 'Shopping' },
  { value: 8, label: 'Nightlife' },
]

const DIFFICULTIES = ['Easy', 'Moderate', 'Hard']

export function ActivitiesAdminPage() {
  const qc = useQueryClient()
  const push = useToastStore((s) => s.push)
  const [page, setPage] = useState(1)
  const [search, setSearch] = useState('')

  const { data, isLoading, isError, refetch } = useActivitySearch({ page, pageSize: 20, search })

  const [editing, setEditing] = useState<Activity | null>(null)
  const [creating, setCreating] = useState(false)
  const [deactivateTarget, setDeactivateTarget] = useState<string | null>(null)

  const [form, setForm] = useState({
    name: '',
    category: '1',
    description: '',
    durationMinutes: '60',
    estimatedCost: '0',
    currency: 'LKR',
    location: '',
    isIndoor: false,
    difficulty: 'Easy',
  })

  function resetForm() {
    setForm({
      name: '',
      category: '1',
      description: '',
      durationMinutes: '60',
      estimatedCost: '0',
      currency: 'LKR',
      location: '',
      isIndoor: false,
      difficulty: 'Easy',
    })
  }

  function startEdit(a: Activity) {
    const catNum = CATEGORIES.find((c) => c.label === a.category)
    setForm({
      name: a.name,
      category: String(catNum?.value ?? 1),
      description: a.description,
      durationMinutes: String(a.durationMinutes),
      estimatedCost: String(a.estimatedCost),
      currency: a.currency,
      location: a.location,
      isIndoor: a.isIndoor,
      difficulty: a.difficulty,
    })
    setEditing(a)
  }

  const createMutation = useMutation({
    mutationFn: async (payload: unknown) => {
      const { data } = await apiClient.post('/api/v1/activities', payload)
      return data
    },
    onSuccess: () => {
      push('success', 'Activity created.')
      qc.invalidateQueries({ queryKey: ['activities'] })
      setCreating(false)
      resetForm()
    },
    onError: () => push('error', 'Failed to create activity.'),
  })

  const updateMutation = useMutation({
    mutationFn: async ({ id, payload }: { id: string; payload: unknown }) => {
      const { data } = await apiClient.put(`/api/v1/activities/${id}`, payload)
      return data
    },
    onSuccess: () => {
      push('success', 'Activity updated.')
      qc.invalidateQueries({ queryKey: ['activities'] })
      setEditing(null)
      resetForm()
    },
    onError: () => push('error', 'Failed to update activity.'),
  })

  const deactivateMutation = useMutation({
    mutationFn: async (id: string) => {
      await apiClient.delete(`/api/v1/activities/${id}`)
    },
    onSuccess: () => {
      push('success', 'Activity deactivated.')
      qc.invalidateQueries({ queryKey: ['activities'] })
      setDeactivateTarget(null)
    },
    onError: () => push('error', 'Failed to deactivate.'),
  })

  function buildPayload() {
    return {
      name: form.name.trim(),
      category: Number(form.category),
      description: form.description.trim(),
      durationMinutes: Number(form.durationMinutes),
      estimatedCost: Number(form.estimatedCost),
      currency: form.currency.trim().toUpperCase(),
      location: form.location.trim(),
      isIndoor: form.isIndoor,
      difficulty: form.difficulty,
      isActive: true,
    }
  }

  async function handleSubmit() {
    const payload = buildPayload()
    if (editing) {
      await updateMutation.mutateAsync({ id: editing.id, payload })
    } else {
      await createMutation.mutateAsync(payload)
    }
  }

  return (
    <>
      <PageHeader
        title="Activities"
        subtitle="Activity catalogue management."
        actions={
          <button
            className="btn btn-primary"
            onClick={() => {
              resetForm()
              setCreating(true)
            }}
          >
            <Plus size={16} /> New activity
          </button>
        }
      />

      <div className="card" style={{ marginBottom: '1rem' }}>
        <FormField
          label="Search"
          id="activitySearch"
          value={search}
          onChange={(v) => {
            setSearch(v)
            setPage(1)
          }}
          placeholder="Name or description"
        />
      </div>

      {isLoading && <LoadingState />}
      {isError && <ErrorState message="Failed to load activities." onRetry={() => refetch()} />}
      {data && (
        <>
          <DataTable<Activity>
            rows={data.items}
            getRowKey={(a) => a.id}
            emptyMessage="No activities found."
            columns={[
              { key: 'name', header: 'Name', render: (a) => <strong>{a.name}</strong> },
              { key: 'category', header: 'Category', render: (a) => a.categoryName },
              { key: 'location', header: 'Location', render: (a) => a.location },
              {
                key: 'duration',
                header: 'Duration',
                render: (a) => `${a.durationMinutes} min`,
                align: 'right',
              },
              {
                key: 'cost',
                header: 'Cost',
                render: (a) => `${a.estimatedCost} ${a.currency}`,
                align: 'right',
              },
              {
                key: 'indoor',
                header: 'Type',
                render: (a) => (a.isIndoor ? 'Indoor' : 'Outdoor'),
              },
              {
                key: 'active',
                header: 'Status',
                render: (a) => (
                  <span className={`badge ${a.isActive ? 'badge-success' : 'badge-neutral'}`}>
                    {a.isActive ? 'Active' : 'Inactive'}
                  </span>
                ),
              },
              {
                key: 'actions',
                header: '',
                align: 'right',
                render: (a) => (
                  <div style={{ display: 'flex', gap: '0.25rem', justifyContent: 'flex-end' }}>
                    <button className="btn btn-secondary" onClick={() => startEdit(a)} aria-label="Edit">
                      <Pencil size={14} />
                    </button>
                    <button
                      className="btn btn-secondary"
                      onClick={() => setDeactivateTarget(a.id)}
                      aria-label="Deactivate"
                    >
                      <PowerOff size={14} />
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
        title={editing ? 'Edit activity' : 'New activity'}
        onClose={() => {
          setCreating(false)
          setEditing(null)
        }}
        footer={
          <>
            <button
              className="btn btn-secondary"
              onClick={() => {
                setCreating(false)
                setEditing(null)
              }}
            >
              Cancel
            </button>
            <button
              className="btn btn-primary"
              onClick={handleSubmit}
              disabled={createMutation.isPending || updateMutation.isPending}
            >
              {createMutation.isPending || updateMutation.isPending ? 'Saving…' : 'Save'}
            </button>
          </>
        }
      >
        <FormField label="Name" id="actName" value={form.name} onChange={(v) => setForm({ ...form, name: v })} required />
        <FormSelect
          label="Category"
          id="actCategory"
          value={form.category}
          onChange={(v) => setForm({ ...form, category: v })}
          options={CATEGORIES.map((c) => ({ value: c.value, label: c.label }))}
        />
        <FormTextarea
          label="Description"
          id="actDescription"
          value={form.description}
          onChange={(v) => setForm({ ...form, description: v })}
          required
        />
        <FormField
          label="Duration (minutes)"
          id="actDuration"
          type="number"
          value={form.durationMinutes}
          onChange={(v) => setForm({ ...form, durationMinutes: v })}
          required
        />
        <FormField
          label="Estimated cost"
          id="actCost"
          type="number"
          value={form.estimatedCost}
          onChange={(v) => setForm({ ...form, estimatedCost: v })}
        />
        <FormField
          label="Currency"
          id="actCurrency"
          value={form.currency}
          onChange={(v) => setForm({ ...form, currency: v })}
        />
        <FormField
          label="Location"
          id="actLocation"
          value={form.location}
          onChange={(v) => setForm({ ...form, location: v })}
          required
        />
        <FormSelect
          label="Difficulty"
          id="actDifficulty"
          value={form.difficulty}
          onChange={(v) => setForm({ ...form, difficulty: v })}
          options={DIFFICULTIES.map((d) => ({ value: d, label: d }))}
        />
        <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '0.875rem' }}>
          <input
            type="checkbox"
            checked={form.isIndoor}
            onChange={(e) => setForm({ ...form, isIndoor: e.target.checked })}
          />
          Indoor activity
        </label>
      </Modal>

      <ConfirmDialog
        open={deactivateTarget !== null}
        title="Deactivate activity?"
        description="It will no longer appear in search results."
        confirmLabel="Deactivate"
        danger
        submitting={deactivateMutation.isPending}
        onConfirm={() => deactivateTarget && deactivateMutation.mutate(deactivateTarget)}
        onClose={() => setDeactivateTarget(null)}
      />
    </>
  )
}