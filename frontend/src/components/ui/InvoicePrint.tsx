import type { Invoice } from '../../types'

interface InvoicePrintProps {
  invoice: Invoice
  onClose: () => void
}

export default function InvoicePrint({ invoice, onClose }: InvoicePrintProps) {
  const handlePrint = () => {
    window.print()
  }

  return (
    <div className="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
      <div className="bg-white max-w-3xl w-full max-h-[90vh] overflow-auto rounded-xl shadow-2xl">
        <div className="sticky top-0 bg-white border-b border-slate-200 p-4 flex justify-between items-center">
          <h2 className="text-lg font-semibold">Hóa đơn xuất kho</h2>
          <div className="flex gap-2">
            <button
              onClick={handlePrint}
              className="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 text-sm font-medium"
            >
              In hóa đơn
            </button>
            <button
              onClick={onClose}
              className="px-4 py-2 bg-slate-200 text-slate-700 rounded-lg hover:bg-slate-300 text-sm font-medium"
            >
              Đóng
            </button>
          </div>
        </div>

        <div className="p-8" id="invoice-content">
          {/* Header */}
          <div className="text-center mb-8">
            <h1 className="text-2xl font-bold text-slate-900 mb-2">HÓA ĐƠN XUẤT KHO</h1>
            <p className="text-slate-600">Mã hóa đơn: {invoice.invoiceCode}</p>
            <p className="text-slate-600">Ngày: {new Date(invoice.invoiceDate).toLocaleDateString('vi-VN')}</p>
          </div>

          {/* Customer Info */}
          <div className="mb-8 p-4 bg-slate-50 rounded-lg">
            <h3 className="font-semibold text-slate-900 mb-3">Thông tin khách hàng</h3>
            <div className="grid grid-cols-2 gap-4 text-sm">
              <div>
                <span className="text-slate-600">Tên khách hàng:</span>
                <span className="ml-2 font-medium">{invoice.customerName}</span>
              </div>
              {invoice.customerPhone && (
                <div>
                  <span className="text-slate-600">Điện thoại:</span>
                  <span className="ml-2 font-medium">{invoice.customerPhone}</span>
                </div>
              )}
              {invoice.customerAddress && (
                <div className="col-span-2">
                  <span className="text-slate-600">Địa chỉ:</span>
                  <span className="ml-2 font-medium">{invoice.customerAddress}</span>
                </div>
              )}
              {invoice.customerTaxCode && (
                <div>
                  <span className="text-slate-600">Mã số thuế:</span>
                  <span className="ml-2 font-medium">{invoice.customerTaxCode}</span>
                </div>
              )}
            </div>
          </div>

          {/* Invoice Lines */}
          <div className="mb-8">
            <h3 className="font-semibold text-slate-900 mb-3">Chi tiết hàng hóa</h3>
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-slate-100">
                  <th className="px-4 py-2 text-left">SKU</th>
                  <th className="px-4 py-2 text-left">Tên sản phẩm</th>
                  <th className="px-4 py-2 text-left">Khu vực</th>
                  <th className="px-4 py-2 text-right">Số lượng</th>
                  <th className="px-4 py-2 text-right">Đơn giá</th>
                  <th className="px-4 py-2 text-right">Thành tiền</th>
                </tr>
              </thead>
              <tbody>
                {invoice.lines.map((line, index) => (
                  <tr key={index} className="border-b border-slate-200">
                    <td className="px-4 py-2">{line.productSku}</td>
                    <td className="px-4 py-2">{line.productName}</td>
                    <td className="px-4 py-2">{line.warehouseZoneCode}</td>
                    <td className="px-4 py-2 text-right">{line.quantity}</td>
                    <td className="px-4 py-2 text-right">{line.unitPrice.toLocaleString('vi-VN')} ₫</td>
                    <td className="px-4 py-2 text-right">{line.lineTotal.toLocaleString('vi-VN')} ₫</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>

          {/* Totals */}
          <div className="flex justify-end">
            <div className="w-64 space-y-2 text-sm">
              <div className="flex justify-between">
                <span className="text-slate-600">Tổng tiền hàng:</span>
                <span className="font-medium">{invoice.subtotal.toLocaleString('vi-VN')} ₫</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-600">Thuế ({(invoice.taxRate * 100).toFixed(0)}%):</span>
                <span className="font-medium">{invoice.taxAmount.toLocaleString('vi-VN')} ₫</span>
              </div>
              <div className="flex justify-between pt-2 border-t-2 border-slate-900">
                <span className="font-semibold text-slate-900">Tổng cộng:</span>
                <span className="font-bold text-lg text-slate-900">{invoice.totalAmount.toLocaleString('vi-VN')} ₫</span>
              </div>
            </div>
          </div>

          {/* Notes */}
          {invoice.notes && (
            <div className="mt-8 p-4 bg-slate-50 rounded-lg">
              <h3 className="font-semibold text-slate-900 mb-2">Ghi chú</h3>
              <p className="text-sm text-slate-600">{invoice.notes}</p>
            </div>
          )}

          {/* Footer */}
          <div className="mt-12 pt-8 border-t border-slate-200 text-center text-sm text-slate-600">
            <p>Cảm ơn quý khách đã sử dụng dịch vụ!</p>
            <p className="mt-2">Phiếu xuất kho: {invoice.stockDocumentCode}</p>
          </div>
        </div>
      </div>
    </div>
  )
}
