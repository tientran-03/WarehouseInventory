import { useAuth } from '../../context/AuthContext'

export default function AccountManagementPage() {
  const { user } = useAuth()

  if (!user) return null

  return (
    <div className="space-y-6 max-w-4xl mx-auto">
      <div className="bg-white p-6 border border-slate-200 shadow-sm rounded-2xl flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h2 className="text-xl font-normal text-slate-900 tracking-tight">Quản lý tài khoản</h2>
          <p className="text-sm text-slate-500 mt-1">Thông tin từ phiên đăng nhập API.</p>
        </div>
        <span className="inline-flex items-center px-3 py-1.5 text-xs bg-indigo-50 text-indigo-700 border border-indigo-100 rounded-full w-max">
          {user.role}
        </span>
      </div>

      <div className="bg-white border border-slate-200 shadow-sm rounded-2xl p-6 space-y-4">
        <h3 className="text-base text-slate-900 border-b border-slate-100 pb-3">Thông tin phiên đăng nhập</h3>
        <div className="grid grid-cols-1 md:grid-cols-2 gap-4 text-sm">
          <div>
            <p className="text-slate-500 text-xs mb-1">Tên đăng nhập</p>
            <p className="text-slate-900">{user.username}</p>
          </div>
          <div>
            <p className="text-slate-500 text-xs mb-1">Vai trò</p>
            <p className="text-slate-900">{user.role}</p>
          </div>
          <div>
            <p className="text-slate-500 text-xs mb-1">Hết hạn token</p>
            <p className="text-slate-900">{new Date(user.expiration).toLocaleString('vi-VN')}</p>
          </div>
        </div>
      </div>
    </div>
  )
}
