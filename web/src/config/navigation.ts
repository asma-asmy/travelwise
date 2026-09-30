import {
  LayoutDashboard,
  Map,
  CalendarDays,
  GitBranch,
  Users,
  Bell,
  ClipboardList,
  Settings,
  FileCheck2,
  Heart,
} from 'lucide-react'
import type { UserRole } from '@/types/auth'

export interface NavItem {
  label: string
  to: string
  icon: React.ComponentType<{ size?: number; strokeWidth?: number }>
  requireAnyRole?: UserRole[]
}

export interface NavSection {
  title: string
  items: NavItem[]
}

export const NAVIGATION: NavSection[] = [
  {
    title: 'Overview',
    items: [
      {
        label: 'Dashboard',
        to: '/dashboard',
        icon: LayoutDashboard,
      },
    ],
  },
  {
    title: 'My Work',
    items: [
      {
        label: 'My Trips',
        to: '/trips',
        icon: Map,
      },
      {
        label: 'My Documents',
        to: '/documents',
        icon: FileCheck2,
      },
      {
        label: 'Preferences',
        to: '/preferences',
        icon: Heart,
      },
    ],
  },
  {
    title: 'Operations',
    items: [
      {
        label: 'Workflows',
        to: '/workflows',
        icon: GitBranch,
        requireAnyRole: ['TravelOpsStaff', 'Admin'],
      },
      {
        label: 'Safety Alerts',
        to: '/alerts',
        icon: Bell,
        requireAnyRole: ['TravelOpsStaff', 'Admin'],
      },
    ],
  },
  {
    title: 'Catalogue',
    items: [
      {
        label: 'Activities',
        to: '/activities',
        icon: CalendarDays,
        requireAnyRole: ['Admin'],
      },
      {
        label: 'Requirements',
        to: '/admin/requirements',
        icon: ClipboardList,
        requireAnyRole: ['Admin'],
      },
    ],
  },
  {
    title: 'Admin',
    items: [
      {
        label: 'Users',
        to: '/admin/users',
        icon: Users,
        requireAnyRole: ['Admin'],
      },
      {
        label: 'Settings',
        to: '/admin/settings',
        icon: Settings,
        requireAnyRole: ['Admin'],
      },
    ],
  },
]