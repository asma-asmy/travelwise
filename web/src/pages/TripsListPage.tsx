import { Link } from 'react-router'
import { Plus } from 'lucide-react'
import { PageHeader } from '@/components/PageHeader'
import { ErrorState } from '@/components/ErrorState'
import { EmptyState } from '@/components/EmptyState'
import { StatusBadge } from '@/components/StatusBadge'
import { DataTable } from '@/components/DataTable'
import { useTrips } from '@/hooks/useTrips'
import { useAuthStore } from '@/stores/authStore'
import type { Trip } from '@/types/trips'
import { SkeletonTable } from '@/components/SkeletonTable'

export function TripsListPage() {
  const roles = useAuthStore((s) => s.roles)
  const isStaff = roles.includes('TravelOpsStaff') || roles.includes('Admin')
  const { data, isLoading, isError, refetch } = useTrips()

  return (
    <>
      <PageHeader
        title="Trips"
        subtitle={isStaff ? 'Trips you own.' : 'Your trips.'}
        actions={
          <Link to="/trips/new" className="btn btn-primary">
            <Plus size={16} />
            New Trip
          </Link>
        }
      />

      {isLoading && <SkeletonTable rows={5} columns={5} />}
      {isError && <ErrorState message="Failed to load trips." onRetry={() => refetch()} />}
      {data && data.length === 0 && (
        <EmptyState
          title="No trips yet"
          description="Create a trip to start planning."
          action={
            <Link to="/trips/new" className="btn btn-primary">
              <Plus size={16} />
              New Trip
            </Link>
          }
        />
      )}
      {data && data.length > 0 && (
        <DataTable<Trip>
          rows={data}
          getRowKey={(t) => t.id}
          columns={[
            {
              key: 'title',
              header: 'Title',
              render: (t) => (
                <Link to={`/trips/${t.id}`} style={{ fontWeight: 600 }}>
                  {t.title}
                </Link>
              ),
            },
            { key: 'destination', header: 'Destination', render: (t) => t.destination },
            {
              key: 'dates',
              header: 'Dates',
              render: (t) => (
                <span style={{ color: 'var(--color-text-muted)', fontSize: '0.8125rem' }}>
                  {t.startDate} → {t.endDate}
                </span>
              ),
            },
            { key: 'type', header: 'Type', render: (t) => t.tripType },
            {
              key: 'status',
              header: 'Status',
              render: (t) => <StatusBadge status={t.status} />,
            },
            {
              key: 'actions',
              header: '',
              align: 'right',
              render: (t) => (
                <Link to={`/trips/${t.id}`} className="btn btn-secondary">
                  Open
                </Link>
              ),
            },
          ]}
        />
      )}
    </>
  )
}