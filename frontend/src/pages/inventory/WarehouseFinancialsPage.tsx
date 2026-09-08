import Alert from '../../components/ui/Alert'
import LoadingState from '../../components/ui/LoadingState'
import StatCard from '../../components/StatCard'
import { useTenantContext } from '../../context/TenantContext'
import { useAnalytics } from '../../hooks/useAnalytics'

export default function WarehouseFinancialsPage() {
  const { activeTenantId } = useTenantContext()
  const { financials, loading, error } = useAnalytics(activeTenantId)

  if (!activeTenantId) {
    return <Alert variant="warning" message="Chọn tenant ở sidebar để xem tài chính kho." />
  }

  if (loading) return <LoadingState message="Đang tính giá trị tồn kho..." />

  return (
    <div className="space-y-6">
      <div className="bg-white p-6 border border-slate-200 shadow-sm rounded-2xl">
        <h2 className="text-xl font-normal text-slate-900 tracking-tight">Tài chính & Giá trị tồn kho</h2>
        <p className="text-sm text-slate-500 mt-1">Báo cáo giá trị hàng tồn theo từng kho (số lượng × giá sản phẩm).</p>
      </div>

      {error && <Alert variant="error" message={error} />}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <StatCard title="Tổng giá trị tồn" value={`${(financials?.grandTotal ?? 0).toLocaleString('vi-VN')} ₫`} />
        <StatCard title="Số kho" value={financials?.warehouseCount ?? 0} />
        <StatCard title="Dòng tồn kho" value={financials?.inventoryLineCount ?? 0} />
      </div>

      <div className="bg-white border border-slate-200 shadow-sm rounded-2xl overflow-hidden">
        <div className="px-6 py-5 border-b border-slate-100">
          <h3 className="font-normal text-slate-900 text-base">Phân bổ giá trị theo kho</h3>
        </div>
        {!financials || financials.warehouses.length === 0 ? (
          <p className="px-6 py-12 text-center text-slate-400 text-sm">Chưa có dữ liệu tồn kho.</p>
        ) : (
          <div className="p-6 space-y-5">
            {financials.warehouses.map((f) => {
              const pct = financials.grandTotal > 0 ? (f.totalValue / financials.grandTotal) * 100 : 0
              return (
                <div key={f.warehouseId} className="space-y-1.5">
                  <div className="flex justify-between text-xs font-medium">
                    <span className="text-slate-700">{f.warehouseName}</span>
                    <span className="text-slate-500">{f.totalValue.toLocaleString('vi-VN')} ₫ · {pct.toFixed(1)}%</span>
                  </div>
                  <div className="h-2 bg-slate-100 rounded-full overflow-hidden">
                    <div className="h-full bg-indigo-400/80 rounded-full transition-all" style={{ width: `${pct}%` }} />
                  </div>
                  <p className="text-[11px] text-slate-400">{f.totalUnits} đơn vị · {f.lineCount} dòng tồn</p>
                </div>
              )
            })}
          </div>
        )}
      </div>
    </div>
  )
}
