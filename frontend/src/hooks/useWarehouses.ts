import { useCallback, useEffect, useState } from 'react'
import { warehouseService } from '../services'
import type { UpsertWarehouseRequest, Warehouse } from '../types'
import { getErrorMessage } from '../utils/errorHandler'

export function useWarehouses() {
  const [warehouses, setWarehouses] = useState<Warehouse[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchWarehouses = useCallback(async () => {
    setLoading(true)
    setError(null)
    try {
      const { data } = await warehouseService.getAll()
      setWarehouses(data)
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [])

  useEffect(() => {
    fetchWarehouses()
  }, [fetchWarehouses])

  const createWarehouse = async (payload: UpsertWarehouseRequest) => {
    const res = await warehouseService.create(payload)
    await fetchWarehouses()
    return res.data
  }

  const updateWarehouse = async (id: string, payload: UpsertWarehouseRequest) => {
    await warehouseService.update(id, payload)
    await fetchWarehouses()
  }

  const deleteWarehouse = async (id: string) => {
    await warehouseService.delete(id)
    await fetchWarehouses()
  }

  return { warehouses, loading, error, fetchWarehouses, createWarehouse, updateWarehouse, deleteWarehouse }
}
