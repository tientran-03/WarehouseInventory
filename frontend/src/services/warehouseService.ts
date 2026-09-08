import axiosClient from '../api/axiosClient'
import type { UpsertWarehouseRequest, Warehouse } from '../types'

export const warehouseService = {
  getAll: () => axiosClient.get<Warehouse[]>('/warehouses'),
  getById: (id: string) => axiosClient.get<Warehouse>(`/warehouses/${id}`),
  create: (payload: UpsertWarehouseRequest) => axiosClient.post<Warehouse>('/warehouses', payload),
  update: (id: string, payload: UpsertWarehouseRequest) => axiosClient.put(`/warehouses/${id}`, payload),
  delete: (id: string) => axiosClient.delete(`/warehouses/${id}`),
}
