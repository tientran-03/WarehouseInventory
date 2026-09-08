import { useState } from 'react'
import type { FormEvent } from 'react'
import Alert from '../../components/ui/Alert'
import Button from '../../components/ui/Button'
import ConfirmModal from '../../components/ui/ConfirmModal'
import { FormField, ModalFooterButtons, SelectInput, TextInput } from '../../components/ui/FormField'
import LoadingState from '../../components/ui/LoadingState'
import Modal from '../../components/ui/Modal'
import PageSectionHeader from '../../components/ui/PageSectionHeader'
import { useTenantContext } from '../../context/TenantContext'
import { useToast } from '../../context/ToastContext'
import { useInventory } from '../../hooks/useInventory'
import { useProducts } from '../../hooks/useProducts'
import { useWarehouses } from '../../hooks/useWarehouses'
import { useZones } from '../../hooks/useZones'
import { parseApiError } from '../../utils/errorHandler'
import type { InventoryItem } from '../../types/product.types'

const emptyForm = { warehouseId: '', warehouseZoneId: '', productId: '', onHandStock: 0, reorderLevel: 10 }

export default function InventoryPage() {
  const { activeTenantId } = useTenantContext()
  const { showError, showSuccess } = useToast()
  const { items, loading, error, upsertInventory, updateInventory, deleteInventory } = useInventory(activeTenantId)
  const { products } = useProducts(activeTenantId)
  const { warehouses, loading: warehousesLoading } = useWarehouses()
  const { zones, loading: zonesLoading } = useZones(activeTenantId)
  const [modalOpen, setModalOpen] = useState(false)
  const [form, setForm] = useState(emptyForm)
  const [submitting, setSubmitting] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [search, setSearch] = useState('')
  const [editingItem, setEditingItem] = useState<InventoryItem | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<InventoryItem | null>(null)
  const [deleting, setDeleting] = useState(false)

  const tenantWarehouses = warehouses.filter((w) => w.tenantId === activeTenantId && w.isActive)
  const warehouseZones = zones.filter((zone) => zone.warehouseId === form.warehouseId)

  const filtered = items.filter(
    (i) =>
      i.productName.toLowerCase().includes(search.toLowerCase()) ||
      i.productSku.toLowerCase().includes(search.toLowerCase()) ||
      i.warehouseName.toLowerCase().includes(search.toLowerCase()) ||
      i.warehouseZoneCode?.toLowerCase().includes(search.toLowerCase()),
  )

  const closeModal = () => {
    if (submitting) return
    setModalOpen(false)
    setForm(emptyForm)
    setFormError(null)
    setEditingItem(null)
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (!activeTenantId) return

    setSubmitting(true)
    setFormError(null)
    try {
      const payload = {
        tenantId: activeTenantId,
        warehouseId: form.warehouseId,
        productId: form.productId,
        onHandStock: form.onHandStock,
        reorderLevel: form.reorderLevel,
        warehouseZoneId: form.warehouseZoneId,
      }
      if (editingItem) {
        await updateInventory(editingItem.id, payload)
        showSuccess('Cập nhật tồn kho thành công!')
      } else {
        await upsertInventory(payload)
        showSuccess('Thêm tồn kho thành công!')
      }
      setModalOpen(false)
      setForm(emptyForm)
      setFormError(null)
      setEditingItem(null)
    } catch (err) {
      const parsed = parseApiError(err)
      setFormError(parsed.message)
      showError(parsed.message)
    } finally {
      setSubmitting(false)
    }
  }

  const openCreateModal = () => {
    setForm(emptyForm)
    setEditingItem(null)
    setFormError(null)
    setModalOpen(true)
  }

  const openEditModal = (item: InventoryItem) => {
    setForm({
      warehouseId: item.warehouseId,
      warehouseZoneId: item.warehouseZoneId ?? '',
      productId: item.productId,
      onHandStock: item.onHandStock,
      reorderLevel: item.reorderLevel,
    })
    setEditingItem(item)
    setFormError(null)
    setModalOpen(true)
  }

  const handleDeleteInventory = async () => {
    if (!deleteTarget) return
    setDeleting(true)
    try {
      await deleteInventory(deleteTarget.id)
      showSuccess('Đã xóa dòng tồn kho.')
      setDeleteTarget(null)
    } catch (err) {
      showError(parseApiError(err).message, 'Không thể xóa tồn kho')
    } finally {
      setDeleting(false)
    }
  }

  if (!activeTenantId) {
    return <Alert variant="warning" message="Chọn tenant ở sidebar để xem tồn kho." />
  }

  if (loading || warehousesLoading || zonesLoading) return <LoadingState message="Đang tải dữ liệu kho & tồn kho..." />

  return (
    <div className="space-y-6">
      <PageSectionHeader
        title="Quản lý Tồn kho"
        badge={`${items.length} dòng`}
        action={
          <Button variant="primary" onClick={openCreateModal} disabled={tenantWarehouses.length === 0 || zones.length === 0}>
            Thêm tồn kho
          </Button>
        }
      />

      {tenantWarehouses.length === 0 ? (
        <Alert variant="info" message="Tenant hiện tại chưa có kho hoạt động. Hãy tạo kho trước khi cập nhật tồn kho." />
      ) : (
        <div className="flex justify-end">
          <input
            type="text"
            placeholder="Tìm SKU, sản phẩm, kho..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="w-full max-w-sm rounded-xl border border-slate-200 bg-slate-50/80 px-3.5 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-indigo-500/15 focus:border-indigo-300"
          />
        </div>
      )}

      {tenantWarehouses.length > 0 && zones.length === 0 && (
        <Alert variant="info" message="Hãy tạo ít nhất một khu vực lưu trữ trước khi cập nhật tồn kho." />
      )}

      {error && <Alert variant="error" message={error} />}

      {tenantWarehouses.length > 0 && (
        <div className="bg-white border border-slate-200 shadow-sm overflow-hidden rounded-xl">
          <table className="w-full text-sm text-left">
            <thead className="bg-slate-50/75 border-b border-slate-100 text-xs text-slate-500 uppercase">
              <tr>
                <th className="px-6 py-4">Kho</th>
                <th className="px-6 py-4">SKU</th>
                <th className="px-6 py-4">Sản phẩm</th>
                <th className="px-6 py-4 text-right">Tồn</th>
                <th className="px-6 py-4 text-right">Giữ chỗ</th>
                <th className="px-6 py-4 text-right">Khả dụng</th>
                <th className="px-6 py-4">Khu / vị trí</th>
                <th className="px-6 py-4 text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {filtered.length === 0 ? (
                <tr>
                  <td colSpan={8} className="px-6 py-12 text-center text-slate-400">Chưa có tồn kho.</td>
                </tr>
              ) : (
                filtered.map((i) => {
                  const low = i.availableStock <= i.reorderLevel
                  return (
                    <tr key={i.id} className="hover:bg-slate-50/80">
                      <td className="px-6 py-4 text-slate-700">{i.warehouseName}</td>
                      <td className="px-6 py-4 font-mono text-slate-600">{i.productSku}</td>
                      <td className="px-6 py-4 font-medium text-slate-900">{i.productName}</td>
                      <td className="px-6 py-4 text-right">{i.onHandStock}</td>
                      <td className="px-6 py-4 text-right text-amber-700">{i.reservedStock}</td>
                      <td className="px-6 py-4 text-right">
                        <span className={low ? 'text-rose-600 font-medium' : 'text-emerald-700'}>
                          {i.availableStock}
                        </span>
                      </td>
                      <td className="px-6 py-4 text-slate-500 text-xs">
                        {i.warehouseZoneCode
                          ? `${i.warehouseZoneCode}${i.warehouseZoneName ? ` — ${i.warehouseZoneName}` : ''}`
                          : i.locationInWarehouse ?? 'Chưa phân khu'}
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex justify-end gap-1">
                          <Button variant="ghost" size="sm" onClick={() => openEditModal(i)}>Sửa</Button>
                          <Button variant="danger-ghost" size="sm" onClick={() => setDeleteTarget(i)}>Xóa</Button>
                        </div>
                      </td>
                    </tr>
                  )
                })
              )}
            </tbody>
          </table>
        </div>
      )}

      <Modal
        open={modalOpen}
        onClose={closeModal}
        title={editingItem ? 'Chỉnh sửa tồn kho' : 'Thêm tồn kho'}
        description="Tồn kho được quản lý theo từng kho, sản phẩm và khu vực lưu trữ."
        size="lg"
        footer={
          <ModalFooterButtons onCancel={closeModal} submitLabel={editingItem ? 'Lưu thay đổi' : 'Thêm tồn kho'} loading={submitting} submitForm="inventory-form" />
        }
      >
        {formError && <div className="mb-4"><Alert variant="error" message={formError} /></div>}
        <form id="inventory-form" onSubmit={handleSubmit} className="space-y-4">
          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField label="Kho" required>
              <SelectInput
                value={form.warehouseId}
                onChange={(e) => setForm({ ...form, warehouseId: e.target.value, warehouseZoneId: '' })}
                required
              >
                <option value="">-- Chọn kho --</option>
                {tenantWarehouses.map((w) => (
                  <option key={w.id} value={w.id}>{w.name} ({w.code})</option>
                ))}
              </SelectInput>
            </FormField>
            <FormField label="Khu / vị trí lưu trữ" required>
              <SelectInput
                value={form.warehouseZoneId}
                onChange={(e) => setForm({ ...form, warehouseZoneId: e.target.value })}
                disabled={!form.warehouseId}
                required
              >
                <option value="">{form.warehouseId ? '-- Chọn khu vực --' : '-- Chọn kho trước --'}</option>
                {warehouseZones.map((zone) => (
                  <option key={zone.id} value={zone.id}>
                    {zone.code} — {zone.name} ({zone.usedCapacity}/{zone.capacity})
                  </option>
                ))}
              </SelectInput>
            </FormField>
            <FormField label="Sản phẩm" required>
              <SelectInput value={form.productId} onChange={(e) => setForm({ ...form, productId: e.target.value })} required>
                <option value="">-- Chọn SP --</option>
                {products.map((p) => (
                  <option key={p.id} value={p.id}>{p.sku} — {p.name}</option>
                ))}
              </SelectInput>
            </FormField>
            <FormField label="Tồn thực (On-hand)" required>
              <TextInput type="number" min={0} value={form.onHandStock} onChange={(e) => setForm({ ...form, onHandStock: Number(e.target.value) })} required />
            </FormField>
            <FormField label="Mức reorder">
              <TextInput type="number" min={0} value={form.reorderLevel} onChange={(e) => setForm({ ...form, reorderLevel: Number(e.target.value) })} />
            </FormField>
          </div>
        </form>
      </Modal>

      <ConfirmModal
        open={deleteTarget !== null}
        onClose={() => !deleting && setDeleteTarget(null)}
        onConfirm={handleDeleteInventory}
        title="Xóa dòng tồn kho?"
        message={`Dòng tồn kho của sản phẩm "${deleteTarget?.productName ?? ''}" sẽ bị xóa. Không thể xóa nếu đang có hàng giữ chỗ cho đơn hàng.`}
        confirmLabel="Xóa tồn kho"
        loading={deleting}
      />
    </div>
  )
}
