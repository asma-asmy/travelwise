import { apiClient, REFRESH_KEY, TOKEN_KEY } from '@/api/client'
import type {
  AuthResponse,
  LoginRequest,
  MeResponse,
  RefreshRequest,
  RegisterRequest,
} from '@/types/auth'

export async function login(payload: LoginRequest): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/api/v1/auth/login', payload)
  return data
}

export async function register(payload: RegisterRequest): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/api/v1/auth/register', payload)
  return data
}

export async function refresh(payload: RefreshRequest): Promise<AuthResponse> {
  const { data } = await apiClient.post<AuthResponse>('/api/v1/auth/refresh', payload)
  return data
}

export async function getMe(): Promise<MeResponse> {
  const { data } = await apiClient.get<MeResponse>('/api/v1/auth/me')
  return data
}

export async function logout(): Promise<void> {
  const refreshToken = localStorage.getItem(REFRESH_KEY)
  if (!refreshToken) return
  try {
    await apiClient.post('/api/v1/auth/logout', { refreshToken })
  } catch {
    // best-effort: even if the server rejects it, clear the local session
  } finally {
    localStorage.removeItem(TOKEN_KEY)
    localStorage.removeItem(REFRESH_KEY)
  }
}