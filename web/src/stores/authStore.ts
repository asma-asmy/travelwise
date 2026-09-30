import { create } from 'zustand'
import { TOKEN_KEY, REFRESH_KEY } from '@/api/client'
import * as authApi from '@/api/auth'
import type { AuthResponse, LoginRequest, RegisterRequest, UserRole } from '@/types/auth'

interface AuthState {
  userId: string | null
  email: string | null
  fullName: string | null
  roles: UserRole[]
  isAuthenticated: boolean
  isBootstrapping: boolean

  login: (payload: LoginRequest) => Promise<void>
  register: (payload: RegisterRequest) => Promise<void>
  logout: () => Promise<void>
  bootstrap: () => Promise<void>
  hasRole: (...roles: UserRole[]) => boolean
}

export const useAuthStore = create<AuthState>((set, get) => ({
  userId: null,
  email: null,
  fullName: null,
  roles: [],
  isAuthenticated: false,
  isBootstrapping: true,

  async login(payload) {
    const response = await authApi.login(payload)
    applyAuthResponse(response, set)
  },

  async register(payload) {
    const response = await authApi.register(payload)
    applyAuthResponse(response, set)
  },

  async logout() {
    await authApi.logout()
    set({
      userId: null,
      email: null,
      fullName: null,
      roles: [],
      isAuthenticated: false,
    })
  },

  async bootstrap() {
    set({ isBootstrapping: true })
    const token = localStorage.getItem(TOKEN_KEY)
    const refreshToken = localStorage.getItem(REFRESH_KEY)

    if (!token && !refreshToken) {
      set({ isBootstrapping: false, isAuthenticated: false })
      return
    }

    try {
      const me = await authApi.getMe()
      set({
        userId: me.userId,
        email: me.email,
        fullName: me.fullName,
        roles: me.roles,
        isAuthenticated: true,
        isBootstrapping: false,
      })
    } catch {
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem(REFRESH_KEY)
      set({
        userId: null,
        email: null,
        fullName: null,
        roles: [],
        isAuthenticated: false,
        isBootstrapping: false,
      })
    }
  },

  hasRole(...roles) {
    const userRoles = get().roles
    return roles.some((role) => userRoles.includes(role))
  },
}))

function applyAuthResponse(
  response: AuthResponse,
  set: (partial: Partial<AuthState>) => void,
) {
  localStorage.setItem(TOKEN_KEY, response.accessToken)
  localStorage.setItem(REFRESH_KEY, response.refreshToken)
  set({
    userId: response.userId,
    email: response.email,
    fullName: response.fullName,
    roles: response.roles,
    isAuthenticated: true,
    isBootstrapping: false,
  })
}