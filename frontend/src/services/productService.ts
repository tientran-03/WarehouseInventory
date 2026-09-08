import axiosClient from '../api/axiosClient'
import type { Category, Product, UpsertCategoryRequest, UpsertProductRequest } from '../types'

export const productService = {
  getByTenant: (tenantId: string) =>
    axiosClient.get<Product[]>(`/products?tenantId=${tenantId}`),
  getById: (id: string) => axiosClient.get<Product>(`/products/${id}`),
  create: (payload: UpsertProductRequest) => axiosClient.post<Product>('/products', payload),
  update: (id: string, payload: UpsertProductRequest) => axiosClient.put(`/products/${id}`, payload),
  delete: (id: string) => axiosClient.delete(`/products/${id}`),
  uploadImage: (id: string, file: File) => {
    const formData = new FormData()
    formData.append('file', file)
    return axiosClient.post<{ imageUrl: string }>(`/products/${id}/upload-image`, formData, {
      headers: { 'Content-Type': 'multipart/form-data' },
    })
  },
}

export const categoryService = {
  getByTenant: (tenantId: string) =>
    axiosClient.get<Category[]>(`/categories?tenantId=${tenantId}`),
  create: (payload: UpsertCategoryRequest) =>
    axiosClient.post<Category>('/categories', payload),
}
