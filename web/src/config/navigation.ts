import {
  LayoutDashboard,
  Map,
  Wallet,
  CalendarDays,
  ShieldAlert,
  FileCheck2,
  GitBranch,
  Users,
  Bell,
  ClipboardList,
  Settings,
} from 'lucide-react'
import type { UserRole } from '@/types/auth'

export interface NavItem {
  label: string
  to: string
  icon: React.ComponentType<{ size?: number; strokeWidth?: number }>
  requireAnyRole?: UserRole[]
  children?: NavItem[]
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
    title: 'Trips',
    items: [
      {
        label: 'My Trips',
        to: '/trips',
        icon: Map,
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
    title: 'Reference',
    items: [
      {
        label: 'Activities',
        to: '/activities',
        icon: CalendarDays,
        requireAnyRole: ['Admin'],
      },
      {
        label: 'Budget',
        to: '/budget',
        icon: Wallet,
      },
      {
        label: 'Risk',
        to: '/risk',
        icon: ShieldAlert,
      },
      {
        label: 'Readiness',
        to: '/readiness',
        icon: FileCheck2,
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
        label: 'Requirements',
        to: '/admin/requirements',
        icon: ClipboardList,
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