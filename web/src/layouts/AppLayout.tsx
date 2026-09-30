import { useState } from 'react'
import { Outlet } from 'react-router'
import { Sidebar } from '@/components/Sidebar'
import { Topbar } from '@/components/Topbar'

export function AppLayout() {
  // Controls the mobile drawer; on desktop the sidebar is always visible.
  const [mobileOpen, setMobileOpen] = useState(false)

  return (
    <div style={{ minHeight: '100vh', display: 'flex' }}>
      <Sidebar
        isOpen={mobileOpen || isDesktop()}
        onNavigate={() => setMobileOpen(false)}
      />
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', minWidth: 0 }}>
        <Topbar onMenuClick={() => setMobileOpen((v) => !v)} />
        <main style={{ flex: 1, padding: '1.5rem 0', overflow: 'auto' }}>
          <div className="container">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  )
}

// Tiny helper — on wide screens the sidebar is always shown.
function isDesktop(): boolean {
  if (typeof window === 'undefined') return true
  return window.innerWidth > 768
}