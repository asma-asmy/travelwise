interface Props extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger'
  loading?: boolean
  children: React.ReactNode
}

export function Button({ variant = 'primary', loading, children, disabled, ...rest }: Props) {
  const cls = variant === 'primary' ? 'btn btn-primary' : variant === 'danger' ? 'btn btn-danger' : 'btn btn-secondary'
  return (
    <button className={cls} disabled={disabled || loading} {...rest}>
      {loading ? 'Working…' : children}
    </button>
  )
}