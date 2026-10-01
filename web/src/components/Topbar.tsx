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
    <header className="app-topbar">
      <button
        className="btn btn-secondary app-menu-button"
        onClick={onMenuClick}
        aria-label="Toggle navigation"
      >
        <Menu size={18} />
      </button>

      <div style={{ flex: 1 }} />

      <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
        <div className="app-user">
          <div style={{ fontSize: '0.875rem', fontWeight: 600 }}>{fullName}</div>
          <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>{email}</div>
        </div>
        <button className="btn btn-secondary" onClick={handleLogout} aria-label="Logout">
          <LogOut size={16} />
          <span className="app-logout-label">Logout</span>
        </button>
      </div>
    </header>
  )
}