import { useEffect, useState } from 'react'
import { Link, useSearchParams } from 'react-router-dom'
import Alert from '../../components/ui/Alert'
import Button from '../../components/ui/Button'
import { ROUTES } from '../../constants/routes'
import { authService } from '../../services'
import { parseApiError } from '../../utils/errorHandler'

export default function VerifyEmailPage() {
  const [searchParams] = useSearchParams()
  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading')
  const [message, setMessage] = useState('Đang xác minh địa chỉ email của bạn...')

  useEffect(() => {
    const token = searchParams.get('token')
    if (!token) {
      setStatus('error')
      setMessage('Liên kết xác minh không hợp lệ.')
      return
    }

    void authService.verifyEmail(token)
      .then(() => {
        setStatus('success')
        setMessage('Email đã được xác minh. Bạn có thể đăng nhập bằng mật khẩu tạm thời.')
      })
      .catch((error: unknown) => {
        setStatus('error')
        setMessage(parseApiError(error, 'Không thể xác minh email.').message)
      })
  }, [searchParams])

  return (
    <div className="min-h-screen flex items-center justify-center bg-slate-50 px-4 text-slate-800">
      <div className="w-full max-w-md bg-white border border-slate-200 shadow-sm rounded-3xl p-8 space-y-6 text-center">
        <div className={`mx-auto w-14 h-14 rounded-full flex items-center justify-center ${status === 'success' ? 'bg-emerald-100 text-emerald-700' : status === 'error' ? 'bg-rose-100 text-rose-700' : 'bg-indigo-100 text-indigo-700'}`}>
          {status === 'success' ? '✓' : status === 'error' ? '!' : '…'}
        </div>
        <div className="space-y-2">
          <h1 className="text-xl font-semibold text-slate-900">Xác minh email</h1>
          <p className="text-sm text-slate-500">{message}</p>
        </div>
        {status === 'error' && <Alert variant="error" message={message} />}
        {status !== 'loading' && (
          <Link to={ROUTES.LOGIN}>
            <Button variant="primary" fullWidth>Đi đến đăng nhập</Button>
          </Link>
        )}
      </div>
    </div>
  )
}
