import { useMemo } from 'react'
import { useTenantContext } from '../context/TenantContext'
import { useWarehouses } from './useWarehouses'

export function useTenantWarehouses() {
  const { activeTenantId } = useTenantContext()
  const { warehouses, loading } = useWarehouses()

  const tenantWarehouses = useMemo(
    () => warehouses.filter((w) => w.tenantId === activeTenantId && w.isActive),
    [warehouses, activeTenantId],
  )

  return { tenantWarehouses, loading, activeTenantId }
}
