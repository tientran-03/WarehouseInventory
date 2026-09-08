import { useState } from 'react'
import type { FormEvent } from 'react'
import { useNavigate } from 'react-router-dom'
import Alert from '../../components/ui/Alert'
import Button from '../../components/ui/Button'
import { ROUTES } from '../../constants/routes'
import { useAuth } from '../../context/AuthContext'
import { parseApiError } from '../../utils/errorHandler'

export default function ChangePasswordPage() {
  const navigate = useNavigate()
  const { changePassword, isLoading } = useAuth()
  const [currentPassword, setCurrentPassword] = useState('')
  const [newPassword, setNewPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')
  const [error, setError] = useState<string | null>(null)

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault()
    setError(null)

    if (newPassword.length < 8) {
      setError('Mật khẩu mới cần tối thiểu 8 ký tự.')
      return
    }
    if (newPassword !== confirmPassword) {
      setError('Xác nhận mật khẩu chưa khớp.')
      return
    }

    try {
      await changePassword({ currentPassword, newPassword })
      navigate(ROUTES.DASHBOARD, { replace: true })
    } catch (err) {
      setError(parseApiError(err, 'Không thể đổi mật khẩu.').message)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 px-4 text-slate-800">
      <div className="w-full max-w-md bg-white border border-slate-200 shadow-sm rounded-3xl p-8 space-y-6">
        <div className="text-center space-y-2">
          <div className="mx-auto w-14 h-14 rounded-full bg-amber-100 text-amber-700 flex items-center justify-center text-xl">⌁</div>
          <h1 className="text-xl font-semibold text-slate-900">Đổi mật khẩu bắt buộc</h1>
          <p className="text-sm text-slate-500">Để bảo vệ tài khoản công ty, hãy thay mật khẩu tạm thời trước khi sử dụng hệ thống.</p>
        </div>

        {error && <Alert variant="error" message={error} onClose={() => setError(null)} />}

        <form onSubmit={handleSubmit} className="space-y-4">
          <PasswordField label="Mật khẩu tạm thời" value={currentPassword} onChange={setCurrentPassword} autoComplete="current-password" />
          <PasswordField label="Mật khẩu mới" value={newPassword} onChange={setNewPassword} autoComplete="new-password" hint="Tối thiểu 8 ký tự." />
          <PasswordField label="Xác nhận mật khẩu mới" value={confirmPassword} onChange={setConfirmPassword} autoComplete="new-password" />
          <Button type="submit" variant="primary" fullWidth disabled={isLoading} className="py-3">
            {isLoading ? 'Đang cập nhật...' : 'Lưu mật khẩu mới'}
          </Button>
        </form>
      </div>
    </div>
  )
}

function PasswordField({ label, value, onChange, autoComplete, hint }: {
  label: string
  value: string
  onChange: (value: string) => void
  autoComplete: string
  hint?: string
}) {
  return (
    <div className="space-y-1">
      <label className="block text-xs font-medium text-slate-600">{label}</label>
      <input
        type="password"
        value={value}
        onChange={(event) => onChange(event.target.value)}
        required
        autoComplete={autoComplete}
        className="w-full border border-slate-300 rounded-xl px-4 py-3 text-sm focus:outline-none focus:ring-1 focus:ring-indigo-500 bg-slate-50/50 transition-all text-slate-800"
      />
      {hint && <p className="text-xs text-slate-400">{hint}</p>}
    </div>
  )
}
