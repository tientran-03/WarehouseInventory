import { useState } from "react";
import type { FormEvent } from "react";
import Alert from "../../components/ui/Alert";
import Button from "../../components/ui/Button";
import ConfirmModal from "../../components/ui/ConfirmModal";
import {
  FormField,
  ModalFooterButtons,
  SelectInput,
  TextInput,
} from "../../components/ui/FormField";
import LoadingState from "../../components/ui/LoadingState";
import Modal from "../../components/ui/Modal";
import PageSectionHeader from "../../components/ui/PageSectionHeader";
import { useAuth } from "../../context/AuthContext";
import { useToast } from "../../context/ToastContext";
import { useTenants } from "../../hooks/useTenants";
import { useWarehouses } from "../../hooks/useWarehouses";
import { parseApiError } from "../../utils/errorHandler";
import { isPlatformAdmin } from "../../utils/roles";
import type { Warehouse } from "../../types/warehouse.types";

const emptyForm = {
  tenantId: "",
  name: "",
  code: "",
  address: "",
  city: "",
  latitude: 0,
  longitude: 0,
};

const fieldLabels: Record<"name" | "code" | "address" | "city", string> = {
  name: "Tên kho",
  code: "Mã kho",
  address: "Địa chỉ",
  city: "Thành phố",
};

export default function WarehousesPage() {
  const { user } = useAuth();
  const { showError, showSuccess } = useToast();
  const isAdmin = isPlatformAdmin(user?.role);
  const companyTenantId = user?.tenantId ?? null;
  const { tenants, loading: tenantsLoading } = useTenants(isAdmin);
  const {
    warehouses,
    loading,
    error,
    createWarehouse,
    updateWarehouse,
    deleteWarehouse,
  } = useWarehouses();
  const visibleWarehouses = isAdmin
    ? warehouses
    : warehouses.filter((warehouse) => warehouse.tenantId === companyTenantId);
  const [modalOpen, setModalOpen] = useState(false);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<{
    id: string;
    name: string;
  } | null>(null);
  const [form, setForm] = useState(emptyForm);
  const [submitting, setSubmitting] = useState(false);
  const [deleting, setDeleting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [editingWarehouse, setEditingWarehouse] = useState<Warehouse | null>(
    null,
  );

  const closeModal = () => {
    if (submitting) return;
    setModalOpen(false);
    setForm(emptyForm);
    setFormError(null);
    setEditingWarehouse(null);
  };

  const openCreateModal = () => {
    setForm({
      ...emptyForm,
      tenantId: isAdmin ? "" : (companyTenantId ?? ""),
    });
    setFormError(null);
    setModalOpen(true);
  };

  const openEditModal = (warehouse: Warehouse) => {
    setForm({
      tenantId: warehouse.tenantId,
      name: warehouse.name,
      code: warehouse.code,
      address: warehouse.address,
      city: warehouse.city,
      latitude: warehouse.latitude,
      longitude: warehouse.longitude,
    });
    setEditingWarehouse(warehouse);
    setFormError(null);
    setModalOpen(true);
  };

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    const tenantId = isAdmin ? form.tenantId : companyTenantId;
    if (!tenantId) {
      setFormError(
        isAdmin
          ? "Vui lòng chọn công ty trước khi tạo kho."
          : "Tài khoản công ty này chưa được gắn với công ty. Vui lòng liên hệ quản trị viên.",
      );
      return;
    }

    setSubmitting(true);
    setFormError(null);
    try {
      const payload = {
        tenantId,
        code: form.code,
        name: form.name,
        address: form.address,
        city: form.city,
        latitude: Number(form.latitude),
        longitude: Number(form.longitude),
      };
      if (editingWarehouse) {
        await updateWarehouse(editingWarehouse.id, payload);
        showSuccess("Cập nhật kho hàng thành công!");
      } else {
        await createWarehouse(payload);
        showSuccess("Tạo kho hàng thành công!");
      }
      setModalOpen(false);
      setForm(emptyForm);
      setFormError(null);
      setEditingWarehouse(null);
    } catch (err) {
      const parsed = parseApiError(err);
      setFormError(parsed.message);
      showError(
        parsed.message,
        editingWarehouse ? "Không thể cập nhật kho" : "Không thể tạo kho",
      );
    } finally {
      setSubmitting(false);
    }
  };

  const handleDeleteConfirm = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await deleteWarehouse(deleteTarget.id);
      showSuccess("Đã vô hiệu hóa kho hàng");
      setConfirmOpen(false);
      setDeleteTarget(null);
    } catch (err) {
      showError(parseApiError(err).message, "Không thể xóa kho");
    } finally {
      setDeleting(false);
    }
  };

  if (loading || (isAdmin && tenantsLoading)) {
    return <LoadingState message="Đang tải danh sách kho..." />;
  }

  return (
    <div className="space-y-6">
      <PageSectionHeader
        title="Quản lý Kho hàng"
        badge={`${visibleWarehouses.length} kho`}
        action={
          <Button variant="primary" onClick={openCreateModal}>
            <svg
              className="w-4 h-4"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
              strokeWidth={2}
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                d="M12 4.5v15m7.5-7.5h-15"
              />
            </svg>
            Thêm kho hàng
          </Button>
        }
      />

      {error && !modalOpen && (
        <Alert variant="error" title="Lỗi tải dữ liệu" message={error} />
      )}

      <div className="bg-white border border-slate-200 shadow-sm overflow-hidden rounded-xl">
        <div className="overflow-x-auto">
          <table className="w-full text-sm text-left">
            <thead className="bg-slate-50/75 border-b border-slate-100 text-xs text-slate-500 uppercase tracking-wide">
              <tr>
                <th className="px-6 py-4 font-medium">Mã kho</th>
                <th className="px-6 py-4 font-medium">Tên kho</th>
                <th className="px-6 py-4 font-medium">Thành phố</th>
                <th className="px-6 py-4 font-medium">Tọa độ</th>
                <th className="px-6 py-4 font-medium">Trạng thái</th>
                <th className="px-6 py-4 font-medium text-right">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {visibleWarehouses.length === 0 ? (
                <tr>
                  <td
                    colSpan={6}
                    className="px-6 py-12 text-center text-slate-400 text-sm"
                  >
                    Chưa có kho nào. Nhấn &quot;Thêm kho hàng&quot; để bắt đầu.
                  </td>
                </tr>
              ) : (
                visibleWarehouses.map((wh) => (
                  <tr
                    key={wh.id}
                    className="hover:bg-slate-50/80 transition-colors"
                  >
                    <td className="px-6 py-4 font-mono text-sm text-slate-600">
                      {wh.code}
                    </td>
                    <td className="px-6 py-4 font-medium text-slate-900">
                      {wh.name}
                    </td>
                    <td className="px-6 py-4 text-slate-600">{wh.city}</td>
                    <td className="px-6 py-4 text-xs text-slate-500 font-mono">
                      {wh.latitude}, {wh.longitude}
                    </td>
                    <td className="px-6 py-4">
                      <span
                        className={`inline-flex px-2.5 py-1 rounded-full text-xs font-medium border ${wh.isActive ? "bg-emerald-50 text-emerald-700 border-emerald-200" : "bg-slate-50 text-slate-600 border-slate-200"}`}
                      >
                        {wh.isActive ? "Hoạt động" : "Tạm ngưng"}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-right">
                      {wh.isActive && (
                        <div className="flex justify-end gap-1">
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => openEditModal(wh)}
                          >
                            Sửa
                          </Button>
                          <Button
                            variant="danger-ghost"
                            size="sm"
                            onClick={() => {
                              setDeleteTarget({ id: wh.id, name: wh.name });
                              setConfirmOpen(true);
                            }}
                          >
                            Vô hiệu hóa
                          </Button>
                        </div>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      <Modal
        open={modalOpen}
        onClose={closeModal}
        title={editingWarehouse ? "Chỉnh sửa kho hàng" : "Thêm kho hàng mới"}
        size="lg"
        footer={
          <ModalFooterButtons
            onCancel={closeModal}
            submitLabel={editingWarehouse ? "Lưu thay đổi" : "Tạo kho hàng"}
            loading={submitting}
            submitForm="create-warehouse-form"
          />
        }
      >
        {formError && (
          <div className="mb-4">
            <Alert
              variant="error"
              message={formError}
              onClose={() => setFormError(null)}
            />
          </div>
        )}
        <form
          id="create-warehouse-form"
          onSubmit={handleSubmit}
          className="space-y-4"
        >
          {isAdmin ? (
            <FormField label="Công ty" required>
              <SelectInput
                value={form.tenantId}
                onChange={(e) => setForm({ ...form, tenantId: e.target.value })}
                required
              >
                <option value="">-- Chọn công ty --</option>
                {tenants.map((t) => (
                  <option key={t.id} value={t.id}>
                    {t.name} ({t.code})
                  </option>
                ))}
              </SelectInput>
            </FormField>
          ) : (
            <div className="rounded-lg border border-sky-200 bg-sky-50 px-4 py-3 text-sm text-sky-800"></div>
          )}

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            {(["name", "code", "address", "city"] as const).map((field) => (
              <FormField key={field} label={fieldLabels[field]} required>
                <TextInput
                  value={form[field]}
                  onChange={(e) =>
                    setForm({ ...form, [field]: e.target.value })
                  }
                  placeholder={`Nhập ${fieldLabels[field].toLowerCase()}...`}
                  required
                />
              </FormField>
            ))}
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
            <FormField label="Vĩ độ (Latitude)" required hint="VD: 21.0285">
              <TextInput
                type="number"
                step="any"
                value={form.latitude}
                onChange={(e) =>
                  setForm({ ...form, latitude: Number(e.target.value) })
                }
                required
              />
            </FormField>
            <FormField label="Kinh độ (Longitude)" required hint="VD: 105.8542">
              <TextInput
                type="number"
                step="any"
                value={form.longitude}
                onChange={(e) =>
                  setForm({ ...form, longitude: Number(e.target.value) })
                }
                required
              />
            </FormField>
          </div>
        </form>
      </Modal>

      <ConfirmModal
        open={confirmOpen}
        onClose={() => {
          if (!deleting) {
            setConfirmOpen(false);
            setDeleteTarget(null);
          }
        }}
        onConfirm={handleDeleteConfirm}
        title="Vô hiệu hóa kho hàng?"
        message={`Bạn có chắc muốn vô hiệu hóa kho "${deleteTarget?.name}"?`}
        confirmLabel="Vô hiệu hóa"
        loading={deleting}
      />
    </div>
  );
}
