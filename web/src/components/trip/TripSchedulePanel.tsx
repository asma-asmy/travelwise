import { LoadingState } from '@/components/LoadingState'
import { EmptyState } from '@/components/EmptyState'
import { StatusBadge } from '@/components/StatusBadge'
import { useSchedule, useScheduleConflicts } from '@/hooks/useActivities'

interface Props {
  tripId: string
}

export function TripSchedulePanel({ tripId }: Props) {
  const schedule = useSchedule(tripId)
  const conflicts = useScheduleConflicts(tripId)

  if (schedule.isLoading) return <LoadingState />

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
      {conflicts.data && conflicts.data.length > 0 && (
        <div
          className="card"
          style={{ borderColor: 'var(--color-danger)', background: '#fee2e2', color: '#991b1b' }}
        >
          <strong>{conflicts.data.length} scheduling conflict(s) detected.</strong>
          <ul style={{ marginTop: '0.25rem', paddingLeft: '1.25rem', fontSize: '0.8125rem' }}>
            {conflicts.data.map((c, i) => (
              <li key={i}>
                {c.conflictingActivityName} — {c.reason} on {c.scheduledDate}
              </li>
            ))}
          </ul>
        </div>
      )}

      {schedule.data?.length === 0 ? (
        <EmptyState title="No activities scheduled yet" description="Add activities to the trip itinerary." />
      ) : (
        <div className="card" style={{ padding: 0 }}>
          <ul style={{ listStyle: 'none', margin: 0, padding: 0 }}>
            {schedule.data?.map((s, i) => (
              <li
                key={s.id}
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
                  <div style={{ fontWeight: 600 }}>{s.activityName}</div>
                  <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>
                    {s.scheduledDate} · {s.startTime}–{s.endTime} · {s.location}
                  </div>
                </div>
                <StatusBadge status={s.status} />
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  )
}