export interface OrderListItem {
  id: string
  orderCode: string
  channel: string
  customerName: string
  status: string
  totalAmount: number
  subOrderCount: number
  isSplitOrder: boolean
  createdAt: string
}

export interface OrderItemDetail {
  productId: string
  productSku: string
  productName: string
  quantity: number
  unitPrice: number
  totalPrice: number
}

export interface SubOrderItemDetail {
  productId: string
  productSku: string
  productName: string
  quantity: number
}

export interface SubOrderDetail {
  id: string
  subOrderCode: string
  warehouseId: string
  warehouseName: string
  warehouseCode: string
  status: string
  items: SubOrderItemDetail[]
}

export interface OrderDetail {
  id: string
  tenantId: string
  orderCode: string
  channel: string
  customerName: string
  customerPhone: string
  customerAddress: string
  customerLatitude: number
  customerLongitude: number
  totalAmount: number
  shippingFee: number
  status: string
  createdAt: string
  orderItems: OrderItemDetail[]
  subOrders: SubOrderDetail[]
}

export interface CreateOrderRequest {
  tenantId: string
  orderCode: string
  channel: string
  customerName: string
  customerPhone: string
  customerAddress: string
  customerLatitude: number
  customerLongitude: number
  shippingFee: number
  orderItems: { productId: string; quantity: number; unitPrice: number }[]
}

export interface OrderAllocationResult {
  orderId: string
  orderCode: string
  status: string
  customerLatitude: number
  customerLongitude: number
  subOrderCount: number
  isSplitOrder: boolean
  subOrders: {
    subOrderId: string
    subOrderCode: string
    warehouseId: string
    warehouseName: string
    distanceKm: number
    items: { productId: string; quantity: number }[]
  }[]
}

export interface NewSubOrderEvent {
  orderId: string
  orderCode: string
  subOrderId: string
  subOrderCode: string
  warehouseId: string
  warehouseName: string
  distanceKm: number
  items: { productId: string; quantity: number }[]
  receivedAt: string
}

export interface UpdateOrderStatusRequest {
  status: string
}
