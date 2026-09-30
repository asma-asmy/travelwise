import { useEffect } from 'react'
import { useAuthStore } from '@/stores/authStore'

interface Props {
  children: React.ReactNode
}

export function AuthBootstrap({ children }: Props) {
  const bootstrap = useAuthStore((s) => s.bootstrap)
  const isBootstrapping = useAuthStore((s) => s.isBootstrapping)

  useEffect(() => {
    void bootstrap()
  }, [bootstrap])

  if (isBootstrapping) {
    return (
      <div className="container" style={{ paddingTop: '4rem', textAlign: 'center' }}>
        <p>Loading…</p>
      </div>
    )
  }

  return <>{children}</>
}