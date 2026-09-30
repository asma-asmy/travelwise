import { useNavigate } from 'react-router'
import { Menu, LogOut } from 'lucide-react'
import { useAuthStore } from '@/stores/authStore'

interface Props {
  onMenuClick?: () => void
}

export function Topbar({ onMenuClick }: Props) {
  const navigate = useNavigate()
  const fullName = useAuthStore((s) => s.fullName)
  const email = useAuthStore((s) => s.email)
  const logout = useAuthStore((s) => s.logout)

  async function handleLogout() {
    await logout()
    navigate('/login', { replace: true })
  }

  return (
    <header
      style={{
        background: 'var(--color-surface)',
        borderBottom: '1px solid var(--color-border)',
        padding: '0.5rem 1rem',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        gap: '1rem',
      }}
    >
      <button
        className="btn btn-secondary mobile-menu-button"
        onClick={onMenuClick}
        aria-label="Open navigation"
        style={{ display: 'none' }}
      >
        <Menu size={18} />
      </button>

      <div style={{ flex: 1 }} />

      <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
        <div style={{ textAlign: 'right', lineHeight: 1.2 }}>
          <div style={{ fontSize: '0.875rem', fontWeight: 600 }}>{fullName}</div>
          <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>{email}</div>
        </div>
        <button className="btn btn-secondary" onClick={handleLogout} aria-label="Logout">
          <LogOut size={16} />
          <span className="logout-label">Logout</span>
        </button>
      </div>
    </header>
  )
}