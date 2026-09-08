import { useCallback, useEffect, useState } from 'react'
import { analyticsService } from '../services'
import type { BalancingSuggestion, FinancialSummary } from '../types'
import { getErrorMessage } from '../utils/errorHandler'

export function useAnalytics(tenantId: string | null) {
  const [financials, setFinancials] = useState<FinancialSummary | null>(null)
  const [balancing, setBalancing] = useState<BalancingSuggestion[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchAll = useCallback(async () => {
    if (!tenantId) {
      setFinancials(null)
      setBalancing([])
      setLoading(false)
      return
    }
    setLoading(true)
    setError(null)
    try {
      const [finRes, balRes] = await Promise.all([
        analyticsService.getFinancials(tenantId),
        analyticsService.getBalancing(tenantId),
      ])
      setFinancials(finRes.data)
      setBalancing(balRes.data)
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [tenantId])

  useEffect(() => { fetchAll() }, [fetchAll])

  return { financials, balancing, loading, error, fetchAll }
}
