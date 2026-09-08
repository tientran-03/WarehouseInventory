import { Alert as AntAlert } from 'antd'

type AlertVariant = 'error' | 'warning' | 'success' | 'info'

interface AlertProps {
  variant?: AlertVariant
  title?: string
  message: string
  onClose?: () => void
}

export default function Alert({ variant = 'error', title, message, onClose }: AlertProps) {
  return (
    <AntAlert
      type={variant}
      title={title ?? message}
      description={title ? message : undefined}
      showIcon
      closable={Boolean(onClose)}
      onClose={onClose}
    />
  )
}
