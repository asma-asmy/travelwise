import axios from 'axios'
import type { AxiosError, InternalAxiosRequestConfig } from 'axios'

const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5114'

export const TOKEN_KEY = 'travelwise.accessToken'
export const REFRESH_KEY = 'travelwise.refreshToken'

export const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
  timeout: 30000,
})

// Request interceptor — attach the access token
apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const token = localStorage.getItem(TOKEN_KEY)
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// Response interceptor — on 401, try refresh once
let isRefreshing = false
let refreshPromise: Promise<string> | null = null

async function refreshAccessToken(): Promise<string> {
  const refreshToken = localStorage.getItem(REFRESH_KEY)
  if (!refreshToken) throw new Error('No refresh token available')

  const response = await axios.post(
    `${API_BASE_URL}/api/v1/auth/refresh`,
    { refreshToken },
    { headers: { 'Content-Type': 'application/json' } },
  )

  const { accessToken, refreshToken: newRefresh } = response.data
  localStorage.setItem(TOKEN_KEY, accessToken)
  localStorage.setItem(REFRESH_KEY, newRefresh)
  return accessToken
}

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError) => {
    const original = error.config as InternalAxiosRequestConfig & { _retried?: boolean }

    if (error.response?.status !== 401 || original._retried) {
      return Promise.reject(error)
    }

    if (original.url?.includes('/auth/refresh')) {
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem(REFRESH_KEY)
      return Promise.reject(error)
    }

    original._retried = true

    try {
      if (!isRefreshing) {
        isRefreshing = true
        refreshPromise = refreshAccessToken().finally(() => {
          isRefreshing = false
          refreshPromise = null
        })
      }
      const newToken = await refreshPromise!
      original.headers.Authorization = `Bearer ${newToken}`
      return apiClient(original)
    } catch (refreshError) {
      localStorage.removeItem(TOKEN_KEY)
      localStorage.removeItem(REFRESH_KEY)
      if (window.location.pathname !== '/login') {
        window.location.href = '/login'
      }
      return Promise.reject(refreshError)
    }
  },
)