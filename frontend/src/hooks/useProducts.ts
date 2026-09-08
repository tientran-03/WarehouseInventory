import { useCallback, useEffect, useState } from 'react'
import { categoryService, productService } from '../services'
import type { Category, Product, UpsertCategoryRequest, UpsertProductRequest } from '../types'
import { getErrorMessage } from '../utils/errorHandler'

export function useProducts(tenantId: string | null) {
  const [products, setProducts] = useState<Product[]>([])
  const [categories, setCategories] = useState<Category[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchAll = useCallback(async () => {
    if (!tenantId) {
      setProducts([])
      setCategories([])
      setLoading(false)
      return
    }

    setLoading(true)
    setError(null)
    try {
      const [productsRes, categoriesRes] = await Promise.all([
        productService.getByTenant(tenantId),
        categoryService.getByTenant(tenantId),
      ])
      setProducts(productsRes.data)
      setCategories(categoriesRes.data)
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [tenantId])

  useEffect(() => {
    fetchAll()
  }, [fetchAll])

  const createProduct = async (payload: UpsertProductRequest) => {
    await productService.create(payload)
    await fetchAll()
  }

  const updateProduct = async (id: string, payload: UpsertProductRequest) => {
    await productService.update(id, payload)
    await fetchAll()
  }

  const deleteProduct = async (id: string) => {
    await productService.delete(id)
    await fetchAll()
  }

  const createCategory = async (payload: UpsertCategoryRequest) => {
    await categoryService.create(payload)
    await fetchAll()
  }

  return { products, categories, loading, error, fetchAll, createProduct, updateProduct, createCategory, deleteProduct }
}
