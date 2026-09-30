import { BrowserRouter, Navigate, Route, Routes } from 'react-router'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'

import { AuthBootstrap } from '@/components/AuthBootstrap'
import { ProtectedRoute } from '@/components/ProtectedRoute'
import { ToastContainer } from '@/components/ToastContainer'
import { AppLayout } from '@/layouts/AppLayout'
import { AuthLayout } from '@/layouts/AuthLayout'

import { DashboardPage } from '@/pages/DashboardPage'
import { ForbiddenPage } from '@/pages/ForbiddenPage'
import { NotFoundPage } from '@/pages/NotFoundPage'
import { TripsListPage } from '@/pages/TripsListPage'
import { TripDetailPage } from '@/pages/TripDetailPage'
import { MyDocumentsPage } from '@/pages/MyDocumentsPage'
import { PreferencesPage } from '@/pages/PreferencesPage'
import { WorkflowsListPage } from '@/pages/WorkflowsListPage'
import { WorkflowDetailPage } from '@/pages/WorkflowDetailPage'
import { AlertsPage } from '@/pages/AlertsPage'
import { ActivitiesAdminPage } from '@/pages/ActivitiesAdminPage'
import { UsersAdminPage } from '@/pages/admin/UsersAdminPage'
import { RequirementsAdminPage } from '@/pages/admin/RequirementsAdminPage'
import { SettingsAdminPage } from '@/pages/admin/SettingsAdminPage'
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
        <ToastContainer />
        <AuthBootstrap>
          <Routes>
            {/* Public — auth screens */}
            <Route element={<AuthLayout />}>
              <Route path="/login" element={<LoginPage />} />
              <Route path="/register" element={<RegisterPage />} />
            </Route>

            {/* Protected — everything else */}
            <Route
              element={
                <ProtectedRoute>
                  <AppLayout />
                </ProtectedRoute>
              }
            >
              <Route path="/" element={<Navigate to="/dashboard" replace />} />
              <Route path="/dashboard" element={<DashboardPage />} />

              {/* My Work */}
              <Route path="/trips" element={<TripsListPage />} />
              <Route path="/trips/:id" element={<TripDetailPage />} />
              <Route path="/documents" element={<MyDocumentsPage />} />
              <Route path="/preferences" element={<PreferencesPage />} />

              {/* Operations — staff only */}
              <Route
                path="/workflows"
                element={
                  <ProtectedRoute requireAnyRole={['TravelOpsStaff', 'Admin']}>
                    <WorkflowsListPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/workflows/:id"
                element={
                  <ProtectedRoute requireAnyRole={['TravelOpsStaff', 'Admin']}>
                    <WorkflowDetailPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/alerts"
                element={
                  <ProtectedRoute requireAnyRole={['TravelOpsStaff', 'Admin']}>
                    <AlertsPage />
                  </ProtectedRoute>
                }
              />

              {/* Catalogue — admin only */}
              <Route
                path="/activities"
                element={
                  <ProtectedRoute requireAnyRole={['Admin']}>
                    <ActivitiesAdminPage />
                  </ProtectedRoute>
                }
              />

              {/* Admin — admin only */}
              <Route
                path="/admin/users"
                element={
                  <ProtectedRoute requireAnyRole={['Admin']}>
                    <UsersAdminPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/requirements"
                element={
                  <ProtectedRoute requireAnyRole={['Admin']}>
                    <RequirementsAdminPage />
                  </ProtectedRoute>
                }
              />
              <Route
                path="/admin/settings"
                element={
                  <ProtectedRoute requireAnyRole={['Admin']}>
                    <SettingsAdminPage />
                  </ProtectedRoute>
                }
              />
            </Route>

            {/* Public — error pages */}
            <Route path="/forbidden" element={<ForbiddenPage />} />
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </AuthBootstrap>
      </BrowserRouter>
    </QueryClientProvider>
  )
}