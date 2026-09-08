import { useCallback, useEffect, useState } from 'react'
import { inventoryService } from '../services'
import type { InventoryItem, UpsertInventoryRequest } from '../types'
import { getErrorMessage } from '../utils/errorHandler'

export function useInventory(tenantId: string | null) {
  const [items, setItems] = useState<InventoryItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchInventory = useCallback(async () => {
    if (!tenantId) {
      setItems([])
      setLoading(false)
      return
    }

    setLoading(true)
    setError(null)
    try {
      const { data } = await inventoryService.getByTenant(tenantId)
      setItems(data)
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [tenantId])

  useEffect(() => {
    fetchInventory()
  }, [fetchInventory])

  const upsertInventory = async (payload: UpsertInventoryRequest) => {
    await inventoryService.upsert(payload)
    await fetchInventory()
  }

  const updateInventory = async (id: string, payload: UpsertInventoryRequest) => {
    await inventoryService.update(id, payload)
    await fetchInventory()
  }

  const deleteInventory = async (id: string) => {
    await inventoryService.delete(id)
    await fetchInventory()
  }

  return { items, loading, error, fetchInventory, upsertInventory, updateInventory, deleteInventory }
}
