import { Modal as AntModal } from 'antd'
import type { ReactNode } from 'react'

type ModalTone = 'default' | 'warning' | 'danger' | 'success'

interface ModalProps {
  open: boolean
  onClose: () => void
  title: string
  description?: string
  children: ReactNode
  footer?: ReactNode
  size?: 'sm' | 'md' | 'lg' | 'xl'
  tone?: ModalTone
}

const widths: Record<NonNullable<ModalProps['size']>, number> = {
  sm: 420,
  md: 560,
  lg: 760,
  xl: 960,
}

const titleToneClass: Record<ModalTone, string> = {
  default: '',
  warning: 'text-amber-700',
  danger: 'text-red-700',
  success: 'text-emerald-700',
}

export default function Modal({
  open,
  onClose,
  title,
  description,
  children,
  footer,
  size = 'md',
  tone = 'default',
}: ModalProps) {
  return (
    <AntModal
      open={open}
      onCancel={onClose}
      title={
        <div>
          <span className={titleToneClass[tone]}>{title}</span>
          {description && <p className="mt-1 text-sm font-normal text-slate-500">{description}</p>}
        </div>
      }
      footer={footer}
      width={widths[size]}
      centered
      destroyOnHidden
    >
      {children}
    </AntModal>
  )
}
