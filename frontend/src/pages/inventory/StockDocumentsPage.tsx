import { useEffect, useMemo, useState, type FormEvent } from "react";
import Alert from "../../components/ui/Alert";
import Button from "../../components/ui/Button";
import {
  FormField,
  ModalFooterButtons,
  SelectInput,
  TextArea,
  TextInput,
} from "../../components/ui/FormField";
import LoadingState from "../../components/ui/LoadingState";
import Modal from "../../components/ui/Modal";
import InvoicePrint from "../../components/ui/InvoicePrint";
import StatCard from "../../components/StatCard";
import { useTenantContext } from "../../context/TenantContext";
import { useToast } from "../../context/ToastContext";
import { invoiceService, stockDocumentService } from "../../services";
import { useProducts } from "../../hooks/useProducts";
import { useStockDocuments } from "../../hooks/useStockDocuments";
import { useTenantWarehouses } from "../../hooks/useTenantWarehouses";
import { useZones } from "../../hooks/useZones";
import type {
  CreateInvoiceRequest,
  CreateStockDocumentLineRequest,
  Invoice,
  StockDocumentType,
  StockMovementReportFilter,
} from "../../types";
import { stockDocumentTypeLabel } from "../../types";
import { parseApiError } from "../../utils/errorHandler";

type DocumentForm = {
  type: StockDocumentType;
  warehouseId: string;
  documentDate: string;
  partnerName: string;
  referenceCode: string;
  note: string;
  lines: CreateStockDocumentLineRequest[];
};

const emptyForm = (): DocumentForm => ({
  type: "Inbound",
  warehouseId: "",
  documentDate: new Date().toISOString().slice(0, 16),
  partnerName: "",
  referenceCode: "",
  note: "",
  lines: [],
});

export default function StockDocumentsPage() {
  const { activeTenantId } = useTenantContext();
  const { showError, showSuccess } = useToast();
  const { tenantWarehouses, loading: warehouseLoading } = useTenantWarehouses();
  const { zones, loading: zoneLoading } = useZones(activeTenantId);
  const { products, loading: productLoading } = useProducts(activeTenantId);
  const { documents, report, loading, error, createDocument, fetchReport } =
    useStockDocuments(activeTenantId);
  const [showModal, setShowModal] = useState(false);
  const [showInvoiceModal, setShowInvoiceModal] = useState(false);
  const [selectedDocument, setSelectedDocument] = useState<any>(null);
  const [invoiceData, setInvoiceData] = useState<Invoice | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [invoiceSubmitting, setInvoiceSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [invoiceFormError, setInvoiceFormError] = useState<string | null>(null);
  const [form, setForm] = useState<DocumentForm>(emptyForm);
  const [invoiceForm, setInvoiceForm] = useState({
    customerName: "",
    customerAddress: "",
    customerPhone: "",
    customerTaxCode: "",
    taxRate: 0.1,
    notes: "",
  });
  const [reportFilter, setReportFilter] = useState<
    Omit<StockMovementReportFilter, "tenantId">
  >({});

  const formZones = useMemo(
    () => zones.filter((zone) => zone.warehouseId === form.warehouseId),
    [zones, form.warehouseId],
  );

  useEffect(() => {
    if (activeTenantId) {
      fetchReport().catch((err) => showError(parseApiError(err).message));
    }
  }, [activeTenantId, fetchReport, showError]);

  const openModal = () => {
    const warehouseId = tenantWarehouses[0]?.id ?? "";
    const warehouseZones = zones.filter(
      (zone) => zone.warehouseId === warehouseId,
    );
    setForm({
      ...emptyForm(),
      warehouseId,
      lines:
        products[0] && warehouseZones[0]
          ? [
              {
                productId: products[0].id,
                warehouseZoneId: warehouseZones[0].id,
                quantity: 1,
                unitPrice: products[0].price,
              },
            ]
          : [],
    });
    setFormError(null);
    setShowModal(true);
  };

  const updateLine = (
    index: number,
    changes: Partial<CreateStockDocumentLineRequest>,
  ) => {
    setForm((current) => ({
      ...current,
      lines: current.lines.map((line, lineIndex) =>
        lineIndex === index ? { ...line, ...changes } : line,
      ),
    }));
  };

  const changeWarehouse = (warehouseId: string) => {
    const warehouseZones = zones.filter(
      (zone) => zone.warehouseId === warehouseId,
    );
    setForm((current) => ({
      ...current,
      warehouseId,
      lines: current.lines.map((line) => ({
        ...line,
        warehouseZoneId: warehouseZones[0]?.id ?? "",
      })),
    }));
  };

  const addLine = () => {
    const product = products[0];
    const zone = formZones[0];
    if (!product || !zone) return;
    setForm((current) => ({
      ...current,
      lines: [
        ...current.lines,
        {
          productId: product.id,
          warehouseZoneId: zone.id,
          quantity: 1,
          unitPrice: product.price,
        },
      ],
    }));
  };

  const removeLine = (index: number) => {
    setForm((current) => ({
      ...current,
      lines: current.lines.filter((_, lineIndex) => lineIndex !== index),
    }));
  };

  const handleSubmit = async (event: FormEvent) => {
    event.preventDefault();
    if (!activeTenantId) return;
    if (form.lines.length === 0) {
      setFormError("Phiếu phải có ít nhất một dòng hàng.");
      return;
    }

    setSubmitting(true);
    setFormError(null);
    try {
      await createDocument({
        tenantId: activeTenantId,
        warehouseId: form.warehouseId,
        type: form.type,
        documentDate: new Date(form.documentDate).toISOString(),
        partnerName: form.partnerName || null,
        referenceCode: form.referenceCode || null,
        note: form.note || null,
        lines: form.lines,
      });
      await fetchReport(reportFilter);
      showSuccess(
        `${stockDocumentTypeLabel(form.type)} thành công — tồn kho đã được cập nhật.`,
      );
      setShowModal(false);
    } catch (err) {
      const message = parseApiError(err).message;
      setFormError(message);
      showError(message);
    } finally {
      setSubmitting(false);
    }
  };

  const loadReport = async () => {
    try {
      await fetchReport(reportFilter);
    } catch (err) {
      showError(parseApiError(err).message);
    }
  };

  const downloadExcelReport = async () => {
    if (!activeTenantId) {
      showError("Chọn tenant ở sidebar để tải báo cáo.");
      return;
    }
    try {
      const { data } = await stockDocumentService.exportReportExcel({
        ...reportFilter,
        tenantId: activeTenantId,
      });
      const url = window.URL.createObjectURL(new Blob([data]));
      const link = document.createElement("a");
      link.href = url;
      link.setAttribute(
        "download",
        `bao-cao-nhap-xuat-${new Date().toISOString().slice(0, 10)}.xlsx`,
      );
      document.body.appendChild(link);
      link.click();
      link.remove();
      window.URL.revokeObjectURL(url);
      showSuccess("Đã tải báo cáo Excel.");
    } catch (err) {
      showError(parseApiError(err).message);
    }
  };

  const openCreateInvoice = (document: any) => {
    if (document.type !== "Outbound") {
      showError("Chỉ có thể tạo hóa đơn cho phiếu xuất kho.");
      return;
    }
    setSelectedDocument(document);
    setInvoiceForm({
      customerName: document.partnerName || "",
      customerAddress: "",
      customerPhone: "",
      customerTaxCode: "",
      taxRate: 0.1,
      notes: "",
    });
    setInvoiceFormError(null);
    setShowInvoiceModal(true);
  };

  const handleCreateInvoice = async (e: FormEvent) => {
    e.preventDefault();
    if (!activeTenantId || !selectedDocument) return;

    setInvoiceSubmitting(true);
    setInvoiceFormError(null);
    try {
      const payload: CreateInvoiceRequest = {
        tenantId: activeTenantId,
        stockDocumentId: selectedDocument.id,
        customerName: invoiceForm.customerName,
        customerAddress: invoiceForm.customerAddress || null,
        customerPhone: invoiceForm.customerPhone || null,
        customerTaxCode: invoiceForm.customerTaxCode || null,
        taxRate: invoiceForm.taxRate,
        notes: invoiceForm.notes || null,
      };
      const { data } = await invoiceService.create(payload);
      setInvoiceData(data);
      setShowInvoiceModal(false);
      showSuccess("Tạo hóa đơn thành công!");
    } catch (err) {
      const message = parseApiError(err).message;
      setInvoiceFormError(message);
      showError(message);
    } finally {
      setInvoiceSubmitting(false);
    }
  };

  const closeInvoicePrint = () => {
    setInvoiceData(null);
    setSelectedDocument(null);
  };

  if (!activeTenantId) {
    return (
      <Alert
        variant="warning"
        message="Chọn tenant ở sidebar để lập phiếu nhập/xuất kho."
      />
    );
  }

  if (loading || warehouseLoading || zoneLoading || productLoading) {
    return <LoadingState message="Đang tải dữ liệu nhập/xuất kho..." />;
  }

  const hasSetupData =
    tenantWarehouses.length > 0 && zones.length > 0 && products.length > 0;

  return (
    <div className="space-y-6">
      <div className="bg-white/60 backdrop-blur-md p-6 border border-white/70 shadow-sm rounded-2xl flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h2 className="text-xl font-normal text-slate-900 tracking-tight">
            Nhập / xuất kho
          </h2>
          <p className="text-sm text-slate-500 mt-1">
            Lập phiếu hoàn tất để cập nhật tồn kho, khu vực lưu trữ và nhật ký
            kiểm toán trong cùng một giao dịch.
          </p>
        </div>
        <Button variant="primary" onClick={openModal} disabled={!hasSetupData}>
          + Lập phiếu nhập / xuất
        </Button>
      </div>

      {!hasSetupData && (
        <Alert
          variant="warning"
          message="Cần có ít nhất một kho, khu vực kho và sản phẩm trước khi lập phiếu nhập/xuất."
        />
      )}
      {error && <Alert variant="error" message={error} />}

      <div className="bg-white border border-slate-200 shadow-sm rounded-2xl overflow-hidden">
        <div className="px-6 py-5 border-b border-slate-100 flex items-center justify-between">
          <h3 className="font-normal text-slate-900 text-base">
            Phiếu gần đây
          </h3>
          <span className="text-xs px-3 py-1 rounded-full bg-slate-50 text-slate-600 border border-slate-200">
            {documents.length} phiếu
          </span>
        </div>
        {documents.length === 0 ? (
          <p className="px-6 py-12 text-center text-slate-400 text-sm">
            Chưa có phiếu nhập/xuất kho.
          </p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-sm text-left">
              <thead className="bg-slate-50 border-b border-slate-100 text-xs text-slate-500 uppercase tracking-wider">
                <tr>
                  <th className="px-6 py-4">Mã phiếu</th>
                  <th className="px-6 py-4">Loại</th>
                  <th className="px-6 py-4">Kho</th>
                  <th className="px-6 py-4">Đối tác</th>
                  <th className="px-6 py-4 text-center">SL</th>
                  <th className="px-6 py-4 text-right">Giá trị</th>
                  <th className="px-6 py-4">Ngày</th>
                  <th className="px-6 py-4">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {documents.map((item) => (
                  <tr key={item.id} className="hover:bg-slate-50/80">
                    <td className="px-6 py-4 font-mono text-xs text-slate-600">
                      {item.documentCode}
                    </td>
                    <td className="px-6 py-4">
                      <span
                        className={`inline-flex rounded-full px-3 py-1 text-xs ${item.type === "Inbound" ? "bg-emerald-100 text-emerald-700" : "bg-rose-100 text-rose-700"}`}
                      >
                        {stockDocumentTypeLabel(item.type)}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-slate-700">
                      {item.warehouseName}
                    </td>
                    <td className="px-6 py-4 text-slate-600">
                      {item.partnerName || "—"}
                    </td>
                    <td className="px-6 py-4 text-center font-medium">
                      {item.totalQuantity}
                    </td>
                    <td className="px-6 py-4 text-right">
                      {item.totalValue.toLocaleString("vi-VN")} ₫
                    </td>
                    <td className="px-6 py-4 text-xs text-slate-500">
                      {new Date(item.documentDate).toLocaleString("vi-VN")}
                    </td>
                    <td className="px-6 py-4">
                      <div className="flex gap-1">
                        {item.type === "Outbound" && (
                          <Button
                            variant="ghost"
                            size="sm"
                            onClick={() => openCreateInvoice(item)}
                          >
                            Tạo hóa đơn
                          </Button>
                        )}
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <div className="bg-white border border-slate-200 shadow-sm rounded-2xl overflow-hidden">
        <div className="p-6 border-b border-slate-100 flex flex-col lg:flex-row lg:items-end lg:justify-between gap-4">
          <div>
            <h3 className="font-normal text-slate-900 text-base">
              Báo cáo nhập / xuất
            </h3>
          </div>
          <div className="flex gap-2">
            <Button variant="secondary" onClick={loadReport}>
              Xem báo cáo
            </Button>
            <Button variant="primary" onClick={downloadExcelReport}>
              Tải Excel
            </Button>
          </div>
        </div>
        <div className="p-6 grid grid-cols-1 md:grid-cols-4 gap-4 border-b border-slate-100">
          <FormField label="Kho">
            <SelectInput
              value={reportFilter.warehouseId ?? ""}
              onChange={(event) =>
                setReportFilter({
                  ...reportFilter,
                  warehouseId: event.target.value || undefined,
                })
              }
            >
              <option value="">Tất cả kho</option>
              {tenantWarehouses.map((warehouse) => (
                <option key={warehouse.id} value={warehouse.id}>
                  {warehouse.name}
                </option>
              ))}
            </SelectInput>
          </FormField>
          <FormField label="Loại phiếu">
            <SelectInput
              value={reportFilter.type ?? ""}
              onChange={(event) =>
                setReportFilter({
                  ...reportFilter,
                  type: (event.target.value || undefined) as
                    | StockDocumentType
                    | undefined,
                })
              }
            >
              <option value="">Tất cả</option>
              <option value="Inbound">Nhập kho</option>
              <option value="Outbound">Xuất kho</option>
            </SelectInput>
          </FormField>
          <FormField label="Từ ngày">
            <TextInput
              type="date"
              value={reportFilter.fromDate ?? ""}
              onChange={(event) =>
                setReportFilter({
                  ...reportFilter,
                  fromDate: event.target.value || undefined,
                })
              }
            />
          </FormField>
          <FormField label="Đến ngày">
            <TextInput
              type="date"
              value={reportFilter.toDate ?? ""}
              onChange={(event) =>
                setReportFilter({
                  ...reportFilter,
                  toDate: event.target.value || undefined,
                })
              }
            />
          </FormField>
        </div>
        <div className="p-6 grid grid-cols-1 md:grid-cols-4 gap-4">
          <StatCard title="Số phiếu" value={report?.documentCount ?? 0} />
          <StatCard title="Tổng nhập" value={report?.inboundQuantity ?? 0} />
          <StatCard title="Tổng xuất" value={report?.outboundQuantity ?? 0} />
          <StatCard
            title="Giá trị xuất"
            value={`${(report?.outboundValue ?? 0).toLocaleString("vi-VN")} ₫`}
          />
        </div>
        {!report || report.rows.length === 0 ? (
          <p className="px-6 pb-10 text-center text-slate-400 text-sm">
            Chưa có dữ liệu theo điều kiện lọc.
          </p>
        ) : (
          <div className="overflow-x-auto border-t border-slate-100">
            <table className="w-full text-sm text-left">
              <thead className="bg-slate-50 border-b border-slate-100 text-xs text-slate-500 uppercase tracking-wider">
                <tr>
                  <th className="px-6 py-4">Ngày</th>
                  <th className="px-6 py-4">Phiếu</th>
                  <th className="px-6 py-4">Hàng hóa</th>
                  <th className="px-6 py-4">Khu</th>
                  <th className="px-6 py-4 text-center">Loại / SL</th>
                  <th className="px-6 py-4 text-right">Thành tiền</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-slate-100">
                {report.rows.map((row, index) => (
                  <tr key={`${row.documentCode}-${row.productSku}-${index}`}>
                    <td className="px-6 py-4 text-xs text-slate-500">
                      {new Date(row.documentDate).toLocaleDateString("vi-VN")}
                    </td>
                    <td className="px-6 py-4 font-mono text-xs">
                      {row.documentCode}
                    </td>
                    <td className="px-6 py-4">
                      <p className="font-medium">{row.productName}</p>
                      <p className="text-xs text-slate-500">{row.productSku}</p>
                    </td>
                    <td className="px-6 py-4">{row.warehouseZoneCode}</td>
                    <td className="px-6 py-4 text-center">
                      <span
                        className={
                          row.type === "Inbound"
                            ? "text-emerald-700"
                            : "text-rose-700"
                        }
                      >
                        {stockDocumentTypeLabel(row.type)} · {row.quantity}
                      </span>
                    </td>
                    <td className="px-6 py-4 text-right">
                      {row.lineTotal.toLocaleString("vi-VN")} ₫
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>

      <Modal
        open={showModal}
        onClose={() => !submitting && setShowModal(false)}
        title="Lập phiếu nhập / xuất kho"
        description="Khi lưu, phiếu được hoàn tất ngay và không thể chỉnh sửa để bảo toàn lịch sử tồn kho."
        footer={
          <ModalFooterButtons
            onCancel={() => setShowModal(false)}
            submitLabel="Hoàn tất phiếu"
            loading={submitting}
            submitForm="stock-document-form"
          />
        }
      >
        {formError && (
          <div className="mb-4">
            <Alert variant="error" message={formError} />
          </div>
        )}
        <form
          id="stock-document-form"
          onSubmit={handleSubmit}
          className="space-y-4"
        >
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <FormField label="Loại phiếu" required>
              <SelectInput
                value={form.type}
                onChange={(event) =>
                  setForm({
                    ...form,
                    type: event.target.value as StockDocumentType,
                  })
                }
              >
                <option value="Inbound">Nhập kho</option>
                <option value="Outbound">Xuất kho</option>
              </SelectInput>
            </FormField>
            <FormField label="Kho" required>
              <SelectInput
                value={form.warehouseId}
                onChange={(event) => changeWarehouse(event.target.value)}
              >
                <option value="">Chọn kho</option>
                {tenantWarehouses.map((warehouse) => (
                  <option key={warehouse.id} value={warehouse.id}>
                    {warehouse.name}
                  </option>
                ))}
              </SelectInput>
            </FormField>
            <FormField label="Ngày chứng từ" required>
              <TextInput
                type="datetime-local"
                value={form.documentDate}
                onChange={(event) =>
                  setForm({ ...form, documentDate: event.target.value })
                }
                required
              />
            </FormField>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <FormField label="Đối tác / người nhận">
              <TextInput
                value={form.partnerName}
                onChange={(event) =>
                  setForm({ ...form, partnerName: event.target.value })
                }
                placeholder="Nhà cung cấp hoặc khách hàng"
              />
            </FormField>
            <FormField label="Mã tham chiếu">
              <TextInput
                value={form.referenceCode}
                onChange={(event) =>
                  setForm({ ...form, referenceCode: event.target.value })
                }
                placeholder="Số hóa đơn, đơn hàng..."
              />
            </FormField>
          </div>
          <FormField label="Ghi chú">
            <TextArea
              value={form.note}
              onChange={(event) =>
                setForm({ ...form, note: event.target.value })
              }
              rows={2}
            />
          </FormField>
          <div className="border border-slate-200 rounded-xl overflow-hidden">
            <div className="p-4 bg-slate-50 flex justify-between items-center">
              <p className="font-medium text-sm">Dòng hàng</p>
              <Button
                variant="secondary"
                size="sm"
                onClick={addLine}
                disabled={!products[0] || !formZones[0]}
              >
                + Thêm dòng
              </Button>
            </div>
            <div className="p-4 space-y-3">
              {form.lines.map((line, index) => (
                <div
                  key={index}
                  className="grid grid-cols-1 md:grid-cols-[2fr_1.5fr_90px_110px_auto] gap-3 items-end"
                >
                  <FormField label={index === 0 ? "Sản phẩm" : ""} required>
                    <SelectInput
                      value={line.productId}
                      onChange={(event) => {
                        const product = products.find(
                          (item) => item.id === event.target.value,
                        );
                        updateLine(index, {
                          productId: event.target.value,
                          unitPrice: product?.price ?? line.unitPrice,
                        });
                      }}
                    >
                      <option value="">Chọn sản phẩm</option>
                      {products.map((product) => (
                        <option key={product.id} value={product.id}>
                          {product.name} ({product.sku})
                        </option>
                      ))}
                    </SelectInput>
                  </FormField>
                  <FormField label={index === 0 ? "Khu vực" : ""} required>
                    <SelectInput
                      value={line.warehouseZoneId}
                      onChange={(event) =>
                        updateLine(index, {
                          warehouseZoneId: event.target.value,
                        })
                      }
                    >
                      <option value="">Chọn khu vực</option>
                      {formZones.map((zone) => (
                        <option key={zone.id} value={zone.id}>
                          {zone.code} · {zone.name}
                        </option>
                      ))}
                    </SelectInput>
                  </FormField>
                  <FormField label={index === 0 ? "Số lượng" : ""} required>
                    <TextInput
                      type="number"
                      min={1}
                      value={line.quantity}
                      onChange={(event) =>
                        updateLine(index, {
                          quantity: Number(event.target.value),
                        })
                      }
                      required
                    />
                  </FormField>
                  <FormField label={index === 0 ? "Đơn giá" : ""}>
                    <TextInput
                      type="number"
                      min={0}
                      value={line.unitPrice ?? ""}
                      onChange={(event) =>
                        updateLine(index, {
                          unitPrice:
                            event.target.value === ""
                              ? null
                              : Number(event.target.value),
                        })
                      }
                    />
                  </FormField>
                  <Button
                    variant="danger-ghost"
                    size="sm"
                    onClick={() => removeLine(index)}
                    disabled={form.lines.length === 1}
                  >
                    Xóa
                  </Button>
                </div>
              ))}
            </div>
          </div>
        </form>
      </Modal>

      <Modal
        open={showInvoiceModal}
        onClose={() => !invoiceSubmitting && setShowInvoiceModal(false)}
        title="Tạo hóa đơn xuất kho"
        description="Nhập thông tin khách hàng để tạo hóa đơn."
        footer={
          <ModalFooterButtons
            onCancel={() => setShowInvoiceModal(false)}
            submitLabel="Tạo hóa đơn"
            loading={invoiceSubmitting}
            submitForm="invoice-form"
          />
        }
      >
        {invoiceFormError && (
          <div className="mb-4">
            <Alert variant="error" message={invoiceFormError} />
          </div>
        )}
        <form
          id="invoice-form"
          onSubmit={handleCreateInvoice}
          className="space-y-4"
        >
          <FormField label="Tên khách hàng" required>
            <TextInput
              value={invoiceForm.customerName}
              onChange={(e) =>
                setInvoiceForm({ ...invoiceForm, customerName: e.target.value })
              }
              placeholder="Nhập tên khách hàng"
              required
            />
          </FormField>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <FormField label="Địa chỉ">
              <TextInput
                value={invoiceForm.customerAddress}
                onChange={(e) =>
                  setInvoiceForm({
                    ...invoiceForm,
                    customerAddress: e.target.value,
                  })
                }
                placeholder="Địa chỉ khách hàng"
              />
            </FormField>
            <FormField label="Điện thoại">
              <TextInput
                value={invoiceForm.customerPhone}
                onChange={(e) =>
                  setInvoiceForm({
                    ...invoiceForm,
                    customerPhone: e.target.value,
                  })
                }
                placeholder="Số điện thoại"
              />
            </FormField>
          </div>
          <FormField label="Mã số thuế">
            <TextInput
              value={invoiceForm.customerTaxCode}
              onChange={(e) =>
                setInvoiceForm({
                  ...invoiceForm,
                  customerTaxCode: e.target.value,
                })
              }
              placeholder="Mã số thuế (nếu có)"
            />
          </FormField>
          <FormField label="Thuế suất">
            <SelectInput
              value={invoiceForm.taxRate.toString()}
              onChange={(e) =>
                setInvoiceForm({
                  ...invoiceForm,
                  taxRate: Number(e.target.value),
                })
              }
            >
              <option value="0">0%</option>
              <option value="0.05">5%</option>
              <option value="0.1">10%</option>
              <option value="0.08">8%</option>
            </SelectInput>
          </FormField>
          <FormField label="Ghi chú">
            <TextArea
              value={invoiceForm.notes}
              onChange={(e) =>
                setInvoiceForm({ ...invoiceForm, notes: e.target.value })
              }
              rows={2}
            />
          </FormField>
        </form>
      </Modal>

      {invoiceData && (
        <InvoicePrint invoice={invoiceData} onClose={closeInvoicePrint} />
      )}
    </div>
  );
}
