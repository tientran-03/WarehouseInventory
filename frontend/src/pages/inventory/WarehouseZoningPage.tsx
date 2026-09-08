import { useState, useEffect, type FormEvent } from 'react'
import Alert from '../../components/ui/Alert'
import Button from '../../components/ui/Button'
import { FormField, ModalFooterButtons, SelectInput, TextInput } from '../../components/ui/FormField'
import LoadingState from '../../components/ui/LoadingState'
import Modal from '../../components/ui/Modal'
import { useTenantContext } from '../../context/TenantContext'
import { useToast } from '../../context/ToastContext'
import { useTenantWarehouses } from '../../hooks/useTenantWarehouses'
import { useZones } from '../../hooks/useZones'
import { parseApiError } from '../../utils/errorHandler'

export default function WarehouseZoningPage() {
  const { activeTenantId } = useTenantContext()
  const { showError, showSuccess } = useToast()
  const { tenantWarehouses, loading: whLoading } = useTenantWarehouses()
  const { zones, loading, error, createZone, deleteZone } = useZones(activeTenantId)

  const [showModal, setShowModal] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [form, setForm] = useState({ warehouseId: '', code: '', name: '', capacity: 100 })
  const [expandedWarehouses, setExpandedWarehouses] = useState<Set<string>>(new Set())
  const criticalZones = zones.filter((zone) => zone.capacity > 0 && (zone.usedCapacity / zone.capacity) >= 0.98)

  // Group zones by warehouse
  const zonesByWarehouse = tenantWarehouses.reduce((acc, warehouse) => {
    acc[warehouse.id] = zones.filter((zone) => zone.warehouseId === warehouse.id)
    return acc
  }, {} as Record<string, typeof zones>)

  const toggleWarehouse = (warehouseId: string) => {
    setExpandedWarehouses((prev) => {
      const next = new Set(prev)
      if (next.has(warehouseId)) {
        next.delete(warehouseId)
      } else {
        next.add(warehouseId)
      }
      return next
    })
  }

  // Expand all warehouses by default
  useEffect(() => {
    if (tenantWarehouses.length > 0 && expandedWarehouses.size === 0) {
      setExpandedWarehouses(new Set(tenantWarehouses.map((w) => w.id)))
    }
  }, [tenantWarehouses])

  const getZoneStatus = (usedCapacity: number, capacity: number) => {
    const usagePercent = capacity > 0 ? (usedCapacity / capacity) * 100 : 0
    if (usagePercent >= 98) {
      return {
        label: 'Sắp đầy',
        cardClass: 'bg-rose-50 border-rose-300',
        textClass: 'text-rose-700',
        progressClass: 'bg-rose-500',
        trackClass: 'bg-rose-200',
      }
    }
    if (usagePercent >= 75) {
      return {
        label: 'Gần đầy',
        cardClass: 'bg-amber-50 border-amber-300',
        textClass: 'text-amber-800',
        progressClass: 'bg-amber-500',
        trackClass: 'bg-amber-200',
      }
    }
    return {
      label: 'Ổn định',
      cardClass: 'bg-emerald-50 border-emerald-200',
      textClass: 'text-emerald-800',
      progressClass: 'bg-emerald-500',
      trackClass: 'bg-emerald-200',
    }
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (!activeTenantId) return
    setSubmitting(true)
    setFormError(null)
    try {
      await createZone({
        tenantId: activeTenantId,
        warehouseId: form.warehouseId || tenantWarehouses[0]?.id,
        code: form.code,
        name: form.name,
        capacity: form.capacity,
      })
      showSuccess('Thêm khu vực thành công!')
      setShowModal(false)
      setForm({ warehouseId: '', code: '', name: '', capacity: 100 })
    } catch (err) {
      const msg = parseApiError(err).message
      setFormError(msg)
      showError(msg)
    } finally {
      setSubmitting(false)
    }
  }

  if (!activeTenantId) {
    return <Alert variant="warning" message="Chọn tenant ở sidebar để phân khu kho." />
  }

  if (loading || whLoading) return <LoadingState message="Đang tải phân khu..." />

  return (
    <div className="space-y-6">
      <div className="bg-white p-6 border border-slate-200 shadow-sm rounded-2xl flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h2 className="text-xl font-normal text-slate-900">Phân khu & Vị trí lưu trữ</h2>
          <p className="text-sm text-slate-500 mt-1">Theo dõi sức chứa từng khu vực: xanh lá là ổn định, vàng là gần đầy, đỏ là sắp đầy.</p>
        </div>
        <Button variant="primary" onClick={() => setShowModal(true)} disabled={tenantWarehouses.length === 0}>
          + Thêm khu vực
        </Button>
      </div>

      {error && <Alert variant="error" message={error} />}

      {criticalZones.length > 0 && (
        <Alert
          variant="error"
          title="Cảnh báo khu vực gần đầy"
          message={`${criticalZones.map((zone) => `${zone.warehouseName} · ${zone.code}`).join(', ')} đã sử dụng từ 98% sức chứa.`}
        />
      )}

      <div className="flex flex-wrap gap-3 text-xs">
        <span className="inline-flex items-center gap-2 rounded-full bg-emerald-50 border border-emerald-200 px-3 py-1.5 text-emerald-800">
          <span className="h-2 w-2 rounded-full bg-emerald-500" /> Ổn định: dưới 75%
        </span>
        <span className="inline-flex items-center gap-2 rounded-full bg-amber-50 border border-amber-200 px-3 py-1.5 text-amber-800">
          <span className="h-2 w-2 rounded-full bg-amber-500" /> Gần đầy: 75–97%
        </span>
        <span className="inline-flex items-center gap-2 rounded-full bg-rose-50 border border-rose-200 px-3 py-1.5 text-rose-800">
          <span className="h-2 w-2 rounded-full bg-rose-500" /> Sắp đầy: từ 98%
        </span>
      </div>

      <div className="space-y-4">
        {zones.length === 0 ? (
          <div className="bg-white border border-slate-200 p-12 text-center text-slate-400 rounded-2xl">
            Chưa có khu vực nào được tạo.
          </div>
        ) : (
          tenantWarehouses.map((warehouse) => {
            const warehouseZones = zonesByWarehouse[warehouse.id] || []
            const isExpanded = expandedWarehouses.has(warehouse.id)
            
            if (warehouseZones.length === 0) return null

            return (
              <div key={warehouse.id} className="bg-white border border-slate-200 rounded-2xl overflow-hidden">
                <button
                  onClick={() => toggleWarehouse(warehouse.id)}
                  className="w-full px-6 py-4 flex items-center justify-between hover:bg-slate-50 transition-colors"
                >
                  <div className="flex items-center gap-3">
                    <span className={`transform transition-transform ${isExpanded ? 'rotate-90' : ''}`}>▶</span>
                    <h3 className="font-medium text-slate-900">{warehouse.name}</h3>
                    <span className="text-xs text-slate-500 bg-slate-100 px-2 py-0.5 rounded-full">
                      {warehouseZones.length} khu vực
                    </span>
                  </div>
                </button>
                
                {isExpanded && (
                  <div className="px-6 pb-6">
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                      {warehouseZones.map((z) => {
                        const usagePercent = z.capacity > 0 ? (z.usedCapacity / z.capacity) * 100 : 0
                        const status = getZoneStatus(z.usedCapacity, z.capacity)
                        return (
                          <div
                            key={z.id}
                            className={`border p-5 shadow-sm rounded-xl space-y-3 ${status.cardClass}`}
                          >
                            <div className="flex items-start justify-between">
                              <div>
                                <p className={`text-xs font-mono ${status.textClass}`}>{z.code}</p>
                                <h3 className="font-medium text-slate-900 mt-1">{z.name}</h3>
                              </div>
                              <Button variant="ghost" size="sm" onClick={() => deleteZone(z.id)}>Xóa</Button>
                            </div>
                            <div className="space-y-1">
                              <div className="flex justify-between text-xs">
                                <span className={`${status.textClass} font-medium`}>{status.label}</span>
                                <span className={`${status.textClass} font-medium`}>{z.usedCapacity} / {z.capacity} ({Math.round(usagePercent)}%)</span>
                              </div>
                              <div className={`h-1.5 rounded-full overflow-hidden ${status.trackClass}`}>
                                <div
                                  className={`h-full rounded-full ${status.progressClass}`}
                                  style={{ width: `${Math.min(100, usagePercent)}%` }}
                                />
                              </div>
                            </div>
                          </div>
                        )
                      })}
                    </div>
                  </div>
                )}
              </div>
            )
          })
        )}
      </div>

      <Modal
        open={showModal}
        onClose={() => !submitting && setShowModal(false)}
        title="Thêm khu vực mới"
        footer={<ModalFooterButtons onCancel={() => setShowModal(false)} submitLabel="Thêm khu vực" loading={submitting} submitForm="zone-form" />}
      >
        {formError && <div className="mb-4"><Alert variant="error" message={formError} /></div>}
        <form id="zone-form" onSubmit={handleSubmit} className="space-y-4">
          <FormField label="Kho" required>
            <SelectInput value={form.warehouseId || tenantWarehouses[0]?.id} onChange={(e) => setForm({ ...form, warehouseId: e.target.value })} required>
              {tenantWarehouses.map((w) => (
                <option key={w.id} value={w.id}>{w.name}</option>
              ))}
            </SelectInput>
          </FormField>
          <FormField label="Mã khu" required>
            <TextInput value={form.code} onChange={(e) => setForm({ ...form, code: e.target.value })} required placeholder="VD: A-01" />
          </FormField>
          <FormField label="Tên khu" required>
            <TextInput value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} required />
          </FormField>
          <FormField label="Sức chứa" required>
            <TextInput type="number" min={1} value={form.capacity} onChange={(e) => setForm({ ...form, capacity: Number(e.target.value) })} required />
          </FormField>
        </form>
      </Modal>
    </div>
  )
}
