import { Skeleton } from '@/components/Skeleton'

export function SkeletonCard() {
  return (
    <div className="card">
      <Skeleton width="40%" height="0.875rem" style={{ marginBottom: '0.75rem' }} />
      <Skeleton width="80%" height="1.5rem" style={{ marginBottom: '0.5rem' }} />
      <Skeleton width="60%" height="0.75rem" />
    </div>
  )
}