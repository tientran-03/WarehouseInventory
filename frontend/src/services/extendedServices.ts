import axiosClient from '../api/axiosClient'
import type {
  BalancingSuggestion,
  CompleteStocktakeRequest,
  CreateShippingRouteRequest,
  CreateStocktakeRequest,
  CreateTransferRequest,
  CreateZoneRequest,
  FinancialSummary,
  ShippingRoute,
  Stocktake,
  Transfer,
  WarehouseZone,
  CreateStockDocumentRequest,
  StockDocument,
  StockMovementReport,
  StockMovementReportFilter,
} from '../types'

export const transferService = {
  getByTenant: (tenantId: string) => axiosClient.get<Transfer[]>(`/transfers?tenantId=${tenantId}`),
  create: (payload: CreateTransferRequest) => axiosClient.post<Transfer>('/transfers', payload),
  start: (id: string) => axiosClient.post<Transfer>(`/transfers/${id}/start`),
  complete: (id: string) => axiosClient.post<Transfer>(`/transfers/${id}/complete`),
}

export const stocktakeService = {
  getByTenant: (tenantId: string) => axiosClient.get<Stocktake[]>(`/stocktakes?tenantId=${tenantId}`),
  create: (payload: CreateStocktakeRequest) => axiosClient.post<Stocktake>('/stocktakes', payload),
  complete: (id: string, payload: CompleteStocktakeRequest) =>
    axiosClient.post<Stocktake>(`/stocktakes/${id}/complete`, payload),
}

export const zoneService = {
  getByTenant: (tenantId: string) => axiosClient.get<WarehouseZone[]>(`/warehousezones?tenantId=${tenantId}`),
  create: (payload: CreateZoneRequest) => axiosClient.post<WarehouseZone>('/warehousezones', payload),
  delete: (id: string) => axiosClient.delete(`/warehousezones/${id}`),
}

export const shippingRouteService = {
  getByTenant: (tenantId: string) => axiosClient.get<ShippingRoute[]>(`/shippingroutes?tenantId=${tenantId}`),
  create: (payload: CreateShippingRouteRequest) => axiosClient.post<ShippingRoute>('/shippingroutes', payload),
  delete: (id: string) => axiosClient.delete(`/shippingroutes/${id}`),
}

export const analyticsService = {
  getFinancials: (tenantId: string) => axiosClient.get<FinancialSummary>(`/analytics/financials?tenantId=${tenantId}`),
  getBalancing: (tenantId: string) => axiosClient.get<BalancingSuggestion[]>(`/analytics/balancing?tenantId=${tenantId}`),
}

export const stockDocumentService = {
  getByTenant: (filter: StockMovementReportFilter) =>
    axiosClient.get<StockDocument[]>('/stock-documents', { params: filter }),
  create: (payload: CreateStockDocumentRequest) =>
    axiosClient.post<StockDocument>('/stock-documents', payload),
  getReport: (filter: StockMovementReportFilter) =>
    axiosClient.get<StockMovementReport>('/stock-documents/reports/movements', { params: filter }),
  exportReportExcel: (filter: StockMovementReportFilter) =>
    axiosClient.get<Blob>('/stock-documents/reports/movements/export-excel', {
      params: filter,
      responseType: 'blob',
    }),
}
