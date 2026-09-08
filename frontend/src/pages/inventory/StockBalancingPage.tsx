import Alert from '../../components/ui/Alert'
import Button from '../../components/ui/Button'
import LoadingState from '../../components/ui/LoadingState'
import { useTenantContext } from '../../context/TenantContext'
import { useAnalytics } from '../../hooks/useAnalytics'
import { useTenantWarehouses } from '../../hooks/useTenantWarehouses'
import { useInventory } from '../../hooks/useInventory'

export default function StockBalancingPage() {
  const { activeTenantId } = useTenantContext()
  const { tenantWarehouses, loading: whLoading } = useTenantWarehouses()
  const { items: inventory, loading: invLoading } = useInventory(activeTenantId)
  const { balancing, loading: balLoading, error } = useAnalytics(activeTenantId)

  if (!activeTenantId) {
    return <Alert variant="warning" message="Chọn tenant ở sidebar để xem cân đối tồn." />
  }

  if (whLoading || invLoading || balLoading) {
    return <LoadingState message="Đang phân tích cân đối tồn kho..." />
  }

  return (
    <div className="space-y-6">
      <div className="bg-white p-6 border border-slate-200 shadow-sm rounded-2xl">
        <h2 className="text-xl font-normal text-slate-900 tracking-tight">Cân đối tồn kho đa chi nhánh</h2>
        <p className="text-sm text-slate-500 mt-1">
          Phân tích chênh lệch tồn kho giữa các kho và đề xuất lệnh điều chuyển tự động.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        {tenantWarehouses.map((wh) => {
          const whItems = inventory.filter((i) => i.warehouseId === wh.id)
          const total = whItems.reduce((sum, i) => sum + i.onHandStock, 0)
          return (
            <div key={wh.id} className="bg-white border border-slate-200 p-5 shadow-sm rounded-xl space-y-3">
              <div className="flex items-center justify-between">
                <h3 className="font-medium text-slate-900">{wh.name}</h3>
                <span className="text-xs text-slate-500">{whItems.length} dòng</span>
              </div>
              <p className="text-2xl font-semibold text-slate-800">{total}</p>
              <p className="text-xs text-slate-500">Tổng đơn vị tồn</p>
            </div>
          )
        })}
      </div>

      {error && <Alert variant="error" message={error} />}

      <div className="bg-white border border-slate-200 shadow-sm rounded-2xl overflow-hidden">
        <div className="px-6 py-5 border-b border-slate-100">
          <h3 className="font-normal text-slate-900 text-base">Đề xuất cân bằng tự động</h3>
        </div>
        {balancing.length === 0 ? (
          <p className="px-6 py-12 text-center text-slate-400 text-sm">Không có chênh lệch đáng kể giữa các kho.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm text-left">
              <thead className="bg-slate-50/75 border-b border-slate-100 text-xs text-slate-500 uppercase">
                <tr>
                  <th className="px-6 py-4">Sản phẩm</th>
                  <th className="px-6 py-4">Kho dư</th>
                  <th className="px-6 py-4">Kho thiếu</th>
                  <th className="px-6 py-4 text-center">Chênh lệch</th>
                  <th className="px-6 py-4 text-center">SL gợi ý</th>
                  <th className="px-6 py-4 text-right">Hành động</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {balancing.map((s) => (
                  <tr key={s.productId} className="hover:bg-slate-50/80 transition-colors">
                    <td className="px-6 py-4">
                      <div className="font-normal text-slate-900">{s.productName}</div>
                      <div className="text-xs text-slate-500 font-mono">{s.productSku}</div>
                    </td>
                    <td className="px-6 py-4 text-slate-600">{s.fromWarehouseName}</td>
                    <td className="px-6 py-4 text-slate-600">{s.toWarehouseName}</td>
                    <td className="px-6 py-4 text-center font-medium">{s.imbalance}</td>
                    <td className="px-6 py-4 text-center">{s.suggestedQty}</td>
                    <td className="px-6 py-4 text-right">
                      <Button variant="primary" size="sm">Tạo lệnh tự động</Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  )
}
