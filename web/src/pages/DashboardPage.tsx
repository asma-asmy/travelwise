import { Link } from 'react-router'
import { Map, GitBranch, Bell, ArrowRight, Plus, AlertTriangle } from 'lucide-react'
import { PageHeader } from '@/components/PageHeader'
import { StatCard } from '@/components/StatCard'
import { StatusBadge } from '@/components/StatusBadge'
import { LoadingState } from '@/components/LoadingState'
import { ErrorState } from '@/components/ErrorState'
import { EmptyState } from '@/components/EmptyState'
import { useDashboardData } from '@/hooks/useDashboard'
import { useAuthStore } from '@/stores/authStore'

export function DashboardPage() {
  const fullName = useAuthStore((s) => s.fullName)
  const roles = useAuthStore((s) => s.roles)
  const { isStaff, trips, pendingApprovals, alerts } = useDashboardData()

  const tripsCount = trips.data?.length ?? 0
  const alertsCount = alerts.data?.totalCount ?? 0
  const pendingCount = pendingApprovals?.data?.totalCount ?? 0

  return (
    <>
      <PageHeader
        title={`Welcome, ${fullName}`}
        subtitle={`Signed in as ${roles.join(', ')}`}
        actions={
          <Link to="/trips" className="btn btn-primary">
            <Plus size={16} />
            New Trip
          </Link>
        }
      />

      {/* Stat cards */}
      <div
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))',
          gap: '1rem',
          marginBottom: '1.5rem',
        }}
      >
        <StatCard
        label={isStaff ? 'All Trips' : 'My Trips'}
        value={trips.isLoading ? '…' : tripsCount}
        icon={Map}
        tone="info"
        hint={isStaff ? 'Across all travellers' : undefined}
        />
        {isStaff && (
          <StatCard
            label="Pending Approvals"
            value={pendingApprovals?.isLoading ? '…' : pendingCount}
            icon={GitBranch}
            tone={pendingCount > 0 ? 'warning' : 'success'}
            hint={pendingCount > 0 ? 'Requires your attention' : 'All caught up'}
          />
        )}
        <StatCard
          label="Active Alerts"
          value={alerts.isLoading ? '…' : alertsCount}
          icon={Bell}
          tone={alertsCount > 0 ? 'danger' : 'success'}
        />
      </div>

      {/* Pending approvals — staff only */}
      {isStaff && (
        <section style={{ marginBottom: '1.5rem' }}>
          <div
            style={{
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              marginBottom: '0.75rem',
            }}
          >
            <h2 style={{ fontSize: '1.125rem', fontWeight: 600 }}>Pending Approvals</h2>
            <Link
              to="/workflows"
              style={{ fontSize: '0.875rem', display: 'inline-flex', alignItems: 'center', gap: '0.25rem' }}
            >
              View all <ArrowRight size={14} />
            </Link>
          </div>

          {pendingApprovals?.isLoading && <LoadingState />}
          {pendingApprovals?.isError && (
            <ErrorState
              message="Failed to load pending approvals."
              onRetry={() => pendingApprovals.refetch()}
            />
          )}
          {pendingApprovals?.data && pendingApprovals.data.items.length === 0 && (
            <EmptyState
              title="No pending approvals"
              description="Recovery workflows that need your decision will appear here."
            />
          )}
          {pendingApprovals?.data && pendingApprovals.data.items.length > 0 && (
            <div className="card" style={{ padding: 0 }}>
              <ul style={{ listStyle: 'none', margin: 0, padding: 0 }}>
                {pendingApprovals.data.items.map((w, i) => (
                  <li
                    key={w.id}
                    style={{
                      padding: '0.75rem 1rem',
                      borderTop: i === 0 ? 'none' : '1px solid var(--color-border)',
                      display: 'flex',
                      justifyContent: 'space-between',
                      alignItems: 'center',
                      gap: '1rem',
                    }}
                  >
                    <div style={{ minWidth: 0 }}>
                      <div style={{ fontWeight: 600, fontSize: '0.875rem', marginBottom: '0.125rem' }}>
                        {w.objective}
                      </div>
                      <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
                        {new Date(w.createdAt).toLocaleString()}
                      </div>
                    </div>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                      <StatusBadge status={w.status} />
                      <Link to={`/workflows/${w.id}`} className="btn btn-secondary">
                        Review
                      </Link>
                    </div>
                  </li>
                ))}
              </ul>
            </div>
          )}
        </section>
      )}

      {/* My recent trips */}
      <section>
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            marginBottom: '0.75rem',
          }}
        >
        <h2 style={{ fontSize: '1.125rem', fontWeight: 600 }}>
            {isStaff ? 'Recent Trips (All Users)' : 'Recent Trips'}
        </h2>
          <Link
            to="/trips"
            style={{ fontSize: '0.875rem', display: 'inline-flex', alignItems: 'center', gap: '0.25rem' }}
          >
            View all <ArrowRight size={14} />
          </Link>
        </div>

        {trips.isLoading && <LoadingState />}
        {trips.isError && (
          <ErrorState message="Failed to load trips." onRetry={() => trips.refetch()} />
        )}
        {trips.data && trips.data.length === 0 && (
          <EmptyState
            title="No trips yet"
            description="Create a trip to start planning."
            action={
              <Link to="/trips" className="btn btn-primary">
                <Plus size={16} />
                New Trip
              </Link>
            }
          />
        )}
        {trips.data && trips.data.length > 0 && (
          <div className="card" style={{ padding: 0 }}>
            <ul style={{ listStyle: 'none', margin: 0, padding: 0 }}>
              {trips.data.slice(0, 5).map((t, i) => (
                <li
                  key={t.id}
                  style={{
                    padding: '0.75rem 1rem',
                    borderTop: i === 0 ? 'none' : '1px solid var(--color-border)',
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    gap: '1rem',
                  }}
                >
                  <div style={{ minWidth: 0 }}>
                    <div style={{ fontWeight: 600, fontSize: '0.875rem', marginBottom: '0.125rem' }}>
                      {t.title}
                    </div>
                    <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
                      {t.destination} · {t.startDate} → {t.endDate}
                    </div>
                  </div>
                  <StatusBadge status={t.status} />
                </li>
              ))}
            </ul>
          </div>
        )}
      </section>

      {/* Alerts summary */}
      {alerts.data && alerts.data.items.length > 0 && (
        <section style={{ marginTop: '1.5rem' }}>
          <h2 style={{ fontSize: '1.125rem', fontWeight: 600, marginBottom: '0.75rem' }}>
            Recent Alerts
          </h2>
          <div className="card" style={{ padding: 0 }}>
            <ul style={{ listStyle: 'none', margin: 0, padding: 0 }}>
              {alerts.data.items.slice(0, 5).map((a, i) => (
                <li
                  key={a.id}
                  style={{
                    padding: '0.75rem 1rem',
                    borderTop: i === 0 ? 'none' : '1px solid var(--color-border)',
                    display: 'flex',
                    justifyContent: 'space-between',
                    alignItems: 'center',
                    gap: '1rem',
                  }}
                >
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', minWidth: 0 }}>
                    <AlertTriangle size={16} />
                    <div style={{ minWidth: 0 }}>
                      <div style={{ fontSize: '0.875rem' }}>
                        <strong>{a.destination}</strong> — {a.message}
                      </div>
                      <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
                        {a.category}
                      </div>
                    </div>
                  </div>
                  <StatusBadge status={a.severityName} />
                </li>
              ))}
            </ul>
          </div>
        </section>
      )}
    </>
  )
}