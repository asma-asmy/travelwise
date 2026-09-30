export interface Budget {
  id: string
  tripId: string
  totalAmount: number
  currency: string
  totalSpent: number
  remainingAmount: number
  categories: BudgetCategory[]
  createdAt: string
  updatedAt: string
}

export interface BudgetCategory {
  id: string
  budgetId: string
  name: string
  limitAmount: number
  spentAmount: number
  remainingAmount: number
  isOverLimit: boolean
}

export interface Expense {
  id: string
  tripId: string
  budgetCategoryId: string | null
  budgetCategoryName: string | null
  amount: number
  currency: string
  description: string
  spentAt: string
  createdAt: string
}

export interface BudgetHealth {
  tripId: string
  healthStatus: 'HEALTHY' | 'WATCH' | 'AT_RISK' | 'OVER'
  totalBudget: number
  totalSpent: number
  remainingAmount: number
  tripDaysTotal: number
  tripDaysElapsed: number
  tripDaysRemaining: number
  averageDailySpend: number
  recommendedDailyCap: number
  projectedEndBalance: number
  categoryOverruns: {
    categoryId: string
    categoryName: string
    limitAmount: number
    spentAmount: number
    overrunAmount: number
    utilizationPercent: number
  }[]
  warnings: string[]
}

export interface CreateOrUpdateBudgetRequest {
  totalAmount: number
  currency: string
}

export interface CreateOrUpdateBudgetCategoryRequest {
  name: string
  limitAmount: number
}

export interface CreateExpenseRequest {
  budgetCategoryId?: string | null
  amount: number
  currency: string
  description: string
  spentAt?: string
}

export interface UpdateExpenseRequest {
  budgetCategoryId?: string | null
  amount: number
  currency: string
  description: string
  spentAt: string
}

export interface ExpenseQuery {
  search?: string
  budgetCategoryId?: string
  fromDate?: string
  toDate?: string
  minAmount?: number
  maxAmount?: number
  sortBy?: 'SpentAt' | 'Amount'
  sortDir?: 'asc' | 'desc'
  page?: number
  pageSize?: number
}