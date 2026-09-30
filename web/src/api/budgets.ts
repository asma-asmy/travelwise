import { apiClient } from '@/api/client'
import type {
  Budget,
  BudgetCategory,
  BudgetHealth,
  CreateExpenseRequest,
  CreateOrUpdateBudgetCategoryRequest,
  CreateOrUpdateBudgetRequest,
  Expense,
  ExpenseQuery,
  UpdateExpenseRequest,
} from '@/types/budgets'
import type { PagedResult } from '@/types/common'

export async function getBudget(tripId: string): Promise<Budget | null> {
  const { data, status } = await apiClient.get<Budget | ''>(
    `/api/v1/trips/${tripId}/budget`,
    { validateStatus: (s) => s === 200 || s === 204 },
  )
  if (status === 204 || !data) return null
  return data as Budget
}

export async function upsertBudget(
  tripId: string,
  payload: CreateOrUpdateBudgetRequest,
): Promise<Budget> {
  const { data } = await apiClient.put<Budget>(`/api/v1/trips/${tripId}/budget`, payload)
  return data
}

export async function listCategories(tripId: string): Promise<BudgetCategory[]> {
  const { data } = await apiClient.get<BudgetCategory[]>(
    `/api/v1/trips/${tripId}/budget/categories`,
  )
  return data
}

export async function addCategory(
  tripId: string,
  payload: CreateOrUpdateBudgetCategoryRequest,
): Promise<BudgetCategory> {
  const { data } = await apiClient.post<BudgetCategory>(
    `/api/v1/trips/${tripId}/budget/categories`,
    payload,
  )
  return data
}

export async function updateCategory(
  tripId: string,
  categoryId: string,
  payload: CreateOrUpdateBudgetCategoryRequest,
): Promise<BudgetCategory> {
  const { data } = await apiClient.put<BudgetCategory>(
    `/api/v1/trips/${tripId}/budget/categories/${categoryId}`,
    payload,
  )
  return data
}

export async function deleteCategory(tripId: string, categoryId: string): Promise<void> {
  await apiClient.delete(`/api/v1/trips/${tripId}/budget/categories/${categoryId}`)
}

export async function listExpenses(
  tripId: string,
  query: ExpenseQuery = {},
): Promise<PagedResult<Expense>> {
  const { data } = await apiClient.get<PagedResult<Expense>>(
    `/api/v1/trips/${tripId}/budget/expenses`,
    { params: query },
  )
  return data
}

export async function addExpense(
  tripId: string,
  payload: CreateExpenseRequest,
): Promise<Expense> {
  const { data } = await apiClient.post<Expense>(
    `/api/v1/trips/${tripId}/budget/expenses`,
    payload,
  )
  return data
}

export async function updateExpense(
  tripId: string,
  expenseId: string,
  payload: UpdateExpenseRequest,
): Promise<Expense> {
  const { data } = await apiClient.put<Expense>(
    `/api/v1/trips/${tripId}/budget/expenses/${expenseId}`,
    payload,
  )
  return data
}

export async function deleteExpense(tripId: string, expenseId: string): Promise<void> {
  await apiClient.delete(`/api/v1/trips/${tripId}/budget/expenses/${expenseId}`)
}

export async function getBudgetHealth(tripId: string): Promise<BudgetHealth> {
  const { data } = await apiClient.get<BudgetHealth>(`/api/v1/trips/${tripId}/budget/health`)
  return data
}