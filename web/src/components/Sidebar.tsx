import { NavLink } from 'react-router'
import { NAVIGATION } from '@/config/navigation'
import { useAuthStore } from '@/stores/authStore'
import { RequireRole } from '@/components/RequireRole'

interface Props {
  isOpen?: boolean
  onNavigate?: () => void
}

export function Sidebar({ isOpen = false, onNavigate }: Props) {
  const fullName = useAuthStore((s) => s.fullName)
  const roles = useAuthStore((s) => s.roles)

  return (
    <aside
      className={`app-sidebar${isOpen ? ' app-sidebar--open' : ''}`}
      aria-label="Primary navigation"
    >
      <div style={{ padding: '1rem' }}>
        <div style={{ fontSize: '1.125rem', fontWeight: 700 }}>TravelWise</div>
        <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>{fullName}</div>
        <div style={{ fontSize: '0.75rem', color: 'var(--color-text-muted)' }}>{roles.join(', ')}</div>
      </div>

      <nav style={{ padding: '0 0.5rem 1rem 0.5rem' }}>
        {NAVIGATION.map((section) => (
          <div key={section.title} style={{ marginTop: '1rem' }}>
            <div
              style={{
                fontSize: '0.6875rem',
                fontWeight: 600,
                letterSpacing: '0.05em',
                textTransform: 'uppercase',
                color: 'var(--color-text-muted)',
                padding: '0.25rem 0.75rem',
              }}
            >
              {section.title}
            </div>
            <ul style={{ listStyle: 'none', padding: 0, margin: 0 }}>
              {section.items.map((item) => (
                <li key={item.to}>
                  <RequireRole requireAnyRole={item.requireAnyRole}>
                    <NavLink
                      to={item.to}
                      onClick={onNavigate}
                      style={({ isActive }) => ({
                        display: 'flex',
                        alignItems: 'center',
                        gap: '0.625rem',
                        padding: '0.5rem 0.75rem',
                        borderRadius: 'var(--radius-sm)',
                        color: isActive ? 'white' : 'var(--color-text)',
                        background: isActive ? 'var(--color-primary)' : 'transparent',
                        fontWeight: isActive ? 600 : 400,
                        fontSize: '0.875rem',
                        textDecoration: 'none',
                        marginTop: '0.125rem',
                      })}
                    >
                      <item.icon size={16} strokeWidth={1.75} />
                      {item.label}
                    </NavLink>
                  </RequireRole>
                </li>
              ))}
            </ul>
          </div>
        ))}
      </nav>

      <div style={{ flex: 1 }} />

      <div
        style={{
          padding: '0.75rem 1rem',
          fontSize: '0.75rem',
          color: 'var(--color-text-muted)',
          borderTop: '1px solid var(--color-border)',
        }}
      >
        v0.1.0 · SE3090
      </div>
    </aside>
  )
}