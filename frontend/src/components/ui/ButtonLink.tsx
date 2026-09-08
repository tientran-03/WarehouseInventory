import { Button as AntButton } from 'antd'
import type { MouseEvent } from 'react'
import { type LinkProps, useLinkClickHandler } from 'react-router-dom'
import { getAntButtonProps, type ButtonSize, type ButtonVariant } from './Button'

interface ButtonLinkProps extends LinkProps {
  variant?: ButtonVariant
  size?: ButtonSize
  fullWidth?: boolean
}

export default function ButtonLink({
  variant = 'primary',
  size = 'md',
  fullWidth = false,
  className,
  children,
  onClick,
  to,
  target,
  replace,
  state,
  preventScrollReset,
  relative,
  viewTransition,
}: ButtonLinkProps) {
  const handleNavigation = useLinkClickHandler<HTMLButtonElement>(to, {
    target,
    replace,
    state,
    preventScrollReset,
    relative,
    viewTransition,
  })

  const handleClick = (event: MouseEvent<HTMLButtonElement>) => {
    onClick?.(event as unknown as MouseEvent<HTMLAnchorElement>)
    if (!event.defaultPrevented) handleNavigation(event)
  }

  return (
    <AntButton
      {...getAntButtonProps(variant, size, fullWidth)}
      className={className}
      onClick={handleClick}
    >
      {children}
    </AntButton>
  )
}
