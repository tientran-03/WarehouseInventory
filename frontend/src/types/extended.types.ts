export interface Transfer {
  id: string
  tenantId: string
  transferCode: string
  productId: string
  productName: string
  productSku: string
  quantity: number
  fromWarehouseId: string
  fromWarehouseName: string
  toWarehouseId: string
  toWarehouseName: string
  status: string
  createdAt: string
}

export interface CreateTransferRequest {
  tenantId: string
  productId: string
  quantity: number
  fromWarehouseId: string
  toWarehouseId: string
}

export interface Stocktake {
  id: string
  tenantId: string
  stocktakeCode: string
  warehouseId: string
  warehouseName: string
  auditor: string
  status: string
  totalItems: number
  discrepancyCount: number
  createdAt: string
}

export interface CreateStocktakeRequest {
  tenantId: string
  warehouseId: string
  auditor: string
}

export interface StocktakeDiscrepancy {
  productId: string
  productSku: string
  productName: string
  systemQuantity: number
  actualQuantity: number
  discrepancy: number
  reason: string
}

export interface CompleteStocktakeRequest {
  discrepancyCount: number
  discrepancies: StocktakeDiscrepancy[]
  note?: string
}

export interface WarehouseZone {
  id: string
  tenantId: string
  warehouseId: string
  warehouseName: string
  code: string
  name: string
  capacity: number
  usedCapacity: number
}

export interface CreateZoneRequest {
  tenantId: string
  warehouseId: string
  code: string
  name: string
  capacity: number
}

export interface ShippingRoute {
  id: string
  tenantId: string
  name: string
  fromWarehouseId: string
  fromWarehouseName: string
  toRegion: string
  distanceKm: number
  estimatedDays: number
  status: string
  createdAt: string
}

export interface CreateShippingRouteRequest {
  tenantId: string
  name: string
  fromWarehouseId: string
  toRegion: string
  distanceKm: number
  estimatedDays: number
}

export interface WarehouseFinancialSummary {
  warehouseId: string
  warehouseName: string
  totalUnits: number
  totalValue: number
  lineCount: number
}

export interface FinancialSummary {
  grandTotal: number
  warehouseCount: number
  inventoryLineCount: number
  warehouses: WarehouseFinancialSummary[]
}

export interface BalancingSuggestion {
  productId: string
  productName: string
  productSku: string
  fromWarehouseName: string
  toWarehouseName: string
  suggestedQty: number
  imbalance: number
}

export type StockDocumentType = 'Inbound' | 'Outbound'

export interface StockDocumentLine {
  id: string
  productId: string
  productSku: string
  productName: string
  warehouseZoneId: string
  warehouseZoneCode: string
  warehouseZoneName: string
  quantity: number
  unitPrice: number
  lineTotal: number
}

export interface StockDocument {
  id: string
  tenantId: string
  documentCode: string
  type: StockDocumentType
  status: string
  warehouseId: string
  warehouseName: string
  documentDate: string
  partnerName: string | null
  referenceCode: string | null
  note: string | null
  createdBy: string
  createdAt: string
  lines: StockDocumentLine[]
  totalQuantity: number
  totalValue: number
}

export interface CreateStockDocumentLineRequest {
  productId: string
  warehouseZoneId: string
  quantity: number
  unitPrice?: number | null
}

export interface CreateStockDocumentRequest {
  tenantId: string
  warehouseId: string
  type: StockDocumentType
  documentDate?: string | null
  partnerName?: string | null
  referenceCode?: string | null
  note?: string | null
  lines: CreateStockDocumentLineRequest[]
}

export interface StockMovementReportRow {
  documentDate: string
  documentCode: string
  type: StockDocumentType
  warehouseName: string
  productSku: string
  productName: string
  warehouseZoneCode: string
  quantity: number
  unitPrice: number
  lineTotal: number
  partnerName: string | null
  referenceCode: string | null
  note: string | null
}

export interface StockMovementReport {
  fromDate: string | null
  toDate: string | null
  documentCount: number
  inboundQuantity: number
  outboundQuantity: number
  inboundValue: number
  outboundValue: number
  rows: StockMovementReportRow[]
}

export interface StockMovementReportFilter {
  tenantId: string
  warehouseId?: string
  type?: StockDocumentType
  fromDate?: string
  toDate?: string
}

export function transferStatusLabel(status: string) {
  switch (status) {
    case 'Pending': return 'Chờ xử lý'
    case 'InTransit': return 'Đang vận chuyển'
    case 'Completed': return 'Đã hoàn thành'
    default: return status
  }
}

export function stocktakeStatusLabel(status: string) {
  switch (status) {
    case 'InProgress': return 'Đang kiểm kê'
    case 'Completed': return 'Hoàn thành'
    default: return status
  }
}

export function stockDocumentTypeLabel(type: StockDocumentType) {
  return type === 'Inbound' ? 'Nhập kho' : 'Xuất kho'
}
