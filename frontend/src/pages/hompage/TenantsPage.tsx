import { useState } from 'react'
import type { FormEvent } from 'react'
import Alert from '../../components/ui/Alert'
import Button from '../../components/ui/Button'
import ConfirmModal from '../../components/ui/ConfirmModal'
import { FormField, ModalFooterButtons, TextInput } from '../../components/ui/FormField'
import LoadingState from '../../components/ui/LoadingState'
import Modal from '../../components/ui/Modal'
import PageSectionHeader from '../../components/ui/PageSectionHeader'
import { useToast } from '../../context/ToastContext'
import { useTenants } from '../../hooks/useTenants'
import { parseApiError } from '../../utils/errorHandler'

const emptyForm = { name: '', code: '', accountEmail: '', accountPassword: '', confirmPassword: '' }

export default function TenantsPage() {
  const { showError, showSuccess } = useToast()
  const { tenants, loading, error, createTenant, deleteTenant } = useTenants()
  const [modalOpen, setModalOpen] = useState(false)
  const [confirmOpen, setConfirmOpen] = useState(false)
  const [deleteTarget, setDeleteTarget] = useState<{ id: string; name: string } | null>(null)
  const [form, setForm] = useState(emptyForm)
  const [submitting, setSubmitting] = useState(false)
  const [deleting, setDeleting] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)

  const closeModal = () => {
    if (submitting) return
    setModalOpen(false)
    setForm(emptyForm)
    setFormError(null)
  }

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault()
    if (form.accountPassword !== form.confirmPassword) {
      setFormError('Xác nhận mật khẩu tạm thời chưa khớp.')
      return
    }
    setSubmitting(true)
    setFormError(null)
    try {
      const payload = {
        name: form.name,
        code: form.code,
        accountEmail: form.accountEmail,
        accountPassword: form.accountPassword,
      }
      await createTenant(payload)
      showSuccess('Đã tạo công ty và gửi email xác minh.')
      closeModal()
    } catch (err) {
      const parsed = parseApiError(err)
      setFormError(parsed.message)
      showError(parsed.message, 'Không thể tạo tenant')
    } finally {
      setSubmitting(false)
    }
  }

  const handleDeleteConfirm = async () => {
    if (!deleteTarget) return
    setDeleting(true)
    try {
      await deleteTenant(deleteTarget.id)
      showSuccess('Đã vô hiệu hóa tenant')
      setConfirmOpen(false)
      setDeleteTarget(null)
    } catch (err) {
      showError(parseApiError(err).message, 'Không thể xóa tenant')
    } finally {
      setDeleting(false)
    }
  }

  if (loading) {
    return <LoadingState message="Đang tải danh sách tenant..." />
  }

  return (
    <div className="space-y-6">
      <PageSectionHeader
        title="Quản lý Tenant"
        subtitle="Tạo và quản lý các tenant trong hệ thống đa kho."
        badge={`${tenants.length} tenant`}
        action={
          <Button variant="primary" onClick={() => setModalOpen(true)}>
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
            </svg>
            Thêm tenant
          </Button>
        }
      />

      {error && !modalOpen && (
        <Alert variant="error" title="Lỗi tải dữ liệu" message={error} />
      )}

      <div className="bg-white border border-slate-200 shadow-sm overflow-hidden rounded-xl">
        <table className="w-full text-sm text-left">
          <thead className="bg-slate-50/75 border-b border-slate-100 text-xs text-slate-500 uppercase tracking-wide">
            <tr>
              <th className="px-6 py-4 font-medium">Mã</th>
              <th className="px-6 py-4 font-medium">Tên</th>
              <th className="px-6 py-4 font-medium">Tài khoản công ty</th>
              <th className="px-6 py-4 font-medium">Xác minh email</th>
              <th className="px-6 py-4 font-medium">Trạng thái</th>
              <th className="px-6 py-4 font-medium text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {tenants.length === 0 ? (
              <tr>
                <td colSpan={6} className="px-6 py-12 text-center text-slate-400 text-sm">
                  Chưa có tenant nào. Nhấn &quot;Thêm tenant&quot; để bắt đầu.
                </td>
              </tr>
            ) : (
              tenants.map((t) => {
                const account = t.accounts?.[0]
                return (
                <tr key={t.id} className="hover:bg-slate-50/80 transition-colors">
                  <td className="px-6 py-4 font-mono text-sm text-slate-600">{t.code}</td>
                  <td className="px-6 py-4 font-medium text-slate-900">{t.name}</td>
                  <td className="px-6 py-4 text-slate-600">{account?.email ?? '—'}</td>
                  <td className="px-6 py-4">
                    {account ? (
                      <span className={`inline-flex px-2.5 py-1 rounded-full text-xs font-medium border ${account.isEmailVerified ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-amber-50 text-amber-700 border-amber-200'}`}>
                        {account.isEmailVerified ? 'Đã xác minh' : 'Chờ xác minh'}
                      </span>
                    ) : '—'}
                  </td>
                  <td className="px-6 py-4">
                    <span className={`inline-flex px-2.5 py-1 rounded-full text-xs font-medium border ${t.isActive ? 'bg-emerald-50 text-emerald-700 border-emerald-200' : 'bg-slate-50 text-slate-600 border-slate-200'}`}>
                      {t.isActive ? 'Hoạt động' : 'Ngưng'}
                    </span>
                  </td>
                  <td className="px-6 py-4 text-right">
                    {t.isActive && (
                      <Button
                        variant="danger-ghost"
                        size="sm"
                        onClick={() => {
                          setDeleteTarget({ id: t.id, name: t.name })
                          setConfirmOpen(true)
                        }}
                      >
                        Vô hiệu hóa
                      </Button>
                    )}
                  </td>
                </tr>
                )
              })
            )}
          </tbody>
        </table>
      </div>

      <Modal
        open={modalOpen}
        onClose={closeModal}
        title="Thêm công ty và tài khoản"
        description="Hệ thống gửi liên kết xác minh đến email công ty. Tài khoản sẽ buộc đổi mật khẩu tạm thời sau khi xác minh."
        footer={
          <ModalFooterButtons
            onCancel={closeModal}
            submitLabel="Tạo tenant"
            loading={submitting}
            submitForm="create-tenant-form"
          />
        }
      >
        {formError && (
          <div className="mb-4">
            <Alert variant="error" message={formError} onClose={() => setFormError(null)} />
          </div>
        )}
        <form id="create-tenant-form" onSubmit={handleSubmit} className="space-y-4">
          <FormField label="Tên tenant" required hint="VD: Công ty ABC">
            <TextInput
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
              placeholder="Nhập tên tenant..."
              required
              autoFocus
            />
          </FormField>
          <FormField label="Mã tenant" required hint="VD: TENANT-001, viết hoa không dấu cách">
            <TextInput
              value={form.code}
              onChange={(e) => setForm({ ...form, code: e.target.value.toUpperCase() })}
              placeholder="TENANT-001"
              required
            />
          </FormField>
          <FormField label="Email tài khoản công ty" required hint="Email này cũng dùng để đăng nhập và nhận liên kết xác minh.">
            <TextInput
              type="email"
              value={form.accountEmail}
              onChange={(e) => setForm({ ...form, accountEmail: e.target.value })}
              placeholder="contact@company.com"
              required
            />
          </FormField>
          <FormField label="Mật khẩu tạm thời" required hint="Tối thiểu 8 ký tự. Công ty phải đổi mật khẩu sau khi đăng nhập.">
            <TextInput
              type="password"
              value={form.accountPassword}
              onChange={(e) => setForm({ ...form, accountPassword: e.target.value })}
              autoComplete="new-password"
              minLength={8}
              required
            />
          </FormField>
          <FormField label="Xác nhận mật khẩu tạm thời" required>
            <TextInput
              type="password"
              value={form.confirmPassword}
              onChange={(e) => setForm({ ...form, confirmPassword: e.target.value })}
              autoComplete="new-password"
              minLength={8}
              required
            />
          </FormField>
        </form>
      </Modal>

      <ConfirmModal
        open={confirmOpen}
        onClose={() => {
          if (!deleting) {
            setConfirmOpen(false)
            setDeleteTarget(null)
          }
        }}
        onConfirm={handleDeleteConfirm}
        title="Vô hiệu hóa tenant?"
        message={`Bạn có chắc muốn vô hiệu hóa tenant "${deleteTarget?.name}"? Hành động này có thể ảnh hưởng đến kho và dữ liệu liên quan.`}
        confirmLabel="Vô hiệu hóa"
        loading={deleting}
      />
    </div>
  )
}
