import { useState } from 'react'
import { Plus, Trash2 } from 'lucide-react'
import { LoadingState } from '@/components/LoadingState'
import { EmptyState } from '@/components/EmptyState'
import { Modal } from '@/components/Modal'
import { FormField, FormSelect } from '@/components/FormField'
import { ConfirmDialog } from '@/components/ConfirmDialog'
import { useToastStore } from '@/stores/toastStore'
import {
  useAddCategory,
  useAddExpense,
  useBudget,
  useBudgetHealth,
  useDeleteCategory,
  useDeleteExpense,
  useExpenses,
  useUpsertBudget,
} from '@/hooks/useBudgets'
import { formatCurrency, formatDateTime } from '@/utils/format'

interface Props {
  tripId: string
  currency: string
}

export function TripBudgetPanel({ tripId, currency }: Props) {
  const budget = useBudget(tripId)
  const health = useBudgetHealth(tripId)
  const expenses = useExpenses(tripId, { page: 1, pageSize: 50, sortBy: 'SpentAt', sortDir: 'desc' })

  const upsert = useUpsertBudget(tripId)
  const addCategory = useAddCategory(tripId)
  const deleteCategory = useDeleteCategory(tripId)
  const addExpense = useAddExpense(tripId)
  const deleteExpense = useDeleteExpense(tripId)
  const push = useToastStore((s) => s.push)

  const [budgetOpen, setBudgetOpen] = useState(false)
  const [categoryOpen, setCategoryOpen] = useState(false)
  const [expenseOpen, setExpenseOpen] = useState(false)
  const [deleteCategoryTarget, setDeleteCategoryTarget] = useState<string | null>(null)
  const [deleteExpenseTarget, setDeleteExpenseTarget] = useState<string | null>(null)

  const [budgetAmount, setBudgetAmount] = useState('')
  const [categoryName, setCategoryName] = useState('')
  const [categoryLimit, setCategoryLimit] = useState('')
  const [expenseAmount, setExpenseAmount] = useState('')
  const [expenseDescription, setExpenseDescription] = useState('')
  const [expenseCategoryId, setExpenseCategoryId] = useState('')
  const [expenseSpentAt, setExpenseSpentAt] = useState(new Date().toISOString().slice(0, 16))

  if (budget.isLoading) return <LoadingState />

  const budgetData = budget.data
  const hasBudget = Boolean(budgetData)

  // ---------- handlers ----------

  async function handleUpsertBudget() {
    try {
      await upsert.mutateAsync({ totalAmount: Number(budgetAmount), currency })
      push('success', 'Budget saved.')
      setBudgetOpen(false)
      setBudgetAmount('')
    } catch {
      push('error', 'Failed to save budget.')
    }
  }

  async function handleAddCategory() {
    try {
      await addCategory.mutateAsync({ name: categoryName, limitAmount: Number(categoryLimit) })
      push('success', 'Category added.')
      setCategoryOpen(false)
      setCategoryName('')
      setCategoryLimit('')
    } catch (e: unknown) {
      const title = (e as { response?: { data?: { title?: string } } })?.response?.data?.title
      push('error', title ?? 'Failed to add category.')
    }
  }

  async function handleAddExpense() {
    try {
      await addExpense.mutateAsync({
        amount: Number(expenseAmount),
        currency,
        description: expenseDescription,
        budgetCategoryId: expenseCategoryId || null,
        spentAt: new Date(expenseSpentAt).toISOString(),
      })
      push('success', 'Expense added.')
      setExpenseOpen(false)
      setExpenseAmount('')
      setExpenseDescription('')
      setExpenseCategoryId('')
      setExpenseSpentAt(new Date().toISOString().slice(0, 16))
    } catch (e: unknown) {
      const title = (e as { response?: { data?: { title?: string } } })?.response?.data?.title
      push('error', title ?? 'Failed to add expense.')
    }
  }

  async function handleDeleteCategory() {
    if (!deleteCategoryTarget) return
    try {
      await deleteCategory.mutateAsync(deleteCategoryTarget)
      push('success', 'Category deleted.')
    } catch {
      push('error', 'Failed to delete category.')
    } finally {
      setDeleteCategoryTarget(null)
    }
  }

  async function handleDeleteExpense() {
    if (!deleteExpenseTarget) return
    try {
      await deleteExpense.mutateAsync(deleteExpenseTarget)
      push('success', 'Expense deleted.')
    } catch {
      push('error', 'Failed to delete expense.')
    } finally {
      setDeleteExpenseTarget(null)
    }
  }

  // ---------- render ----------

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      {/* Budget summary */}
      <section>
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
          <h3 style={{ fontSize: '1rem', fontWeight: 600 }}>Budget</h3>
          <button
            className="btn btn-primary"
            onClick={() => {
              setBudgetAmount(budgetData?.totalAmount.toString() ?? '')
              setBudgetOpen(true)
            }}
          >
            {hasBudget ? 'Edit budget' : 'Set budget'}
          </button>
        </div>
        {!hasBudget ? (
          <EmptyState title="No budget yet" description="Set a total budget to start tracking expenses." />
        ) : (
          <div className="card">
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(140px, 1fr))', gap: '1rem' }}>
              <div>
                <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)', textTransform: 'uppercase' }}>
                  Total
                </div>
                <div style={{ fontWeight: 700 }}>{formatCurrency(budgetData!.totalAmount, currency)}</div>
              </div>
              <div>
                <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)', textTransform: 'uppercase' }}>
                  Spent
                </div>
                <div style={{ fontWeight: 700 }}>{formatCurrency(budgetData!.totalSpent, currency)}</div>
              </div>
              <div>
                <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)', textTransform: 'uppercase' }}>
                  Remaining
                </div>
                <div
                  style={{
                    fontWeight: 700,
                    color: budgetData!.remainingAmount < 0 ? 'var(--color-danger)' : 'inherit',
                  }}
                >
                  {formatCurrency(budgetData!.remainingAmount, currency)}
                </div>
              </div>
              {health.data && (
                <div>
                  <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)', textTransform: 'uppercase' }}>
                    Health
                  </div>
                  <div style={{ fontWeight: 700 }}>{health.data.healthStatus}</div>
                </div>
              )}
            </div>
            {health.data?.warnings && health.data.warnings.length > 0 && (
              <ul style={{ marginTop: '0.75rem', paddingLeft: '1.25rem', fontSize: '0.8125rem', color: 'var(--color-warning)' }}>
                {health.data.warnings.map((w, i) => (
                  <li key={i}>{w}</li>
                ))}
              </ul>
            )}
          </div>
        )}
      </section>

      {/* Categories */}
      {hasBudget && (
        <section>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
            <h3 style={{ fontSize: '1rem', fontWeight: 600 }}>Categories</h3>
            <button className="btn btn-secondary" onClick={() => setCategoryOpen(true)}>
              <Plus size={14} /> Add category
            </button>
          </div>
          {budgetData!.categories.length === 0 ? (
            <EmptyState title="No categories yet" />
          ) : (
            <div className="card" style={{ padding: 0 }}>
              <ul style={{ listStyle: 'none', margin: 0, padding: 0 }}>
                {budgetData!.categories.map((c, i) => (
                  <li
                    key={c.id}
                    style={{
                      padding: '0.625rem 0.875rem',
                      borderTop: i === 0 ? 'none' : '1px solid var(--color-border)',
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                      gap: '1rem',
                    }}
                  >
                    <div>
                      <div style={{ fontWeight: 600 }}>{c.name}</div>
                      <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
                        {formatCurrency(c.spentAmount, currency)} / {formatCurrency(c.limitAmount, currency)}
                        {c.isOverLimit && <span style={{ color: 'var(--color-danger)', marginLeft: '0.5rem' }}>· Over limit</span>}
                      </div>
                    </div>
                    <button
                      className="btn btn-secondary"
                      onClick={() => setDeleteCategoryTarget(c.id)}
                      aria-label={`Delete ${c.name}`}
                    >
                      <Trash2 size={14} />
                    </button>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </section>
      )}

      {/* Expenses */}
      {hasBudget && (
        <section>
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
            <h3 style={{ fontSize: '1rem', fontWeight: 600 }}>Expenses</h3>
            <button className="btn btn-secondary" onClick={() => setExpenseOpen(true)}>
              <Plus size={14} /> Add expense
            </button>
          </div>
          {expenses.data?.items.length === 0 ? (
            <EmptyState title="No expenses yet" />
          ) : (
            <div className="card" style={{ padding: 0 }}>
              <ul style={{ listStyle: 'none', margin: 0, padding: 0 }}>
                {expenses.data?.items.map((e, i) => (
                  <li
                    key={e.id}
                    style={{
                      padding: '0.625rem 0.875rem',
                      borderTop: i === 0 ? 'none' : '1px solid var(--color-border)',
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                      gap: '1rem',
                    }}
                  >
                    <div>
                      <div style={{ fontWeight: 600 }}>{e.description}</div>
                      <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
                        {e.budgetCategoryName ?? 'Uncategorized'} · {formatDateTime(e.spentAt)}
                      </div>
                    </div>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                      <strong>{formatCurrency(e.amount, currency)}</strong>
                      <button
                        className="btn btn-secondary"
                        onClick={() => setDeleteExpenseTarget(e.id)}
                        aria-label={`Delete expense`}
                      >
                        <Trash2 size={14} />
                      </button>
                    </div>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </section>
      )}

      {/* Modals */}
      <Modal
        open={budgetOpen}
        title={hasBudget ? 'Edit budget' : 'Set budget'}
        onClose={() => setBudgetOpen(false)}
        footer={
          <>
            <button className="btn btn-secondary" onClick={() => setBudgetOpen(false)}>Cancel</button>
            <button className="btn btn-primary" onClick={handleUpsertBudget} disabled={upsert.isPending}>
              {upsert.isPending ? 'Saving…' : 'Save'}
            </button>
          </>
        }
      >
        <FormField
          label="Total budget"
          id="budgetAmount"
          type="number"
          value={budgetAmount}
          onChange={setBudgetAmount}
          required
        />
      </Modal>

      <Modal
        open={categoryOpen}
        title="Add category"
        onClose={() => setCategoryOpen(false)}
        footer={
          <>
            <button className="btn btn-secondary" onClick={() => setCategoryOpen(false)}>Cancel</button>
            <button className="btn btn-primary" onClick={handleAddCategory} disabled={addCategory.isPending}>
              {addCategory.isPending ? 'Adding…' : 'Add'}
            </button>
          </>
        }
      >
        <FormField label="Name" id="catName" value={categoryName} onChange={setCategoryName} required />
        <FormField
          label="Limit amount"
          id="catLimit"
          type="number"
          value={categoryLimit}
          onChange={setCategoryLimit}
          required
        />
      </Modal>

      <Modal
        open={expenseOpen}
        title="Add expense"
        onClose={() => setExpenseOpen(false)}
        footer={
          <>
            <button className="btn btn-secondary" onClick={() => setExpenseOpen(false)}>Cancel</button>
            <button className="btn btn-primary" onClick={handleAddExpense} disabled={addExpense.isPending}>
              {addExpense.isPending ? 'Adding…' : 'Add'}
            </button>
          </>
        }
      >
        <FormField
          label="Amount"
          id="expAmt"
          type="number"
          value={expenseAmount}
          onChange={setExpenseAmount}
          required
        />
        <FormField
          label="Description"
          id="expDesc"
          value={expenseDescription}
          onChange={setExpenseDescription}
          required
        />
        <FormSelect
          label="Category"
          id="expCat"
          value={expenseCategoryId}
          onChange={setExpenseCategoryId}
          options={[
            { value: '', label: 'Uncategorized' },
            ...(budgetData?.categories.map((c) => ({ value: c.id, label: c.name })) ?? []),
          ]}
        />
        <FormField          label="Spent at"
          id="expSpentAt"
          type="datetime-local"
          value={expenseSpentAt}
          onChange={setExpenseSpentAt}
        />
      </Modal>

      <ConfirmDialog
        open={deleteCategoryTarget !== null}
        title="Delete category?"
        description="Expenses tied to this category will become uncategorized."
        confirmLabel="Delete"
        danger
        submitting={deleteCategory.isPending}
        onConfirm={handleDeleteCategory}
        onClose={() => setDeleteCategoryTarget(null)}
      />

      <ConfirmDialog
        open={deleteExpenseTarget !== null}
        title="Delete expense?"
        confirmLabel="Delete"
        danger
        submitting={deleteExpense.isPending}
        onConfirm={handleDeleteExpense}
        onClose={() => setDeleteExpenseTarget(null)}
      />
    </div>
  )
}