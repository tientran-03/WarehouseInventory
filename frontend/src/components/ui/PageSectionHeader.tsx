interface PageSectionHeaderProps {
  title: string
  subtitle?: string
  action?: React.ReactNode
  badge?: string
}

export default function PageSectionHeader({ title, subtitle, action, badge }: PageSectionHeaderProps) {
  return (
    <div className="bg-white/60 backdrop-blur-md p-6 border border-white/70 shadow-sm rounded-xl flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
      <div>
        <div className="flex items-center gap-3">
          <h2 className="text-xl font-semibold text-slate-900 tracking-tight">{title}</h2>
          {badge && (
            <span className="text-xs px-2.5 py-1 rounded-full bg-white/50 text-slate-600 border border-white/70 font-medium backdrop-blur-sm">
              {badge}
            </span>
          )}
        </div>
        {subtitle && <p className="text-sm text-slate-500 mt-1">{subtitle}</p>}
      </div>
      {action}
    </div>
  )
}
