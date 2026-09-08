interface PageHeaderProps {
  title: string
  subtitle?: string
}

export default function PageHeader({ title, subtitle }: PageHeaderProps) {
  return (
    <div className="mb-8 animate-fade-in-up">
      <h2 className="text-3xl font-bold text-white tracking-tight">{title}</h2>
      {subtitle && (
        <p className="text-slate-400 mt-1.5 text-sm">{subtitle}</p>
      )}
    </div>
  )
}
