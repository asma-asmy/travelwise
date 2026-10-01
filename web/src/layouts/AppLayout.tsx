import { useEffect, useState } from 'react'
import { Outlet, useLocation } from 'react-router'
import { Sidebar } from '@/components/Sidebar'
import { Topbar } from '@/components/Topbar'

export function AppLayout() {
  const [mobileOpen, setMobileOpen] = useState(false)
  const location = useLocation()

  // Close the mobile drawer when route changes
  useEffect(() => {
    setMobileOpen(false)
  }, [location.pathname])

  // Close the mobile drawer on Escape key
  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') setMobileOpen(false)
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [])

  // Lock body scroll while drawer is open
  useEffect(() => {
    document.body.style.overflow = mobileOpen ? 'hidden' : ''
    return () => {
      document.body.style.overflow = ''
    }
  }, [mobileOpen])

  return (
    <div className="app-shell">
      <Sidebar isOpen={mobileOpen} onNavigate={() => setMobileOpen(false)} />

      {mobileOpen && (
        <button
          className="app-overlay"
          aria-label="Close navigation"
          onClick={() => setMobileOpen(false)}
        />
      )}

      <div className="app-main">
        <Topbar onMenuClick={() => setMobileOpen((v) => !v)} />
        <main id="main-content" className="app-content">
          <div className="container">
            <Outlet />
          </div>
        </main>
      </div>
    </div>
  )
}