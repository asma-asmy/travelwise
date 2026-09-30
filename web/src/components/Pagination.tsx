interface Props {
  page: number
  totalPages: number
  totalCount: number
  onPageChange: (page: number) => void
}

export function Pagination({ page, totalPages, totalCount, onPageChange }: Props) {
  const tp = Math.max(1, totalPages)
  return (
    <div
      style={{
        display: 'flex',
        justifyContent: 'space-between',
        alignItems: 'center',
        marginTop: '0.75rem',
        fontSize: '0.875rem',
      }}
    >
      <div style={{ color: 'var(--color-text-muted)' }}>
        Page {page} of {tp} — {totalCount} total
      </div>
      <div style={{ display: 'flex', gap: '0.5rem' }}>
        <button className="btn btn-secondary" disabled={page <= 1} onClick={() => onPageChange(page - 1)}>
          Previous
        </button>
        <button className="btn btn-secondary" disabled={page >= tp} onClick={() => onPageChange(page + 1)}>
          Next
        </button>
      </div>
    </div>
  )
}