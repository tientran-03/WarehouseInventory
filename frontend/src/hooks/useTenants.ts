import { useCallback, useEffect, useState } from 'react'
import { tenantService } from '../services'
import type { Tenant, UpsertTenantRequest } from '../types'
import { getErrorMessage } from '../utils/errorHandler'

export function useTenants(enabled = true) {
  const [tenants, setTenants] = useState<Tenant[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchTenants = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const { data } = await tenantService.getAll()
      setTenants(data)
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    if (!enabled) {
      setTenants([])
      setError(null)
      setLoading(false)
      return
    }

    void fetchTenants()
  }, [enabled, fetchTenants])

  const createTenant = async (payload: UpsertTenantRequest) => {
    await tenantService.create(payload)
    await fetchTenants()
  }

  const updateTenant = async (id: string, payload: UpsertTenantRequest) => {
    await tenantService.update(id, payload)
    await fetchTenants()
  }

  const deleteTenant = async (id: string) => {
    await tenantService.delete(id)
    await fetchTenants()
  }

  return { tenants, loading, error, fetchTenants, createTenant, updateTenant, deleteTenant }
}
