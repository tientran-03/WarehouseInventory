import { BrowserRouter, Routes, Route } from 'react-router-dom'
import Layout from './components/Layout'
import ProtectedRoute from './components/ProtectedRoute'
import { AdminRoute, CompanyRoute } from './components/RoleRoute'
import { PasswordChangeCompletedRoute, PasswordChangeRequiredRoute } from './components/PasswordChangeRoute'
import LoginPage from './pages/auth/LoginPage'
import VerifyEmailPage from './pages/auth/VerifyEmailPage'
import ChangePasswordPage from './pages/auth/ChangePasswordPage'
import DashboardPage from './pages/hompage/DashboardPage'
import WarehousesPage from './pages/hompage/WarehousesPage'
import TenantsPage from './pages/hompage/TenantsPage'
import ProductsPage from './pages/hompage/ProductsPage'
import InventoryPage from './pages/hompage/InventoryPage'
import OrdersPage from './pages/hompage/OrdersPage'
import StocktakesPage from './pages/inventory/StocktakesPage'
import StockBalancingPage from './pages/inventory/StockBalancingPage'
import WarehouseZoningPage from './pages/inventory/WarehouseZoningPage'
import WarehouseFinancialsPage from './pages/inventory/WarehouseFinancialsPage'
import AccountManagementPage from './pages/auth/AccountManagementPage'
import WarehouseTransfersPage from './pages/inventory/WarehouseTransfersPage'
import StockDocumentsPage from './pages/inventory/StockDocumentsPage'
import { ROUTES } from './constants/routes'

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path={ROUTES.LOGIN} element={<LoginPage />} />
        <Route path={ROUTES.VERIFY_EMAIL} element={<VerifyEmailPage />} />
        <Route element={<ProtectedRoute />}>
          <Route element={<PasswordChangeRequiredRoute />}>
            <Route path={ROUTES.CHANGE_PASSWORD} element={<ChangePasswordPage />} />
          </Route>
          <Route element={<PasswordChangeCompletedRoute />}>
            <Route element={<Layout />}>
              <Route element={<AdminRoute />}>
                <Route path={ROUTES.TENANTS} element={<TenantsPage />} />
              </Route>
              <Route element={<CompanyRoute />}>
                <Route path={ROUTES.DASHBOARD} element={<DashboardPage />} />
                <Route path={ROUTES.WAREHOUSES} element={<WarehousesPage />} />
                <Route path={ROUTES.PRODUCTS} element={<ProductsPage />} />
                <Route path={ROUTES.INVENTORY} element={<InventoryPage />} />
                <Route path={ROUTES.STOCK_DOCUMENTS} element={<StockDocumentsPage />} />
                <Route path={ROUTES.ORDERS} element={<OrdersPage />} />
                <Route path={ROUTES.TRANSFERS} element={<WarehouseTransfersPage />} />
                <Route path={ROUTES.STOCKTAKES} element={<StocktakesPage />} />
                <Route path={ROUTES.BALANCING} element={<StockBalancingPage />} />
                <Route path={ROUTES.ZONING} element={<WarehouseZoningPage />} />
                <Route path={ROUTES.FINANCIALS} element={<WarehouseFinancialsPage />} />
              </Route>
              <Route path={ROUTES.ACCOUNT} element={<AccountManagementPage />} />
            </Route>
          </Route>
        </Route>
      </Routes>
    </BrowserRouter>
  )
}
