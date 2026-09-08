import { createContext, useContext, useMemo, useState, type ReactNode } from 'react'
import { authService } from '../services'
import type { AuthUser, ChangePasswordRequest, LoginRequest } from '../types'
import { parseApiError } from '../utils/errorHandler'

interface AuthContextType {
  user: AuthUser | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (payload: LoginRequest) => Promise<AuthUser>
  changePassword: (payload: ChangePasswordRequest) => Promise<AuthUser>
  logout: () => void
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => authService.getStoredUser())
  const [isLoading, setIsLoading] = useState(false)

  const login = async (payload: LoginRequest) => {
    setIsLoading(true)
    try {
      const authUser = await authService.login(payload)
      setUser(authUser)
      return authUser
    } catch (error) {
      const { message } = parseApiError(error, 'Đăng nhập thất bại')
      throw new Error(message)
    } finally {
      setIsLoading(false)
    }
  }

  const changePassword = async (payload: ChangePasswordRequest) => {
    setIsLoading(true)
    try {
      const authUser = await authService.changePassword(payload)
      setUser(authUser)
      return authUser
    } catch (error) {
      const { message } = parseApiError(error, 'Không thể đổi mật khẩu')
      throw new Error(message)
    } finally {
      setIsLoading(false)
    }
  }

  const logout = () => {
    authService.logout()
    setUser(null)
  }

  const value = useMemo(
    () => ({
      user,
      isAuthenticated: !!user,
      isLoading,
      login,
      changePassword,
      logout,
    }),
    [user, isLoading],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth() {
  const context = useContext(AuthContext)
  if (!context) throw new Error('useAuth must be used within an AuthProvider')
  return context
}
