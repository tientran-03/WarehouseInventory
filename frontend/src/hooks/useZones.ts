import { useCallback, useEffect, useState } from 'react'
import { zoneService } from '../services'
import type { CreateZoneRequest, WarehouseZone } from '../types'
import { getErrorMessage } from '../utils/errorHandler'

export function useZones(tenantId: string | null) {
  const [zones, setZones] = useState<WarehouseZone[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchAll = useCallback(async () => {
    if (!tenantId) {
      setZones([])
      setLoading(false)
      return
    }
    setLoading(true)
    setError(null)
    try {
      const { data } = await zoneService.getByTenant(tenantId)
      setZones(data)
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [tenantId])

  useEffect(() => { fetchAll() }, [fetchAll])

  const createZone = async (payload: CreateZoneRequest) => {
    await zoneService.create(payload)
    await fetchAll()
  }

  const deleteZone = async (id: string) => {
    await zoneService.delete(id)
    await fetchAll()
  }

  return { zones, loading, error, fetchAll, createZone, deleteZone }
}
