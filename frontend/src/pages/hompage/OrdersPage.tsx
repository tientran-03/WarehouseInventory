import { useCallback, useState } from 'react'
import type { FormEvent } from 'react'
import Alert from '../../components/ui/Alert'
import Button from '../../components/ui/Button'
import { FormField, ModalFooterButtons, SelectInput, TextInput } from '../../components/ui/FormField'
import LoadingState from '../../components/ui/LoadingState'
import Modal from '../../components/ui/Modal'
import PageSectionHeader from '../../components/ui/PageSectionHeader'
import { useTenantContext } from '../../context/TenantContext'
import { useToast } from '../../context/ToastContext'
import { useOrders } from '../../hooks/useOrders'
import { useProducts } from '../../hooks/useProducts'
import { useWarehouseOrdersHub } from '../../hooks/useWarehouseOrdersHub'
import { useWarehouses } from '../../hooks/useWarehouses'
import { orderService } from '../../services'
import type { NewSubOrderEvent, OrderDetail } from '../../types'
import { parseApiError } from '../../utils/errorHandler'

const emptyOrderForm = {
  orderCode: '',
  channel: 'Web',
  customerName: '',
  customerPhone: '',
  customerAddress: '',
  customerLatitude: 0,
  customerLongitude: 0,
  shippingFee: 0,
  productId: '',
  quantity: 1,
  unitPrice: 0,
}

const orderStatusLabel = (status: string): string => {
  const labels: Record<string, string> = {
    'Pending': 'Chờ xử lý',
    'Allocated': 'Đã phân bổ',
    'Processing': 'Đang xử lý',
    'Shipped': 'Đang vận chuyển',
    'Delivered': 'Đã giao',
    'Cancelled': 'Đã hủy',
  }
  return labels[status] || status
}

export default function OrdersPage() {
  const { activeTenantId } = useTenantContext()
  const { showError, showSuccess, showWarning } = useToast()
  const { orders, loading, error, fetchOrders, createOrder, getOrderDetail } = useOrders(activeTenantId)
  const { products } = useProducts(activeTenantId)
  const { warehouses } = useWarehouses()

  const [createOpen, setCreateOpen] = useState(false)
  const [detailOpen, setDetailOpen] = useState(false)
  const [detail, setDetail] = useState<OrderDetail | null>(null)
  const [form, setForm] = useState(emptyOrderForm)
  const [submitting, setSubmitting] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [statusUpdating, setStatusUpdating] = useState(false)
  const [stockDocCreating, setStockDocCreating] = useState(false)

  const warehouseIds = warehouses.filter((w) => w.tenantId === activeTenantId).map((w) => w.id)

  const onNewSubOrder = useCallback(
    (event: NewSubOrderEvent) => {
      showWarning(`Đơn mới ${event.orderCode} → kho ${event.warehouseName}`, 'SignalR')
      fetchOrders()
    },
    [fetchOrders, showWarning],
  )

  useWarehouseOrdersHub(warehouseIds, onNewSubOrder)

  const openDetail = async (id: string) => {
    try {
      const data = await getOrderDetail(id)
      setDetail(data)
      setDetailOpen(true)
    } catch (err) {
      showError(parseApiError(err).message)
    }
  }

  const handleUpdateStatus = async (status: string) => {
    if (!detail) return
    setStatusUpdating(true)
    try {
      const updated = await orderService.updateStatus(detail.id, { status })
      setDetail(updated.data)
      fetchOrders()
      showSuccess('Đã cập nhật trạng thái đơn hàng')
    } catch (err) {
      showError(parseApiError(err).message)
    } finally {
      setStatusUpdating(false)
    }
  }

  const handleCreateStockDocuments = async () => {
    if (!detail) return
    setStockDocCreating(true)
    try {
      const updated = await orderService.createStockDocuments(detail.id)
      setDetail(updated.data)
      fetchOrders()
      showSuccess('Đã tạo phiếu xuất từ đơn hàng')
    } catch (err) {
      showError(parseApiError(err).message)
    } finally {
      setStockDocCreating(false)
    }
  }

  const handleCreate = async (e: FormEvent) => {
    e.preventDefault()
    if (!activeTenantId) return

    setSubmitting(true)
    setFormError(null)
    try {
      const result = await createOrder({
        tenantId: activeTenantId,
        orderCode: form.orderCode,
        channel: form.channel,
        customerName: form.customerName,
        customerPhone: form.customerPhone,
        customerAddress: form.customerAddress,
        customerLatitude: form.customerLatitude,
        customerLongitude: form.customerLongitude,
        shippingFee: form.shippingFee,
        orderItems: [{ productId: form.productId, quantity: form.quantity, unitPrice: form.unitPrice }],
      })
      showSuccess(
        result.isSplitOrder
          ? `Phân bổ thành công — ${result.subOrderCount} kho (split order)`
          : `Phân bổ thành công — 1 kho`,
      )
      setCreateOpen(false)
      setForm(emptyOrderForm)
    } catch (err) {
      const parsed = parseApiError(err)
      setFormError(parsed.message)
      showError(parsed.message)
    } finally {
      setSubmitting(false)
    }
  }

  if (!activeTenantId) {
    return <Alert variant="warning" message="Chọn tenant ở sidebar để xem đơn hàng." />
  }

  if (loading) return <LoadingState message="Đang tải đơn hàng..." />

  return (
    <div className="space-y-6">
      <PageSectionHeader
        title="Đơn hàng & Phân bổ"
        badge={`${orders.length} đơn`}
        action={
          <Button variant="primary" onClick={() => setCreateOpen(true)}>
            Tạo đơn thử
          </Button>
        }
      />

      {error && <Alert variant="error" message={error} />}

      <div className="bg-white border border-slate-200 shadow-sm overflow-hidden rounded-xl">
        <table className="w-full text-sm text-left">
          <thead className="bg-slate-50/75 border-b border-slate-100 text-xs text-slate-500 uppercase">
            <tr>
              <th className="px-6 py-4">Mã đơn</th>
              <th className="px-6 py-4">Kênh</th>
              <th className="px-6 py-4">Khách</th>
              <th className="px-6 py-4">Trạng thái</th>
              <th className="px-6 py-4 text-center">Sub-orders</th>
              <th className="px-6 py-4 text-right">Tổng tiền</th>
              <th className="px-6 py-4 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {orders.length === 0 ? (
              <tr>
                <td colSpan={7} className="px-6 py-12 text-center text-slate-400">
                  Chưa có đơn. Gọi POST /api/orders/webhook hoặc &quot;Tạo đơn thử&quot;.
                </td>
              </tr>
            ) : (
              orders.map((o) => (
                <tr key={o.id} className="hover:bg-slate-50/80">
                  <td className="px-6 py-4 font-mono text-slate-700">{o.orderCode}</td>
                  <td className="px-6 py-4">{o.channel}</td>
                  <td className="px-6 py-4">{o.customerName}</td>
                  <td className="px-6 py-4">
                    <span className="px-2 py-1 rounded-full text-xs bg-slate-100 text-slate-600">{orderStatusLabel(o.status)}</span>
                  </td>
                  <td className="px-6 py-4 text-center">
                    {o.subOrderCount}
                    {o.isSplitOrder && (
                      <span className="ml-1 text-xs text-amber-600">split</span>
                    )}
                  </td>
                  <td className="px-6 py-4 text-right">{o.totalAmount.toLocaleString('vi-VN')} ₫</td>
                  <td className="px-6 py-4 text-right">
                    <Button variant="ghost" size="sm" onClick={() => openDetail(o.id)}>Chi tiết</Button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <Modal
        open={createOpen}
        onClose={() => !submitting && setCreateOpen(false)}
        title="Tạo đơn & phân bổ"
        description="Mô phỏng webhook — engine Haversine + kiểm tra tồn kho."
        size="lg"
        footer={
          <ModalFooterButtons
            onCancel={() => setCreateOpen(false)}
            submitLabel="Phân bổ đơn"
            loading={submitting}
            submitForm="order-form"
          />
        }
      >
        {formError && <div className="mb-4"><Alert variant="error" message={formError} /></div>}
        <form id="order-form" onSubmit={handleCreate} className="space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField label="Mã đơn" required>
              <TextInput value={form.orderCode} onChange={(e) => setForm({ ...form, orderCode: e.target.value })} placeholder="WEB-001" required />
            </FormField>
            <FormField label="Kênh" required>
              <SelectInput value={form.channel} onChange={(e) => setForm({ ...form, channel: e.target.value })}>
                <option value="Web">Web</option>
                <option value="Shopee">Shopee</option>
              </SelectInput>
            </FormField>
            <FormField label="Tên khách" required>
              <TextInput value={form.customerName} onChange={(e) => setForm({ ...form, customerName: e.target.value })} required />
            </FormField>
            <FormField label="SĐT" required>
              <TextInput value={form.customerPhone} onChange={(e) => setForm({ ...form, customerPhone: e.target.value })} required />
            </FormField>
            <FormField label="Địa chỉ" required>
              <TextInput value={form.customerAddress} onChange={(e) => setForm({ ...form, customerAddress: e.target.value })} required />
            </FormField>
            <FormField label="Sản phẩm" required>
              <SelectInput
                value={form.productId}
                onChange={(e) => {
                  const p = products.find((x) => x.id === e.target.value)
                  setForm({ ...form, productId: e.target.value, unitPrice: p?.price ?? 0 })
                }}
                required
              >
                <option value="">-- Chọn --</option>
                {products.map((p) => (
                  <option key={p.id} value={p.id}>{p.sku} — {p.name}</option>
                ))}
              </SelectInput>
            </FormField>
            <FormField label="Số lượng" required>
              <TextInput type="number" min={1} value={form.quantity} onChange={(e) => setForm({ ...form, quantity: Number(e.target.value) })} required />
            </FormField>
            <FormField label="Phí ship">
              <TextInput type="number" min={0} value={form.shippingFee} onChange={(e) => setForm({ ...form, shippingFee: Number(e.target.value) })} />
            </FormField>
          </div>
        </form>
      </Modal>

      <Modal
        open={detailOpen}
        onClose={() => setDetailOpen(false)}
        title={detail ? `Đơn ${detail.orderCode}` : 'Chi tiết đơn'}
        description={detail ? `${detail.channel} · ${detail.customerAddress}` : undefined}
        size="lg"
        footer={
          <div className="flex gap-2">
            <Button variant="secondary" onClick={() => setDetailOpen(false)}>Đóng</Button>
            {detail && detail.status === 'Allocated' && (
              <Button
                variant="primary"
                onClick={handleCreateStockDocuments}
                loading={stockDocCreating}
              >
                Tạo phiếu xuất
              </Button>
            )}
            {detail && detail.status !== 'Cancelled' && detail.status !== 'Delivered' && (
              <SelectInput
                value={detail.status}
                onChange={(e) => handleUpdateStatus(e.target.value)}
                disabled={statusUpdating}
                className="w-32"
              >
                <option value="Pending">Chờ xử lý</option>
                <option value="Allocated">Đã phân bổ</option>
                <option value="Processing">Đang xử lý</option>
                <option value="Shipped">Đang vận chuyển</option>
                <option value="Delivered">Đã giao</option>
                <option value="Cancelled">Đã hủy</option>
              </SelectInput>
            )}
          </div>
        }
      >
        {detail && (
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-3 text-sm">
              <div><span className="text-slate-500">Khách:</span> {detail.customerName}</div>
              <div><span className="text-slate-500">Trạng thái:</span> {orderStatusLabel(detail.status)}</div>
              <div><span className="text-slate-500">Tổng:</span> {detail.totalAmount.toLocaleString('vi-VN')} ₫</div>
            </div>
            <div>
              <h4 className="text-sm font-medium text-slate-900 mb-2">Sub-orders theo kho</h4>
              <div className="space-y-3">
                {detail.subOrders.map((s) => (
                  <div key={s.id} className="p-3 rounded-xl border border-slate-200 bg-slate-50/50">
                    <div className="flex justify-between text-sm">
                      <span className="font-medium">{s.subOrderCode}</span>
                      <span className="text-slate-500">{s.warehouseName}</span>
                    </div>
                    <ul className="mt-2 text-xs text-slate-600 space-y-1">
                      {s.items.map((i) => (
                        <li key={i.productId}>{i.productSku} × {i.quantity}</li>
                      ))}
                    </ul>
                  </div>
                ))}
              </div>
            </div>
          </div>
        )}
      </Modal>
    </div>
  )
}
