import axiosClient from '../api/axiosClient'
import type { ApiSuccessResponse, AuthResponse, AuthUser, ChangePasswordRequest, LoginRequest } from '../types'
import { STORAGE_KEYS } from '../constants/storageKeys'

export const authService = {
  async login(payload: LoginRequest): Promise<AuthUser> {
    const { data } = await axiosClient.post<ApiSuccessResponse<AuthResponse>>('/auth/login', payload)
    const auth = data.data

    return storeAuthUser(auth)
  },

  async changePassword(payload: ChangePasswordRequest): Promise<AuthUser> {
    const { data } = await axiosClient.post<ApiSuccessResponse<AuthResponse>>('/auth/change-password', payload)
    return storeAuthUser(data.data)
  },

  verifyEmail: (token: string) => axiosClient.post('/auth/verify-email', undefined, { params: { token } }),

  logout(): void {
    localStorage.removeItem(STORAGE_KEYS.TOKEN)
    localStorage.removeItem(STORAGE_KEYS.USER)
  },

  getStoredUser(): AuthUser | null {
    const raw = localStorage.getItem(STORAGE_KEYS.USER)
    if (!raw) return null
    try {
      return JSON.parse(raw) as AuthUser
    } catch {
      return null
    }
  },
}

function storeAuthUser(auth: AuthResponse): AuthUser {
  const user: AuthUser = {
    username: auth.username,
    role: auth.role,
    token: auth.token,
    expiration: auth.expiration,
    tenantId: auth.tenantId,
    requiresPasswordChange: auth.requiresPasswordChange,
  }

  localStorage.setItem(STORAGE_KEYS.TOKEN, auth.token)
  localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(user))
  return user
}
