import { useState } from 'react'
import { Link, useParams } from 'react-router'
import { ArrowLeft } from 'lucide-react'
import { PageHeader } from '@/components/PageHeader'
import { LoadingState } from '@/components/LoadingState'
import { ErrorState } from '@/components/ErrorState'
import { StatusBadge } from '@/components/StatusBadge'
import { useTrip } from '@/hooks/useTrips'
import { TripBudgetPanel } from '@/components/trip/TripBudgetPanel'
import { TripSchedulePanel } from '@/components/trip/TripSchedulePanel'
import { TripRiskPanel } from '@/components/trip/TripRiskPanel'
import { TripReadinessPanel } from '@/components/trip/TripReadinessPanel'

type Tab = 'budget' | 'schedule' | 'risk' | 'readiness'

export function TripDetailPage() {
  const { id } = useParams<{ id: string }>()
  const { data: trip, isLoading, isError, refetch } = useTrip(id)
  const [tab, setTab] = useState<Tab>('budget')

  if (isLoading) return <LoadingState />
  if (isError || !trip) return <ErrorState message="Failed to load trip." onRetry={() => refetch()} />

  return (
    <>
      <div style={{ marginBottom: '0.5rem' }}>
        <Link to="/trips" style={{ fontSize: '0.875rem', display: 'inline-flex', alignItems: 'center', gap: '0.25rem' }}>
          <ArrowLeft size={14} />
          Back to trips
        </Link>
      </div>

      <PageHeader
        title={trip.title}
        subtitle={`${trip.destination} · ${trip.startDate} → ${trip.endDate} · ${trip.currency}`}
        actions={<StatusBadge status={trip.status} />}
      />

      <div
        style={{
          display: 'flex',
          gap: '0.25rem',
          borderBottom: '1px solid var(--color-border)',
          marginBottom: '1rem',
        }}
      >
        {(
          [
            ['budget', 'Budget'],
            ['schedule', 'Schedule'],
            ['risk', 'Risk'],
            ['readiness', 'Readiness'],
          ] as [Tab, string][]
        ).map(([key, label]) => (
          <button
            key={key}
            onClick={() => setTab(key)}
            style={{
              padding: '0.5rem 0.875rem',
              background: 'none',
              border: 'none',
              borderBottom: tab === key ? '2px solid var(--color-primary)' : '2px solid transparent',
              color: tab === key ? 'var(--color-primary)' : 'var(--color-text-muted)',
              fontWeight: tab === key ? 600 : 400,
              cursor: 'pointer',
              fontSize: '0.875rem',
            }}
          >
            {label}
          </button>
        ))}
      </div>

      {tab === 'budget' && <TripBudgetPanel tripId={trip.id} currency={trip.currency} />}
      {tab === 'schedule' && <TripSchedulePanel tripId={trip.id} />}
      {tab === 'risk' && <TripRiskPanel tripId={trip.id} destination={trip.destination} />}
      {tab === 'readiness' && <TripReadinessPanel tripId={trip.id} />}
    </>
  )
}