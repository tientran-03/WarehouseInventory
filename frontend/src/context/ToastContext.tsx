import { createContext, useContext, useMemo, type ReactNode } from 'react'
import { toast, ToastContainer, type TypeOptions } from 'react-toastify'
import 'react-toastify/dist/ReactToastify.css'

type ToastVariant = Extract<TypeOptions, 'error' | 'warning' | 'success' | 'info'>

interface ToastContextType {
  showError: (message: string, title?: string) => void
  showSuccess: (message: string, title?: string) => void
  showWarning: (message: string, title?: string) => void
  showInfo: (message: string, title?: string) => void
}

interface ToastMessageProps {
  message: string
  title?: string
}

const ToastContext = createContext<ToastContextType | undefined>(undefined)

function ToastMessage({ message, title }: ToastMessageProps) {
  return (
    <div>
      {title && <strong className="mb-1 block">{title}</strong>}
      <span>{message}</span>
    </div>
  )
}

function showToast(variant: ToastVariant, message: string, title?: string) {
  toast(<ToastMessage message={message} title={title} />, { type: variant })
}

export function ToastProvider({ children }: { children: ReactNode }) {
  const value = useMemo<ToastContextType>(
    () => ({
      showError: (message, title) => showToast('error', message, title),
      showSuccess: (message, title) => showToast('success', message, title),
      showWarning: (message, title) => showToast('warning', message, title),
      showInfo: (message, title) => showToast('info', message, title),
    }),
    [],
  )

  return (
    <ToastContext.Provider value={value}>
      {children}
      <ToastContainer
        position="top-right"
        autoClose={5000}
        closeOnClick
        pauseOnFocusLoss
        draggable
        pauseOnHover
        newestOnTop
        limit={4}
      />
    </ToastContext.Provider>
  )
}

export function useToast() {
  const context = useContext(ToastContext)
  if (!context) throw new Error('useToast must be used within ToastProvider')
  return context
}
