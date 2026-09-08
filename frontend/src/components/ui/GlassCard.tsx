import type { ReactNode } from 'react'

interface GlassCardProps {
  children: ReactNode
  className?: string
  strong?: boolean
  hover?: boolean
}

export default function GlassCard({
  children,
  className = '',
  strong = false,
  hover = false,
}: GlassCardProps) {
  return (
    <div
      className={`rounded-2xl ${strong ? 'glass-strong' : 'glass'} ${
        hover ? 'transition-all duration-300 hover:border-white/25 hover:shadow-teal-500/10' : ''
      } ${className}`}
    >
      {children}
    </div>
  )
}
