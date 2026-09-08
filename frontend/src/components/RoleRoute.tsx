import { Navigate, Outlet } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import { ROUTES } from '../constants/routes'
import { isPlatformAdmin } from '../utils/roles'

export function AdminRoute() {
  const { user } = useAuth()
  if (!isPlatformAdmin(user?.role)) {
    return <Navigate to={ROUTES.DASHBOARD} replace />
  }
  return <Outlet />
}

export function CompanyRoute() {
  const { user } = useAuth()
  if (isPlatformAdmin(user?.role)) {
    return <Navigate to={ROUTES.TENANTS} replace />
  }
  return <Outlet />
}
