import { Skeleton } from '@/components/Skeleton'

interface Props {
  rows?: number
  columns?: number
}

export function SkeletonTable({ rows = 5, columns = 4 }: Props) {
  return (
    <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
      <div
        style={{
          padding: '0.5rem 0.75rem',
          background: '#f8fafc',
          display: 'grid',
          gridTemplateColumns: `repeat(${columns}, 1fr)`,
          gap: '1rem',
        }}
      >
        {Array.from({ length: columns }).map((_, i) => (
          <Skeleton key={i} height="0.875rem" />
        ))}
      </div>
      {Array.from({ length: rows }).map((_, r) => (
        <div
          key={r}
          style={{
            padding: '0.75rem',
            borderTop: '1px solid var(--color-border)',
            display: 'grid',
            gridTemplateColumns: `repeat(${columns}, 1fr)`,
            gap: '1rem',
          }}
        >
          {Array.from({ length: columns }).map((_, c) => (
            <Skeleton key={c} height="0.875rem" />
          ))}
        </div>
      ))}
    </div>
  )
}