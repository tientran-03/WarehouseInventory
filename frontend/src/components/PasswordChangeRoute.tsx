import { Navigate, Outlet } from 'react-router-dom'
import { ROUTES } from '../constants/routes'
import { useAuth } from '../context/AuthContext'

export function PasswordChangeRequiredRoute() {
  const { user } = useAuth()
  if (!user) return <Navigate to={ROUTES.LOGIN} replace />
  if (!user.requiresPasswordChange) return <Navigate to={ROUTES.DASHBOARD} replace />
  return <Outlet />
}

export function PasswordChangeCompletedRoute() {
  const { user } = useAuth()
  if (user?.requiresPasswordChange) return <Navigate to={ROUTES.CHANGE_PASSWORD} replace />
  return <Outlet />
}
