import { useCallback, useEffect, useState } from 'react'
import { orderService } from '../services'
import type { CreateOrderRequest, OrderDetail, OrderListItem } from '../types'
import { getErrorMessage } from '../utils/errorHandler'

export function useOrders(tenantId: string | null) {
  const [orders, setOrders] = useState<OrderListItem[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  const fetchOrders = useCallback(async () => {
    if (!tenantId) {
      setOrders([])
      setLoading(false)
      return
    }

    setLoading(true)
    setError(null)
    try {
      const { data } = await orderService.getByTenant(tenantId)
      setOrders(data)
    } catch (err) {
      setError(getErrorMessage(err))
    } finally {
      setLoading(false)
    }
  }, [tenantId])

  useEffect(() => {
    fetchOrders()
  }, [fetchOrders])

  const createOrder = async (payload: CreateOrderRequest) => {
    const { data } = await orderService.create(payload)
    await fetchOrders()
    return data.data
  }

  const getOrderDetail = async (id: string) => {
    const { data } = await orderService.getById(id)
    return data as OrderDetail
  }

  const prependOrderHint = useCallback((orderCode: string) => {
    setOrders((prev) => {
      if (prev.some((o) => o.orderCode === orderCode)) return prev
      return prev
    })
    fetchOrders()
  }, [fetchOrders])

  return { orders, loading, error, fetchOrders, createOrder, getOrderDetail, prependOrderHint }
}
