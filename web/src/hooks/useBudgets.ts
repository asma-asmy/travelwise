import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import * as budgetsApi from '@/api/budgets'
import { queryKeys } from '@/hooks/queryKeys'
import type {
  CreateExpenseRequest,
  CreateOrUpdateBudgetCategoryRequest,
  CreateOrUpdateBudgetRequest,
  ExpenseQuery,
  UpdateExpenseRequest,
} from '@/types/budgets'

export function useBudget(tripId: string | undefined) {
  return useQuery({
    queryKey: tripId ? queryKeys.budget.detail(tripId) : ['budget', 'none'],
    queryFn: () => budgetsApi.getBudget(tripId!),
    enabled: Boolean(tripId),
  })
}

export function useBudgetHealth(tripId: string | undefined) {
  return useQuery({
    queryKey: tripId ? queryKeys.budget.health(tripId) : ['budget', 'none', 'health'],
    queryFn: () => budgetsApi.getBudgetHealth(tripId!),
    enabled: Boolean(tripId),
  })
}

export function useExpenses(tripId: string | undefined, query: ExpenseQuery = {}) {
  return useQuery({
    queryKey: tripId ? queryKeys.budget.expenses(tripId, query) : ['budget', 'none', 'expenses'],
    queryFn: () => budgetsApi.listExpenses(tripId!, query),
    enabled: Boolean(tripId),
  })
}

export function useUpsertBudget(tripId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateOrUpdateBudgetRequest) => budgetsApi.upsertBudget(tripId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.budget.root(tripId) })
    },
  })
}

export function useAddCategory(tripId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateOrUpdateBudgetCategoryRequest) =>
      budgetsApi.addCategory(tripId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.budget.root(tripId) })
    },
  })
}

export function useUpdateCategory(tripId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ categoryId, payload }: { categoryId: string; payload: CreateOrUpdateBudgetCategoryRequest }) =>
      budgetsApi.updateCategory(tripId, categoryId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.budget.root(tripId) })
    },
  })
}

export function useDeleteCategory(tripId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (categoryId: string) => budgetsApi.deleteCategory(tripId, categoryId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.budget.root(tripId) })
    },
  })
}

export function useAddExpense(tripId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (payload: CreateExpenseRequest) => budgetsApi.addExpense(tripId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.budget.root(tripId) })
    },
  })
}

export function useUpdateExpense(tripId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: ({ expenseId, payload }: { expenseId: string; payload: UpdateExpenseRequest }) =>
      budgetsApi.updateExpense(tripId, expenseId, payload),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.budget.root(tripId) })
    },
  })
}

export function useDeleteExpense(tripId: string) {
  const qc = useQueryClient()
  return useMutation({
    mutationFn: (expenseId: string) => budgetsApi.deleteExpense(tripId, expenseId),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: queryKeys.budget.root(tripId) })
    },
  })
}