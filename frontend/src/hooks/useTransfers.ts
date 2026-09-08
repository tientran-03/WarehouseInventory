import { useCallback, useEffect, useState } from 'react'
import { transferService } from '../services'
import type { CreateTransferRequest, Transfer } from '../types'
import { getErrorMessage } from '../utils/errorHandler'

export function useTransfers(tenantId: string | null) {
  const [transfers, setTransfers] = useState<Transfer[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchAll = useCallback(async () => {
    if (!tenantId) {
      setTransfers([])
      setLoading(false)
      return
    }
    setLoading(true)
    setError(null)
    try {
      const { data } = await transferService.getByTenant(tenantId)
      setTransfers(data)
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [tenantId])

  useEffect(() => { fetchAll() }, [fetchAll])

  const createTransfer = async (payload: CreateTransferRequest) => {
    await transferService.create(payload)
    await fetchAll()
  }

  const startTransfer = async (id: string) => {
    await transferService.start(id)
    await fetchAll()
  }

  const completeTransfer = async (id: string) => {
    await transferService.complete(id)
    await fetchAll()
  }

  return { transfers, loading, error, fetchAll, createTransfer, startTransfer, completeTransfer }
}
