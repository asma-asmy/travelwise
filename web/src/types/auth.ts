export type UserRole = 'Traveller' | 'TravelOpsStaff' | 'Admin'

export interface AuthResponse {
  userId: string
  email: string
  fullName: string
  accessToken: string
  accessTokenExpiresAtUtc: string
  refreshToken: string
  refreshTokenExpiresAtUtc: string
  roles: UserRole[]
}

export interface MeResponse {
  userId: string
  email: string
  fullName: string
  phoneNumber: string | null
  roles: UserRole[]
}

export interface LoginRequest {
  email: string
  password: string
}

export interface RegisterRequest {
  email: string
  password: string
  fullName: string
  phoneNumber?: string | null
}

export interface RefreshRequest {
  refreshToken: string
}