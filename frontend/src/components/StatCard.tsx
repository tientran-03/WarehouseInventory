interface StatCardProps {
  title: string
  value: string | number
  subtitle?: string
  variant?: 'default' | 'warning' | 'success'
}

const variantStyles = {
  default: 'bg-white border-slate-200',
  warning: 'bg-amber-50 border-amber-200',
  success: 'bg-emerald-50 border-emerald-200',
}

const valueStyles = {
  default: 'text-slate-900',
  warning: 'text-amber-900',
  success: 'text-emerald-900',
}

const subtitleStyles = {
  default: 'text-slate-500',
  warning: 'text-amber-700',
  success: 'text-emerald-700',
}

export default function StatCard({ title, value, subtitle, variant = 'default' }: StatCardProps) {
  return (
    <div className={`border p-6 shadow-sm transition-all ${variantStyles[variant]}`}>
      <p className="text-xs font-normal uppercase tracking-wider text-slate-500">{title}</p>
      <p className={`text-3xl font-normal mt-2 ${valueStyles[variant]}`}>{value}</p>
      {subtitle && <p className={`text-xs font-normal mt-2 ${subtitleStyles[variant]}`}>{subtitle}</p>}
    </div>
  )
}