import { useMemo } from 'react'
import { Link } from 'react-router-dom'
import ButtonLink from '../../components/ui/ButtonLink'
import LoadingState from '../../components/ui/LoadingState'
import StatCard from '../../components/StatCard'
import { ROUTES } from '../../constants/routes'
import { useTenantContext } from '../../context/TenantContext'
import { useInventory } from '../../hooks/useInventory'
import { useOrders } from '../../hooks/useOrders'
import { useProducts } from '../../hooks/useProducts'
import { useWarehouses } from '../../hooks/useWarehouses'

export default function DashboardPage() {
  const { activeTenantId } = useTenantContext()
  const { warehouses, loading: whLoading } = useWarehouses()
  const { products, loading: pLoading } = useProducts(activeTenantId)
  const { items: inventory, loading: invLoading } = useInventory(activeTenantId)
  const { orders, loading: ordLoading } = useOrders(activeTenantId)

  const tenantWarehouses = useMemo(
    () => warehouses.filter((w) => w.tenantId === activeTenantId && w.isActive),
    [warehouses, activeTenantId],
  )

  const lowStock = useMemo(
    () => inventory.filter((i) => i.availableStock <= i.reorderLevel),
    [inventory],
  )

  const splitOrders = useMemo(() => orders.filter((o) => o.isSplitOrder).length, [orders])

  if (whLoading || pLoading || invLoading || ordLoading) {
    return <LoadingState message="Đang tải dữ liệu tổng quan..." />
  }

  return (
    <div className="space-y-6">
      <div className="bg-white/60 backdrop-blur-md p-6 border border-white/70 shadow-sm rounded-xl flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h2 className="text-xl font-semibold text-slate-900 tracking-tight">Thống kê & Tổng quan</h2>

        </div>

      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
        <div className="border border-slate-200/80 rounded-xl shadow-sm bg-white overflow-hidden">
          <StatCard title="Kho hoạt động" value={tenantWarehouses.length} />
        </div>
        <div className="border border-slate-200/80 rounded-xl shadow-sm bg-white overflow-hidden">
          <StatCard title="Sản phẩm" value={products.length} />
        </div>
        <div className="border border-slate-200/80 rounded-xl shadow-sm bg-white overflow-hidden">
          <StatCard title="Dòng tồn kho" value={inventory.length} />
        </div>
        <div className="border border-slate-200/80 rounded-xl shadow-sm bg-white overflow-hidden">
          <StatCard title="Đơn hàng" value={orders.length} />
        </div>
        <div className="border border-slate-200/80 rounded-xl shadow-sm bg-white overflow-hidden">
          <StatCard title="Tồn thấp" value={lowStock.length} variant={lowStock.length > 0 ? 'warning' : 'success'} />
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
        <div className="lg:col-span-2 bg-white border border-slate-200 shadow-sm rounded-xl p-6 space-y-4">
          <div className="flex items-center justify-between">
            <h3 className="text-base font-medium text-slate-900">Đơn hàng gần đây</h3>
            <Link to={ROUTES.ORDERS} className="text-xs text-slate-500 hover:text-slate-800">Xem tất cả</Link>
          </div>
          {orders.length === 0 ? (
            <p className="text-sm text-slate-500">Chưa có đơn. Tạo qua trang Đơn hàng hoặc webhook.</p>
          ) : (
            <div className="space-y-2">
              {orders.slice(0, 5).map((o) => (
                <div key={o.id} className="flex items-center justify-between p-3 rounded-xl border border-slate-100 bg-slate-50/50 text-sm">
                  <div>
                    <span className="font-mono text-slate-700">{o.orderCode}</span>
                    <span className="text-slate-400 mx-2">·</span>
                    <span className="text-slate-600">{o.customerName}</span>
                  </div>
                  <div className="text-right">
                    <span className="text-xs text-slate-500">{o.subOrderCount} kho</span>
                    {o.isSplitOrder && <span className="ml-1 text-xs text-amber-600">split</span>}
                  </div>
                </div>
              ))}
            </div>
          )}
          {splitOrders > 0 && (
            <p className="text-xs text-amber-700">{splitOrders} đơn đã được chia nhiều kho (split order).</p>
          )}
        </div>

        <div className="bg-white border border-slate-200 shadow-sm rounded-xl p-6 space-y-3">
          <h3 className="text-base font-medium text-slate-900">Hành động nhanh</h3>
          <ButtonLink to={ROUTES.ORDERS} variant="primary" fullWidth className="justify-center">Đơn hàng & phân bổ</ButtonLink>
          <ButtonLink to={ROUTES.INVENTORY} variant="secondary" fullWidth className="justify-center">Quản lý tồn kho</ButtonLink>
          <ButtonLink to={ROUTES.PRODUCTS} variant="secondary" fullWidth className="justify-center">Quản lý sản phẩm</ButtonLink>
          <ButtonLink to={ROUTES.WAREHOUSES} variant="secondary" fullWidth className="justify-center">Quản lý kho</ButtonLink>
        </div>
      </div>

      {lowStock.length > 0 && (
        <div className="bg-white border border-amber-200/80 shadow-sm rounded-xl p-6">
          <h3 className="text-base font-medium text-slate-900 mb-3">Cảnh báo tồn thấp</h3>
          <ul className="text-sm text-slate-600 space-y-1">
            {lowStock.slice(0, 5).map((i) => (
              <li key={i.id}>
                {i.warehouseName} — {i.productSku}: còn {i.availableStock} (reorder {i.reorderLevel})
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  )
}