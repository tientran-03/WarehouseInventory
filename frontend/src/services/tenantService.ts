import axiosClient from '../api/axiosClient'
import type { Tenant, UpsertTenantRequest } from '../types'

export const tenantService = {
  getAll: () => axiosClient.get<Tenant[]>('/tenants'),
  getById: (id: string) => axiosClient.get<Tenant>(`/tenants/${id}`),
  create: (payload: UpsertTenantRequest) => axiosClient.post<Tenant>('/tenants', payload),
  update: (id: string, payload: UpsertTenantRequest) => axiosClient.put(`/tenants/${id}`, payload),
  delete: (id: string) => axiosClient.delete(`/tenants/${id}`),
}
