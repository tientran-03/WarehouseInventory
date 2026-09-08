import { useCallback, useEffect, useState } from 'react'
import { stockDocumentService } from '../services'
import type { CreateStockDocumentRequest, StockDocument, StockMovementReport, StockMovementReportFilter } from '../types'
import { getErrorMessage } from '../utils/errorHandler'

export function useStockDocuments(tenantId: string | null) {
  const [documents, setDocuments] = useState<StockDocument[]>([])
  const [report, setReport] = useState<StockMovementReport | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchDocuments = useCallback(async () => {
    if (!tenantId) {
      setDocuments([])
      setLoading(false)
      return
    }

    setLoading(true)
    setError(null)
    try {
      const { data } = await stockDocumentService.getByTenant({ tenantId })
      setDocuments(data)
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [tenantId])

  useEffect(() => { fetchDocuments() }, [fetchDocuments])

  const createDocument = useCallback(async (payload: CreateStockDocumentRequest) => {
    await stockDocumentService.create(payload)
    await fetchDocuments()
  }, [fetchDocuments])

  const fetchReport = useCallback(async (filter: Omit<StockMovementReportFilter, 'tenantId'> = {}) => {
    if (!tenantId) return
    const { data } = await stockDocumentService.getReport({ tenantId, ...filter })
    setReport(data)
  }, [tenantId])

  const exportReport = useCallback(async (filter: Omit<StockMovementReportFilter, 'tenantId'> = {}) => {
    if (!tenantId) return
    const response = await stockDocumentService.exportReport({ tenantId, ...filter })
    const url = URL.createObjectURL(response.data)
    const link = document.createElement('a')
    link.href = url
    link.download = 'bao-cao-nhap-xuat.csv'
    link.click()
    URL.revokeObjectURL(url)
  }, [tenantId])

  return { documents, report, loading, error, fetchDocuments, createDocument, fetchReport, exportReport }
}
