import axiosClient from '../api/axiosClient'
import type { CreateInvoiceRequest, Invoice } from '../types'

export const invoiceService = {
  getByTenant: (tenantId: string) =>
    axiosClient.get<Invoice[]>(`/invoices?tenantId=${tenantId}`),
  getById: (id: string) => axiosClient.get<Invoice>(`/invoices/${id}`),
  getByStockDocumentId: (stockDocumentId: string) =>
    axiosClient.get<Invoice>(`/invoices/by-stock-document/${stockDocumentId}`),
  create: (payload: CreateInvoiceRequest) => axiosClient.post<Invoice>('/invoices', payload),
}
