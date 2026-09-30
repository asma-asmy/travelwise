import { BrowserRouter, Navigate, Route, Routes } from 'react-router'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

import { AuthBootstrap } from '@/components/AuthBootstrap'
import { ProtectedRoute } from '@/components/ProtectedRoute'
import { AppLayout } from '@/layouts/AppLayout'
import { AuthLayout } from '@/layouts/AuthLayout'
import { DashboardPage } from '@/pages/DashboardPage'
import { ForbiddenPage } from '@/pages/ForbiddenPage'
import { NotFoundPage } from '@/pages/NotFoundPage'
import { LoginPage } from '@/pages/auth/LoginPage'
import { RegisterPage } from '@/pages/auth/RegisterPage'

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 30_000,
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
})

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <BrowserRouter>
        <AuthBootstrap>
          <Routes>
            <Route element={<AuthLayout />}>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
            </Route>

            <Route
              element={
                <ProtectedRoute>
                  <AppLayout />
                </ProtectedRoute>
              }
            >
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              <Route path="/dashboard" element={<DashboardPage />} />
            </Route>

            <Route path="/forbidden" element={<ForbiddenPage />} />
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </AuthBootstrap>
      </BrowserRouter>
    </QueryClientProvider>
  )
}