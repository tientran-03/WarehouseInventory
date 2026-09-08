export interface LoginRequest {
  userName: string
  password: string
}

export interface AuthResponse {
  token: string
  username: string
  expiration: string
  role: string
  tenantId?: string | null
  requiresPasswordChange: boolean
}

export interface AuthUser {
  username: string
  role: string
  token: string
  expiration: string
  tenantId?: string | null
  requiresPasswordChange: boolean
}

export interface ChangePasswordRequest {
  currentPassword: string
  newPassword: string
}
