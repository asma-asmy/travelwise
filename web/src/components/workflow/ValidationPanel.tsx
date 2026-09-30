import type { ValidationResult } from '@/types/workflows'
import { formatDateTime } from '@/utils/format'

interface Props {
  validations: ValidationResult[]
}

interface RuleViolation {
  rule: string
  severity: string
  details: string
}

interface ValidationDetails {
  passed: boolean
  violations: RuleViolation[]
  checked_rules: string[]
}

function parseDetails(json: string): ValidationDetails | null {
  try {
    const parsed = JSON.parse(json)
    return {
      passed: Boolean(parsed.passed),
      violations: Array.isArray(parsed.violations) ? parsed.violations : [],
      checked_rules: Array.isArray(parsed.checked_rules) ? parsed.checked_rules : [],
    }
  } catch {
    return null
  }
}

export function ValidationPanel({ validations }: Props) {
  if (validations.length === 0) {
    return <p style={{ color: 'var(--color-text-muted)' }}>No validation results yet.</p>
  }

  return (
    <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
      {validations.map((v) => {
        const parsed = parseDetails(v.detailsJson)
        return (
          <div key={v.id} className="card">
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <div style={{ fontWeight: 600 }}>{v.validatorName}</div>
              <span className={`badge ${v.passed ? 'badge-success' : 'badge-danger'}`}>
                {v.passed ? 'Passed' : 'Failed'}
              </span>
            </div>
            <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)', marginTop: '0.25rem' }}>
              {formatDateTime(v.createdAt)}
            </div>

            {parsed && (
              <div style={{ marginTop: '0.75rem' }}>
                <div style={{ fontSize: '0.75rem', fontWeight: 600, marginBottom: '0.25rem' }}>
                  Checked rules
                </div>
                <div style={{ display: 'flex', gap: '0.375rem', flexWrap: 'wrap' }}>
                  {parsed.checked_rules.map((r) => (
                    <span key={r} className="badge badge-info">
                      {r}
                    </span>
                  ))}
                </div>

                {parsed.violations.length > 0 && (
                  <>
                    <div style={{ fontSize: '0.75rem', fontWeight: 600, marginTop: '0.75rem', marginBottom: '0.25rem' }}>
                      Violations
                    </div>
                    <ul style={{ margin: 0, paddingLeft: '1.25rem', fontSize: '0.8125rem' }}>
                      {parsed.violations.map((viol, i) => (
                        <li key={i} style={{ color: viol.severity === 'BLOCKER' ? 'var(--color-danger)' : 'var(--color-warning)' }}>
                          <strong>{viol.rule}</strong> ({viol.severity}): {viol.details}
                        </li>
                      ))}
                    </ul>
                  </>
                )}
              </div>
            )}
          </div>
        )
      })}
    </div>
  )
}