import axiosClient from '../api/axiosClient'
import type {
  CreateOrderRequest,
  OrderAllocationResult,
  OrderDetail,
  OrderListItem,
  UpdateOrderStatusRequest,
} from '../types'
import type { ApiSuccessResponse } from '../types/api.types'

export const orderService = {
  getByTenant: (tenantId: string, status?: string) => {
    const params = new URLSearchParams({ tenantId })
    if (status) params.set('status', status)
    return axiosClient.get<OrderListItem[]>(`/orders?${params}`)
  },
  getById: (id: string) => axiosClient.get<OrderDetail>(`/orders/${id}`),
  create: (payload: CreateOrderRequest) =>
    axiosClient.post<ApiSuccessResponse<OrderAllocationResult>>('/orders', payload),
  updateStatus: (id: string, payload: UpdateOrderStatusRequest) =>
    axiosClient.patch<OrderDetail>(`/orders/${id}/status`, payload),
  createStockDocuments: (id: string) =>
    axiosClient.post<OrderDetail>(`/orders/${id}/create-stock-documents`),
}
