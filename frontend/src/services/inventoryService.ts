import axiosClient from '../api/axiosClient'
import type { InventoryItem, UpsertInventoryRequest } from '../types'

export const inventoryService = {
  getByTenant: (tenantId: string) =>
    axiosClient.get<InventoryItem[]>(`/inventory?tenantId=${tenantId}`),
  getByWarehouse: (warehouseId: string) =>
    axiosClient.get<InventoryItem[]>(`/inventory/warehouse/${warehouseId}`),
  upsert: (payload: UpsertInventoryRequest) =>
    axiosClient.post<InventoryItem>('/inventory', payload),
  update: (id: string, payload: UpsertInventoryRequest) =>
    axiosClient.put<InventoryItem>(`/inventory/${id}`, payload),
  delete: (id: string) => axiosClient.delete(`/inventory/${id}`),
}
