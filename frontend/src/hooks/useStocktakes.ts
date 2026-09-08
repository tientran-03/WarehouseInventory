import { useCallback, useEffect, useState } from 'react'
import { stocktakeService } from '../services'
import type { CompleteStocktakeRequest, CreateStocktakeRequest, Stocktake } from '../types'
import { getErrorMessage } from '../utils/errorHandler'

export function useStocktakes(tenantId: string | null) {
  const [stocktakes, setStocktakes] = useState<Stocktake[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchAll = useCallback(async () => {
    if (!tenantId) {
      setStocktakes([])
      setLoading(false)
      return
    }
    setLoading(true)
    setError(null)
    try {
      const { data } = await stocktakeService.getByTenant(tenantId)
      setStocktakes(data)
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [tenantId])

  useEffect(() => { fetchAll() }, [fetchAll])

  const createStocktake = async (payload: CreateStocktakeRequest) => {
    await stocktakeService.create(payload)
    await fetchAll()
  }

  const completeStocktake = async (id: string, payload: CompleteStocktakeRequest) => {
    await stocktakeService.complete(id, payload)
    await fetchAll()
  }

  return { stocktakes, loading, error, fetchAll, createStocktake, completeStocktake }
}
