interface FieldProps {
  label: string
  id: string
  type?: string
  value: string | number
  onChange: (v: string) => void
  required?: boolean
  hint?: string
  error?: string
  placeholder?: string
  disabled?: boolean
}

export function FormField({
  label,
  id,
  type = 'text',
  value,
  onChange,
  required,
  hint,
  error,
  placeholder,
  disabled,
}: FieldProps) {
  return (
    <div style={{ marginBottom: '1rem' }}>
      <label htmlFor={id} style={{ display: 'block', marginBottom: '0.25rem', fontSize: '0.875rem' }}>
        {label} {required && <span style={{ color: 'var(--color-danger)' }}>*</span>}
      </label>
      <input
        id={id}
        type={type}
        value={value}
        required={required}
        disabled={disabled}
        placeholder={placeholder}
        onChange={(e) => onChange(e.target.value)}
        style={{
          width: '100%',
          padding: '0.5rem',
          border: `1px solid ${error ? 'var(--color-danger)' : 'var(--color-border)'}`,
          borderRadius: 'var(--radius-sm)',
          fontSize: '0.875rem',
        }}
      />
      {hint && !error && (
        <small style={{ color: 'var(--color-text-muted)', fontSize: '0.75rem' }}>{hint}</small>
      )}
      {error && <small style={{ color: 'var(--color-danger)', fontSize: '0.75rem' }}>{error}</small>}
    </div>
  )
}

interface SelectProps {
  label: string
  id: string
  value: string | number
  onChange: (v: string) => void
  options: { value: string | number; label: string }[]
  required?: boolean
  hint?: string
  error?: string
  disabled?: boolean
}

export function FormSelect({
  label,
  id,
  value,
  onChange,
  options,
  required,
  hint,
  error,
  disabled,
}: SelectProps) {
  return (
    <div style={{ marginBottom: '1rem' }}>
      <label htmlFor={id} style={{ display: 'block', marginBottom: '0.25rem', fontSize: '0.875rem' }}>
        {label} {required && <span style={{ color: 'var(--color-danger)' }}>*</span>}
      </label>
      <select
        id={id}
        value={value}
        required={required}
        disabled={disabled}
        onChange={(e) => onChange(e.target.value)}
        style={{
          width: '100%',
          padding: '0.5rem',
          border: `1px solid ${error ? 'var(--color-danger)' : 'var(--color-border)'}`,
          borderRadius: 'var(--radius-sm)',
          fontSize: '0.875rem',
          background: 'white',
        }}
      >
        {options.map((o) => (
          <option key={o.value} value={o.value}>
            {o.label}
          </option>
        ))}
      </select>
      {hint && !error && (
        <small style={{ color: 'var(--color-text-muted)', fontSize: '0.75rem' }}>{hint}</small>
      )}
      {error && <small style={{ color: 'var(--color-danger)', fontSize: '0.75rem' }}>{error}</small>}
    </div>
  )
}

interface TextareaProps {
  label: string
  id: string
  value: string
  onChange: (v: string) => void
  rows?: number
  required?: boolean
  hint?: string
  error?: string
  maxLength?: number
}

export function FormTextarea({
  label,
  id,
  value,
  onChange,
  rows = 3,
  required,
  hint,
  error,
  maxLength,
}: TextareaProps) {
  return (
    <div style={{ marginBottom: '1rem' }}>
      <label htmlFor={id} style={{ display: 'block', marginBottom: '0.25rem', fontSize: '0.875rem' }}>
        {label} {required && <span style={{ color: 'var(--color-danger)' }}>*</span>}
      </label>
      <textarea
        id={id}
        value={value}
        rows={rows}
        required={required}
        maxLength={maxLength}
        onChange={(e) => onChange(e.target.value)}
        style={{
          width: '100%',
          padding: '0.5rem',
          border: `1px solid ${error ? 'var(--color-danger)' : 'var(--color-border)'}`,
          borderRadius: 'var(--radius-sm)',
          fontSize: '0.875rem',
          fontFamily: 'inherit',
          resize: 'vertical',
        }}
      />
      {hint && !error && (
        <small style={{ color: 'var(--color-text-muted)', fontSize: '0.75rem' }}>{hint}</small>
      )}
      {error && <small style={{ color: 'var(--color-danger)', fontSize: '0.75rem' }}>{error}</small>}
    </div>
  )
}