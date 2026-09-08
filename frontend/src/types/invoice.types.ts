export interface InvoiceLine {
  productId: string
  productSku: string
  productName: string
  warehouseZoneCode: string
  quantity: number
  unitPrice: number
  lineTotal: number
}

export interface Invoice {
  id: string
  tenantId: string
  invoiceCode: string
  stockDocumentId: string
  stockDocumentCode: string
  customerName: string
  customerAddress: string | null
  customerPhone: string | null
  customerTaxCode: string | null
  subtotal: number
  taxRate: number
  taxAmount: number
  totalAmount: number
  notes: string | null
  invoiceDate: string
  status: string
  lines: InvoiceLine[]
}

export interface CreateInvoiceRequest {
  tenantId: string
  stockDocumentId: string
  customerName: string
  customerAddress?: string | null
  customerPhone?: string | null
  customerTaxCode?: string | null
  taxRate?: number
  notes?: string | null
}
