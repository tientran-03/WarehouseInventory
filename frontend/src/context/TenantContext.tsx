import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { STORAGE_KEYS } from '../constants/storageKeys'
import { tenantService } from '../services'
import type { Tenant } from '../types'
import { useAuth } from './AuthContext'
import { isPlatformAdmin } from '../utils/roles'

interface TenantContextType {
  tenants: Tenant[]
  activeTenantId: string | null
  activeTenant: Tenant | null
  loading: boolean
  setActiveTenantId: (id: string) => void
  refreshTenants: () => Promise<void>
}

const TenantContext = createContext<TenantContextType | undefined>(undefined)

export function TenantProvider({ children }: { children: ReactNode }) {
  const { user, isAuthenticated } = useAuth()
  const [tenants, setTenants] = useState<Tenant[]>([])
  const [activeTenantId, setActiveTenantIdState] = useState<string | null>(() =>
    localStorage.getItem(STORAGE_KEYS.ACTIVE_TENANT),
  )
  const [loading, setLoading] = useState(true)

  const refreshTenants = useCallback(async () => {
    if (!isAuthenticated || !user) {
      setTenants([])
      setActiveTenantIdState(null)
      localStorage.removeItem(STORAGE_KEYS.ACTIVE_TENANT)
      setLoading(false)
      return
    }

    if (!isPlatformAdmin(user.role)) {
      const tenantId = user.tenantId ?? null
      setTenants([])
      setActiveTenantIdState(tenantId)
      if (tenantId) localStorage.setItem(STORAGE_KEYS.ACTIVE_TENANT, tenantId)
      else localStorage.removeItem(STORAGE_KEYS.ACTIVE_TENANT)
      setLoading(false)
      return
    }

    setLoading(true)
    try {
      const { data } = await tenantService.getAll()
      setTenants(data)

      setActiveTenantIdState((current) => {
        if (current && data.some((t) => t.id === current)) return current
        const fallback = data[0]?.id ?? null
        if (fallback) localStorage.setItem(STORAGE_KEYS.ACTIVE_TENANT, fallback)
        else localStorage.removeItem(STORAGE_KEYS.ACTIVE_TENANT)
        return fallback
      })
    } finally {
      setLoading(false)
    }
  }, [isAuthenticated, user])

  useEffect(() => {
    refreshTenants()
  }, [refreshTenants])

  const setActiveTenantId = useCallback((id: string) => {
    localStorage.setItem(STORAGE_KEYS.ACTIVE_TENANT, id)
    setActiveTenantIdState(id)
  }, [])

  const activeTenant = useMemo(
    () => tenants.find((t) => t.id === activeTenantId) ?? null,
    [tenants, activeTenantId],
  )

  const value = useMemo(
    () => ({ tenants, activeTenantId, activeTenant, loading, setActiveTenantId, refreshTenants }),
    [tenants, activeTenantId, activeTenant, loading, setActiveTenantId, refreshTenants],
  )

  return <TenantContext.Provider value={value}>{children}</TenantContext.Provider>
}

export function useTenantContext() {
  const ctx = useContext(TenantContext)
  if (!ctx) throw new Error('useTenantContext must be used within TenantProvider')
  return ctx
}
