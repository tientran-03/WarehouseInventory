import { useState, type FormEvent } from 'react'
import Alert from '../../components/ui/Alert'
import Button from '../../components/ui/Button'
import { FormField, ModalFooterButtons, SelectInput, TextInput } from '../../components/ui/FormField'
import LoadingState from '../../components/ui/LoadingState'
import Modal from '../../components/ui/Modal'
import { useTenantContext } from '../../context/TenantContext'
import { useToast } from '../../context/ToastContext'
import { useProducts } from '../../hooks/useProducts'
import { useTenantWarehouses } from '../../hooks/useTenantWarehouses'
import { useTransfers } from '../../hooks/useTransfers'
import { transferStatusLabel } from '../../types'
import { parseApiError } from '../../utils/errorHandler'

export default function WarehouseTransfersPage() {
  const { activeTenantId } = useTenantContext()
  const { showError, showSuccess } = useToast()
  const { tenantWarehouses, loading: whLoading } = useTenantWarehouses()
  const { products } = useProducts(activeTenantId)
  const { transfers, loading, error, createTransfer, startTransfer, completeTransfer } = useTransfers(activeTenantId)

  const [showModal, setShowModal] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [form, setForm] = useState({
    productId: '',
    quantity: 1,
    fromWarehouseId: '',
    toWarehouseId: '',
  })

  const openModal = () => {
    setForm({
      productId: products[0]?.id ?? '',
      quantity: 1,
      fromWarehouseId: tenantWarehouses[0]?.id ?? '',
      toWarehouseId: tenantWarehouses[1]?.id ?? tenantWarehouses[0]?.id ?? '',
    })
    setFormError(null)
    setShowModal(true)
  }

  const handleCreate = async (e: FormEvent) => {
    e.preventDefault()
    if (!activeTenantId) return
    setSubmitting(true)
    setFormError(null)
    try {
      await createTransfer({
        tenantId: activeTenantId,
        productId: form.productId,
        quantity: form.quantity,
        fromWarehouseId: form.fromWarehouseId,
        toWarehouseId: form.toWarehouseId,
      })
      showSuccess('Tạo phiếu điều chuyển thành công!')
      setShowModal(false)
    } catch (err) {
      const msg = parseApiError(err).message
      setFormError(msg)
      showError(msg)
    } finally {
      setSubmitting(false)
    }
  }

  const handleStart = async (id: string) => {
    try {
      await startTransfer(id)
      showSuccess('Đã chuyển sang trạng thái Đang vận chuyển')
    } catch (err) {
      showError(parseApiError(err).message)
    }
  }

  const handleComplete = async (id: string) => {
    try {
      await completeTransfer(id)
      showSuccess('Hoàn thành điều chuyển — tồn kho đã cập nhật')
    } catch (err) {
      showError(parseApiError(err).message)
    }
  }

  const statusClass = (status: string) => {
    if (status === 'Completed') return 'bg-emerald-100 text-emerald-700'
    if (status === 'InTransit') return 'bg-amber-100 text-amber-700'
    return 'bg-slate-100 text-slate-700'
  }

  if (!activeTenantId) {
    return <Alert variant="warning" message="Chọn tenant ở sidebar để quản lý điều chuyển." />
  }

  if (loading || whLoading) return <LoadingState message="Đang tải phiếu điều chuyển..." />

  return (
    <div className="space-y-6">
      <div className="bg-white/60 backdrop-blur-md p-6 border border-white/70 shadow-sm rounded-2xl flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h2 className="text-xl font-normal text-slate-900 tracking-tight">Quản lý điều chuyển kho</h2>
          <p className="text-sm text-slate-500 mt-1 font-normal">
            Theo dõi và tạo mới các lệnh điều chuyển hàng hóa giữa các chi nhánh kho trong hệ thống.
          </p>
        </div>
        <Button variant="primary" onClick={openModal} disabled={tenantWarehouses.length < 2 || products.length === 0}>
          + Tạo lệnh điều chuyển
        </Button>
      </div>

      {error && <Alert variant="error" message={error} />}

      <div className="bg-white border border-slate-200 shadow-sm rounded-2xl overflow-hidden">
        <div className="px-6 py-5 border-b border-slate-100 flex items-center justify-between">
          <h3 className="font-normal text-slate-900 text-base">Danh sách phiếu điều chuyển</h3>
          <span className="text-xs font-normal px-3 py-1 rounded-full bg-white/50 text-slate-600 border border-white/70 backdrop-blur-sm">
            {transfers.length} phiếu
          </span>
        </div>
        {transfers.length === 0 ? (
          <p className="px-6 py-12 text-center text-slate-400 text-sm">Chưa có phiếu điều chuyển.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm text-left">
              <thead className="bg-slate-50/75 border-b border-slate-100 text-xs font-normal text-slate-500 uppercase tracking-wider">
                <tr>
                  <th className="px-6 py-4">Mã phiếu</th>
                  <th className="px-6 py-4">Sản phẩm</th>
                  <th className="px-6 py-4 text-center">Số lượng</th>
                  <th className="px-6 py-4">Kho xuất</th>
                  <th className="px-6 py-4">Kho nhận</th>
                  <th className="px-6 py-4">Ngày tạo</th>
                  <th className="px-6 py-4 text-center">Trạng thái</th>
                  <th className="px-6 py-4 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {transfers.map((item) => (
                  <tr key={item.id} className="hover:bg-slate-50/80 transition-colors">
                    <td className="px-6 py-4 font-mono text-xs text-slate-600">{item.transferCode}</td>
                    <td className="px-6 py-4 font-normal text-slate-900">{item.productName}</td>
                    <td className="px-6 py-4 text-center font-medium text-slate-900">{item.quantity}</td>
                    <td className="px-6 py-4 text-slate-600">{item.fromWarehouseName}</td>
                    <td className="px-6 py-4 text-slate-600">{item.toWarehouseName}</td>
                    <td className="px-6 py-4 text-slate-500 text-xs">{item.createdAt.slice(0, 10)}</td>
                    <td className="px-6 py-4 text-center">
                      <span className={`inline-flex px-3 py-1 text-xs font-normal rounded-full ${statusClass(item.status)}`}>
                        {transferStatusLabel(item.status)}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-right space-x-2">
                      {item.status === 'Pending' && (
                        <Button variant="secondary" size="sm" onClick={() => handleStart(item.id)}>Vận chuyển</Button>
                      )}
                      {item.status !== 'Completed' && (
                        <Button variant="primary" size="sm" onClick={() => handleComplete(item.id)}>Hoàn thành</Button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <Modal
        open={showModal}
        onClose={() => !submitting && setShowModal(false)}
        title="Tạo lệnh điều chuyển kho"
        description="Nhập thông tin sản phẩm và chọn kho xuất, kho nhận."
        footer={
          <ModalFooterButtons
            onCancel={() => setShowModal(false)}
            submitLabel="Xác nhận tạo phiếu"
            loading={submitting}
            submitForm="create-transfer-form"
          />
        }
      >
        {formError && <div className="mb-4"><Alert variant="error" message={formError} /></div>}
        <form id="create-transfer-form" onSubmit={handleCreate} className="space-y-4">
          <FormField label="Sản phẩm" required>
            <SelectInput value={form.productId} onChange={(e) => setForm({ ...form, productId: e.target.value })} required>
              {products.map((p) => (
                <option key={p.id} value={p.id}>{p.name} ({p.sku})</option>
              ))}
            </SelectInput>
          </FormField>
          <FormField label="Số lượng" required>
            <TextInput type="number" min={1} required value={form.quantity} onChange={(e) => setForm({ ...form, quantity: Number(e.target.value) })} />
          </FormField>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <FormField label="Kho xuất (Gửi đi)" required>
              <SelectInput value={form.fromWarehouseId} onChange={(e) => setForm({ ...form, fromWarehouseId: e.target.value })} required>
                {tenantWarehouses.map((w) => (
                  <option key={w.id} value={w.id}>{w.name}</option>
                ))}
              </SelectInput>
            </FormField>
            <FormField label="Kho nhận" required>
              <SelectInput value={form.toWarehouseId} onChange={(e) => setForm({ ...form, toWarehouseId: e.target.value })} required>
                {tenantWarehouses.map((w) => (
                  <option key={w.id} value={w.id}>{w.name}</option>
                ))}
              </SelectInput>
            </FormField>
          </div>
        </form>
      </Modal>
    </div>
  )
}
