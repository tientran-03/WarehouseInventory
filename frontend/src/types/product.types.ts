export interface Product {
  id: string
  tenantId: string
  categoryId: string
  sku: string
  name: string
  price: number
  barcode: string | null
  imageUrl: string | null
  isActive: boolean
}

export interface UpsertProductRequest {
  tenantId: string
  categoryId: string
  sku: string
  name: string
  price: number
  barcode?: string | null
  imageUrl?: string | null
}

export interface Category {
  id: string
  tenantId: string
  name: string
  slug: string
}

export interface UpsertCategoryRequest {
  tenantId: string
  name: string
}

export interface InventoryItem {
  id: string
  tenantId: string
  warehouseId: string
  warehouseName: string
  warehouseCode: string
  productId: string
  productSku: string
  productName: string
  onHandStock: number
  reservedStock: number
  availableStock: number
  reorderLevel: number
  locationInWarehouse: string | null
  warehouseZoneId: string | null
  warehouseZoneCode: string | null
  warehouseZoneName: string | null
}

export interface UpsertInventoryRequest {
  tenantId: string
  warehouseId: string
  productId: string
  onHandStock: number
  reorderLevel?: number
  locationInWarehouse?: string | null
  warehouseZoneId?: string | null
}
