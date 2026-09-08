import { useMemo, useState, type FormEvent } from 'react'
import Alert from '../../components/ui/Alert'
import Button from '../../components/ui/Button'
import { FormField, ModalFooterButtons, SelectInput, TextInput } from '../../components/ui/FormField'
import LoadingState from '../../components/ui/LoadingState'
import Modal from '../../components/ui/Modal'
import StatCard from '../../components/StatCard'
import { useTenantContext } from '../../context/TenantContext'
import { useToast } from '../../context/ToastContext'
import { useStocktakes } from '../../hooks/useStocktakes'
import { useTenantWarehouses } from '../../hooks/useTenantWarehouses'
import type { StocktakeDiscrepancy } from '../../types'
import { stocktakeStatusLabel } from '../../types'
import { parseApiError } from '../../utils/errorHandler'

export default function StocktakesPage() {
  const { activeTenantId } = useTenantContext()
  const { showError, showSuccess } = useToast()
  const { tenantWarehouses, loading: whLoading } = useTenantWarehouses()
  const { stocktakes, loading, error, createStocktake, completeStocktake } = useStocktakes(activeTenantId)

  const [showModal, setShowModal] = useState(false)
  const [showCompleteModal, setShowCompleteModal] = useState(false)
  const [selectedStocktake, setSelectedStocktake] = useState<string | null>(null)
  const [submitting, setSubmitting] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [form, setForm] = useState({ warehouseId: '', auditor: '' })
  const [discrepancies, setDiscrepancies] = useState<StocktakeDiscrepancy[]>([])
  const [note, setNote] = useState('')

  const stats = useMemo(() => ({
    total: stocktakes.length,
    inProgress: stocktakes.filter((s) => s.status === 'InProgress').length,
    discrepancies: stocktakes.reduce((sum, s) => sum + s.discrepancyCount, 0),
  }), [stocktakes])

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (!activeTenantId) return
    setSubmitting(true)
    setFormError(null)
    try {
      await createStocktake({
        tenantId: activeTenantId,
        warehouseId: form.warehouseId || tenantWarehouses[0]?.id,
        auditor: form.auditor,
      })
      showSuccess('Bắt đầu phiên kiểm kê thành công!')
      setShowModal(false)
      setForm({ warehouseId: '', auditor: '' })
    } catch (err) {
      const msg = parseApiError(err).message
      setFormError(msg)
      showError(msg)
    } finally {
      setSubmitting(false)
    }
  }

  const handleComplete = async (id: string) => {
    setSelectedStocktake(id)
    setDiscrepancies([])
    setNote('')
    setShowCompleteModal(true)
  }

  const addDiscrepancy = () => {
    setDiscrepancies([
      ...discrepancies,
      {
        productId: '',
        productSku: '',
        productName: '',
        systemQuantity: 0,
        actualQuantity: 0,
        discrepancy: 0,
        reason: '',
      },
    ])
  }

  const removeDiscrepancy = (index: number) => {
    setDiscrepancies(discrepancies.filter((_, i) => i !== index))
  }

  const updateDiscrepancy = (index: number, field: keyof StocktakeDiscrepancy, value: string | number) => {
    const updated = [...discrepancies]
    updated[index] = { ...updated[index], [field]: value }
    
    // Auto-calculate discrepancy when systemQuantity or actualQuantity changes
    if (field === 'systemQuantity' || field === 'actualQuantity') {
      const sysQty = field === 'systemQuantity' ? Number(value) : updated[index].systemQuantity
      const actQty = field === 'actualQuantity' ? Number(value) : updated[index].actualQuantity
      updated[index].discrepancy = actQty - sysQty
    }
    
    setDiscrepancies(updated)
  }

  const handleCompleteSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (!selectedStocktake) return
    
    setSubmitting(true)
    setFormError(null)
    try {
      const totalDiscrepancy = discrepancies.reduce((sum, d) => sum + Math.abs(d.discrepancy), 0)
      await completeStocktake(selectedStocktake, {
        discrepancyCount: totalDiscrepancy,
        discrepancies,
        note: note || undefined,
      })
      showSuccess('Hoàn thành kiểm kê thành công!')
      setShowCompleteModal(false)
      setSelectedStocktake(null)
      setDiscrepancies([])
      setNote('')
    } catch (err) {
      const msg = parseApiError(err).message
      setFormError(msg)
      showError(msg)
    } finally {
      setSubmitting(false)
    }
  }

  if (!activeTenantId) {
    return <Alert variant="warning" message="Chọn tenant ở sidebar để quản lý kiểm kê." />
  }

  if (loading || whLoading) return <LoadingState message="Đang tải phiên kiểm kê..." />

  return (
    <div className="space-y-6">
      <div className="bg-white p-6 border border-slate-200 shadow-sm rounded-2xl flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h2 className="text-xl font-normal text-slate-900 tracking-tight">Kiểm kê kho hàng</h2>
          <p className="text-sm text-slate-500 mt-1">Quản lý các phiên kiểm kê định kỳ hoặc đột xuất tại từng kho.</p>
        </div>
        <Button variant="primary" onClick={() => setShowModal(true)} disabled={tenantWarehouses.length === 0}>
          + Bắt đầu kiểm kê
        </Button>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <StatCard title="Tổng phiên kiểm kê" value={stats.total} />
        <StatCard title="Đang thực hiện" value={stats.inProgress} variant={stats.inProgress > 0 ? 'warning' : 'default'} />
        <StatCard title="Tổng chênh lệch" value={stats.discrepancies} variant={stats.discrepancies > 0 ? 'warning' : 'success'} />
      </div>

      {error && <Alert variant="error" message={error} />}

      <div className="bg-white border border-slate-200 shadow-sm rounded-2xl overflow-hidden">
        <div className="px-6 py-5 border-b border-slate-100">
          <h3 className="font-normal text-slate-900 text-base">Lịch sử kiểm kê</h3>
        </div>
        {stocktakes.length === 0 ? (
          <p className="px-6 py-12 text-center text-slate-400 text-sm">Chưa có phiên kiểm kê nào.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm text-left">
              <thead className="bg-slate-50/75 border-b border-slate-100 text-xs text-slate-500 uppercase">
                <tr>
                  <th className="px-6 py-4">Mã phiên</th>
                  <th className="px-6 py-4">Kho</th>
                  <th className="px-6 py-4">Người kiểm</th>
                  <th className="px-6 py-4 text-center">Tổng SP</th>
                  <th className="px-6 py-4 text-center">Chênh lệch</th>
                  <th className="px-6 py-4">Trạng thái</th>
                  <th className="px-6 py-4">Ngày</th>
                  <th className="px-6 py-4 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {stocktakes.map((s) => (
                  <tr key={s.id} className="hover:bg-slate-50/80">
                    <td className="px-6 py-4 font-mono text-xs">{s.stocktakeCode}</td>
                    <td className="px-6 py-4">{s.warehouseName}</td>
                    <td className="px-6 py-4">{s.auditor}</td>
                    <td className="px-6 py-4 text-center">{s.totalItems}</td>
                    <td className="px-6 py-4 text-center">{s.discrepancyCount}</td>
                    <td className="px-6 py-4">{stocktakeStatusLabel(s.status)}</td>
                    <td className="px-6 py-4 text-slate-500 text-xs">{s.createdAt.slice(0, 10)}</td>
                    <td className="px-6 py-4 text-right">
                      {s.status === 'InProgress' && (
                        <Button variant="primary" size="sm" onClick={() => handleComplete(s.id)}>Hoàn thành</Button>
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
        title="Bắt đầu kiểm kê mới"
        footer={<ModalFooterButtons onCancel={() => setShowModal(false)} submitLabel="Bắt đầu kiểm kê" loading={submitting} submitForm="stocktake-form" />}
      >
        {formError && <div className="mb-4"><Alert variant="error" message={formError} /></div>}
        <form id="stocktake-form" onSubmit={handleSubmit} className="space-y-4">
          <FormField label="Kho kiểm kê" required>
            <SelectInput value={form.warehouseId || tenantWarehouses[0]?.id} onChange={(e) => setForm({ ...form, warehouseId: e.target.value })} required>
              {tenantWarehouses.map((w) => (
                <option key={w.id} value={w.id}>{w.name}</option>
              ))}
            </SelectInput>
          </FormField>
          <FormField label="Người phụ trách kiểm kê" required>
            <TextInput value={form.auditor} onChange={(e) => setForm({ ...form, auditor: e.target.value })} required placeholder="Nhập tên người kiểm..." />
          </FormField>
        </form>
      </Modal>

      <Modal
        open={showCompleteModal}
        onClose={() => !submitting && setShowCompleteModal(false)}
        title="Hoàn thành kiểm kê - Nhập chi tiết chênh lệch"
        size="xl"
        footer={<ModalFooterButtons onCancel={() => setShowCompleteModal(false)} submitLabel="Hoàn thành kiểm kê" loading={submitting} submitForm="complete-form" />}
      >
        {formError && <div className="mb-4"><Alert variant="error" message={formError} /></div>}
        <form id="complete-form" onSubmit={handleCompleteSubmit} className="space-y-4">
          <div className="mb-4">
            <p className="text-sm text-slate-600">Nhập chi tiết các sản phẩm có chênh lệch giữa số lượng thực tế và hệ thống.</p>
          </div>

          <div className="space-y-3 max-h-96 overflow-y-auto">
            {discrepancies.map((d, index) => (
              <div key={index} className="border border-slate-200 rounded-lg p-4 space-y-3">
                <div className="flex justify-between items-start">
                  <h4 className="font-medium text-sm text-slate-900">Sản phẩm {index + 1}</h4>
                  <Button variant="ghost" size="sm" onClick={() => removeDiscrepancy(index)}>Xóa</Button>
                </div>
                <div className="grid grid-cols-2 gap-3">
                  <FormField label="Mã SKU">
                    <TextInput value={d.productSku} onChange={(e) => updateDiscrepancy(index, 'productSku', e.target.value)} placeholder="VD: SKU-001" />
                  </FormField>
                  <FormField label="Tên sản phẩm">
                    <TextInput value={d.productName} onChange={(e) => updateDiscrepancy(index, 'productName', e.target.value)} placeholder="Tên sản phẩm" />
                  </FormField>
                </div>
                <div className="grid grid-cols-3 gap-3">
                  <FormField label="SL hệ thống">
                    <TextInput type="number" min={0} value={d.systemQuantity || ''} onChange={(e) => updateDiscrepancy(index, 'systemQuantity', Number(e.target.value))} />
                  </FormField>
                  <FormField label="SL thực tế">
                    <TextInput type="number" min={0} value={d.actualQuantity || ''} onChange={(e) => updateDiscrepancy(index, 'actualQuantity', Number(e.target.value))} />
                  </FormField>
                  <FormField label="Chênh lệch">
                    <TextInput type="number" value={d.discrepancy || ''} readOnly className="bg-slate-50" />
                  </FormField>
                </div>
                <FormField label="Lý do chênh lệch">
                  <TextInput value={d.reason} onChange={(e) => updateDiscrepancy(index, 'reason', e.target.value)} placeholder="VD: Hư hỏng, thất lạc, nhập sai..." />
                </FormField>
              </div>
            ))}
          </div>

          <Button variant="secondary" onClick={addDiscrepancy} className="w-full">+ Thêm sản phẩm chênh lệch</Button>

          <FormField label="Ghi chú chung">
            <TextInput value={note} onChange={(e) => setNote(e.target.value)} placeholder="Ghi chú thêm về phiên kiểm kê..." />
          </FormField>
        </form>
      </Modal>
    </div>
  )
}
