import { Button as AntButton, type ButtonProps as AntButtonProps } from 'antd'
import type { ButtonHTMLAttributes } from 'react'

export type ButtonVariant = 'primary' | 'secondary' | 'ghost' | 'danger-ghost' | 'modal-primary'
export type ButtonSize = 'sm' | 'md' | 'icon'

interface ButtonProps extends Omit<AntButtonProps, 'type' | 'variant' | 'size' | 'htmlType' | 'block'> {
  variant?: ButtonVariant
  size?: ButtonSize
  fullWidth?: boolean
  type?: ButtonHTMLAttributes<HTMLButtonElement>['type']
}

const variantProps: Record<ButtonVariant, Pick<AntButtonProps, 'type' | 'danger'>> = {
  primary: { type: 'primary' },
  secondary: { type: 'default' },
  ghost: { type: 'text' },
  'danger-ghost': { type: 'text', danger: true },
  'modal-primary': { type: 'primary' },
}

const sizeProps: Record<ButtonSize, NonNullable<AntButtonProps['size']>> = {
  sm: 'small',
  md: 'medium',
  icon: 'small',
}

export function getAntButtonProps(
  variant: ButtonVariant = 'secondary',
  size: ButtonSize = 'md',
  fullWidth = false,
) {
  return {
    ...variantProps[variant],
    size: sizeProps[size],
    block: fullWidth,
  }
}

export default function Button({
  variant = 'secondary',
  size = 'md',
  fullWidth = false,
  type = 'button',
  children,
  ...props
}: ButtonProps) {
  return (
    <AntButton
      {...getAntButtonProps(variant, size, fullWidth)}
      htmlType={type}
      {...props}
    >
      {children}
    </AntButton>
  )
}
