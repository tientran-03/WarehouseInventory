import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { ConfigProvider } from 'antd'
import './index.css'
import App from './App.tsx'
import { AuthProvider } from './context/AuthContext'
import { TenantProvider } from './context/TenantContext'
import { ToastProvider } from './context/ToastContext'

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <ConfigProvider
      theme={{
        token: {
          colorPrimary: '#2563eb',
          borderRadius: 12,
          fontFamily: 'Inter, ui-sans-serif, system-ui, sans-serif',
        },
      }}
    >
      <AuthProvider>
        <TenantProvider>
          <ToastProvider>
            <App />
          </ToastProvider>
        </TenantProvider>
      </AuthProvider>
    </ConfigProvider>
  </StrictMode>,
)
