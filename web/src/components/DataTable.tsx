interface Column<T> {
  key: string
  header: string
  render: (row: T) => React.ReactNode
  width?: string
  align?: 'left' | 'right' | 'center'
}

interface Props<T> {
  columns: Column<T>[]
  rows: T[]
  getRowKey: (row: T) => string
  emptyMessage?: string
}

export function DataTable<T>({ columns, rows, getRowKey, emptyMessage = 'No records.' }: Props<T>) {
  if (rows.length === 0) {
    return (
      <div className="card" style={{ textAlign: 'center', padding: '2rem', color: 'var(--color-text-muted)' }}>
        {emptyMessage}
      </div>
    )
  }

  return (
    <div className="card" style={{ padding: 0, overflowX: 'auto' }}>
      <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.875rem' }}>
        <thead>
          <tr style={{ textAlign: 'left', background: '#f8fafc' }}>
            {columns.map((c) => (
              <th
                key={c.key}
                style={{
                  padding: '0.5rem 0.75rem',
                  borderBottom: '1px solid var(--color-border)',
                  textAlign: c.align ?? 'left',
                  width: c.width,
                  fontWeight: 600,
                  fontSize: '0.8125rem',
                }}
              >
                {c.header}
              </th>
            ))}
          </tr>
        </thead>
        <tbody>
          {rows.map((row) => (
            <tr key={getRowKey(row)} style={{ borderBottom: '1px solid var(--color-border)' }}>
              {columns.map((c) => (
                <td
                  key={c.key}
                  style={{
                    padding: '0.5rem 0.75rem',
                    textAlign: c.align ?? 'left',
                    verticalAlign: 'middle',
                  }}
                >
                  {c.render(row)}
                </td>
              ))}
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  )
}