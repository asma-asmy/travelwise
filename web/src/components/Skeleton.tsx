interface Props {
  width?: string | number
  height?: string | number
  radius?: string
  style?: React.CSSProperties
}

export function Skeleton({ width = '100%', height = '1rem', radius = '4px', style }: Props) {
  return (
    <span
      className="skeleton"
      style={{
        display: 'inline-block',
        width,
        height,
        borderRadius: radius,
        ...style,
      }}
      aria-hidden="true"
    />
  )
}