import { Fragment, useState } from 'react'
import type { ToolCall } from '@/types/workflows'
import { formatDuration, formatJson } from '@/utils/format'

interface Props {
  toolCalls: ToolCall[]
  agentNames?: Record<string, string>
}

export function ToolCallTable({ toolCalls, agentNames }: Props) {
  const [expanded, setExpanded] = useState<string | null>(null)

  if (toolCalls.length === 0) {
    return <p style={{ color: 'var(--color-text-muted)' }}>No tool calls recorded.</p>
  }

  return (
    <div className="card" style={{ padding: 0, overflowX: 'auto' }}>
      <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.8125rem' }}>
        <thead>
          <tr style={{ textAlign: 'left', background: '#f8fafc' }}>
            <th style={{ padding: '0.5rem 0.75rem', borderBottom: '1px solid var(--color-border)' }}>Tool</th>
            {agentNames && (
              <th style={{ padding: '0.5rem 0.75rem', borderBottom: '1px solid var(--color-border)' }}>Agent</th>
            )}
            <th style={{ padding: '0.5rem 0.75rem', borderBottom: '1px solid var(--color-border)' }}>Status</th>
            <th style={{ padding: '0.5rem 0.75rem', borderBottom: '1px solid var(--color-border)' }}>Duration</th>
            <th style={{ padding: '0.5rem 0.75rem', borderBottom: '1px solid var(--color-border)' }}></th>
          </tr>
        </thead>
        <tbody>
          {toolCalls.map((tc) => {
            const ok = tc.status.toLowerCase() === 'succeeded'
            const isOpen = expanded === tc.id

            return (
              <Fragment key={tc.id}>
                <tr style={{ borderBottom: '1px solid var(--color-border)' }}>
                  <td style={{ padding: '0.5rem 0.75rem', fontFamily: 'monospace' }}>{tc.toolName}</td>
                  {agentNames && (
                    <td style={{ padding: '0.5rem 0.75rem', color: 'var(--color-text-muted)' }}>
                      {agentNames[tc.toolName] ?? '—'}
                    </td>
                  )}
                  <td style={{ padding: '0.5rem 0.75rem' }}>
                    <span className={`badge ${ok ? 'badge-success' : 'badge-danger'}`}>{tc.status}</span>
                  </td>
                  <td style={{ padding: '0.5rem 0.75rem', color: 'var(--color-text-muted)' }}>
                    {formatDuration(tc.durationMs)}
                  </td>
                  <td style={{ padding: '0.5rem 0.75rem' }}>
                    <button
                      className="btn btn-secondary"
                      style={{ padding: '0.125rem 0.5rem', fontSize: '0.75rem' }}
                      onClick={() => setExpanded(isOpen ? null : tc.id)}
                    >
                      {isOpen ? 'Hide' : 'View'}
                    </button>
                  </td>
                </tr>

                {isOpen && (
                  <tr>
                    <td
                      colSpan={agentNames ? 5 : 4}
                      style={{ padding: '0.75rem', background: '#f8fafc' }}
                    >
                      <div style={{ marginBottom: '0.5rem', fontSize: '0.75rem', fontWeight: 600 }}>
                        Input
                      </div>
                      <pre
                        style={{
                          margin: 0,
                          background: '#0f172a',
                          color: '#e2e8f0',
                          padding: '0.5rem',
                          borderRadius: 'var(--radius-sm)',
                          fontSize: '0.6875rem',
                          overflow: 'auto',
                          maxHeight: '200px',
                        }}
                      >
                        {formatJson(tc.inputJson) || '(none)'}
                      </pre>

                      {tc.outputJson && (
                        <>
                          <div style={{ margin: '0.75rem 0 0.5rem', fontSize: '0.75rem', fontWeight: 600 }}>
                            Output
                          </div>
                          <pre
                            style={{
                              margin: 0,
                              background: '#0f172a',
                              color: '#e2e8f0',
                              padding: '0.5rem',
                              borderRadius: 'var(--radius-sm)',
                              fontSize: '0.6875rem',
                              overflow: 'auto',
                              maxHeight: '240px',
                            }}
                          >
                            {formatJson(tc.outputJson)}
                          </pre>
                        </>
                      )}

                      {tc.errorMessage && (
                        <div
                          style={{
                            marginTop: '0.5rem',
                            padding: '0.5rem',
                            background: '#fee2e2',
                            color: '#991b1b',
                            fontSize: '0.75rem',
                            borderRadius: 'var(--radius-sm)',
                          }}
                        >
                          {tc.errorMessage}
                        </div>
                      )}
                    </td>
                  </tr>
                )}
              </Fragment>
            )
          })}
        </tbody>
      </table>
    </div>
  )
}