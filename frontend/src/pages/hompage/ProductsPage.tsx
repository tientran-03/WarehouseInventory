import { useState } from 'react'
import type { FormEvent } from 'react'
import Alert from '../../components/ui/Alert'
import Button from '../../components/ui/Button'
import ConfirmModal from '../../components/ui/ConfirmModal'
import { FormField, ModalFooterButtons, SelectInput, TextInput } from '../../components/ui/FormField'
import LoadingState from '../../components/ui/LoadingState'
import Modal from '../../components/ui/Modal'
import PageSectionHeader from '../../components/ui/PageSectionHeader'
import { useTenantContext } from '../../context/TenantContext'
import { useToast } from '../../context/ToastContext'
import { useProducts } from '../../hooks/useProducts'
import { productService } from '../../services/productService'
import { parseApiError } from '../../utils/errorHandler'
import type { Product } from '../../types/product.types'

const emptyProductForm = { categoryId: '', sku: '', name: '', price: 0, imageUrl: '' }

export default function ProductsPage() {
  const { activeTenantId } = useTenantContext()
  const { showError, showSuccess } = useToast()
  const { products, categories, loading, error, createProduct, updateProduct, createCategory, deleteProduct } = useProducts(activeTenantId)
  const [productModalOpen, setProductModalOpen] = useState(false)
  const [categoryModalOpen, setCategoryModalOpen] = useState(false)
  const [productForm, setProductForm] = useState(emptyProductForm)
  const [categoryName, setCategoryName] = useState('')
  const [submitting, setSubmitting] = useState(false)
  const [uploadingImage, setUploadingImage] = useState(false)
  const [formError, setFormError] = useState<string | null>(null)
  const [editingProduct, setEditingProduct] = useState<Product | null>(null)
  const [deleteTarget, setDeleteTarget] = useState<Product | null>(null)
  const [deleting, setDeleting] = useState(false)

  const closeProductModal = () => {
    if (submitting) return
    setProductModalOpen(false)
    setProductForm(emptyProductForm)
    setFormError(null)
    setEditingProduct(null)
  }

  const closeCategoryModal = () => {
    if (submitting) return
    setCategoryModalOpen(false)
    setCategoryName('')
    setFormError(null)
  }

  const handleCreateCategory = async (e: FormEvent) => {
    e.preventDefault()
    if (!activeTenantId) return

    setSubmitting(true)
    setFormError(null)
    try {
      await createCategory({ tenantId: activeTenantId, name: categoryName.trim() })
      showSuccess('Tạo danh mục thành công!')
      setCategoryModalOpen(false)
      setCategoryName('')
      setFormError(null)
    } catch (err) {
      const parsed = parseApiError(err)
      setFormError(parsed.message)
      showError(parsed.message)
    } finally {
      setSubmitting(false)
    }
  }

  const handleCreateProduct = async (e: FormEvent) => {
    e.preventDefault()
    if (!activeTenantId) return

    setSubmitting(true)
    setFormError(null)
    try {
      const payload = {
        tenantId: activeTenantId,
        categoryId: productForm.categoryId,
        sku: productForm.sku.toUpperCase(),
        name: productForm.name,
        price: productForm.price,
      }
      if (editingProduct) {
        await updateProduct(editingProduct.id, payload)
        showSuccess('Cập nhật sản phẩm thành công!')
      } else {
        await createProduct(payload)
        showSuccess('Tạo sản phẩm thành công!')
      }
      setProductModalOpen(false)
      setProductForm(emptyProductForm)
      setFormError(null)
      setEditingProduct(null)
    } catch (err) {
      const parsed = parseApiError(err)
      setFormError(parsed.message)
      showError(parsed.message)
    } finally {
      setSubmitting(false)
    }
  }

  const openEditProduct = (product: Product) => {
    setProductForm({
      categoryId: product.categoryId,
      sku: product.sku,
      name: product.name,
      price: product.price,
      imageUrl: product.imageUrl || '',
    })
    setEditingProduct(product)
    setFormError(null)
    setProductModalOpen(true)
  }

  const openCreateProduct = () => {
    setProductForm(emptyProductForm)
    setEditingProduct(null)
    setFormError(null)
    setProductModalOpen(true)
  }

  const handleDeleteProduct = async () => {
    if (!deleteTarget) return
    setDeleting(true)
    try {
      await deleteProduct(deleteTarget.id)
      showSuccess('Đã xóa sản phẩm.')
      setDeleteTarget(null)
    } catch (err) {
      showError(parseApiError(err).message, 'Không thể xóa sản phẩm')
    } finally {
      setDeleting(false)
    }
  }

  const handleImageUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]
    if (!file || !editingProduct) return

    setUploadingImage(true)
    try {
      const { data } = await productService.uploadImage(editingProduct.id, file)
      setProductForm({ ...productForm, imageUrl: data.imageUrl })
      showSuccess('Upload ảnh thành công!')
    } catch (err) {
      showError(parseApiError(err).message)
    } finally {
      setUploadingImage(false)
    }
  }

  if (!activeTenantId) {
    return <Alert variant="warning" message="Chọn tenant ở sidebar để quản lý sản phẩm." />
  }

  if (loading) return <LoadingState message="Đang tải sản phẩm..." />

  return (
    <div className="space-y-6">
      <PageSectionHeader
        title="Quản lý Sản phẩm"
        badge={`${products.length} SP · ${categories.length} danh mục`}
        action={
          <div className="flex gap-2">
            <Button variant="secondary" onClick={() => setCategoryModalOpen(true)}>
              Thêm danh mục
            </Button>
            <Button variant="primary" onClick={openCreateProduct} disabled={categories.length === 0}>
              Thêm sản phẩm
            </Button>
          </div>
        }
      />

      {categories.length === 0 && (
        <Alert variant="info" message="Tenant chưa có danh mục. Tạo danh mục trước khi thêm sản phẩm." />
      )}

      {error && <Alert variant="error" message={error} />}

      {categories.length > 0 && (
        <div className="flex flex-wrap gap-2">
          {categories.map((c) => (
            <span key={c.id} className="inline-flex px-3 py-1 text-xs bg-slate-100 text-slate-700 rounded-full border border-slate-200">
              {c.name}
            </span>
          ))}
        </div>
      )}

      <div className="bg-white border border-slate-200 shadow-sm overflow-hidden rounded-xl">
        <table className="w-full text-sm text-left">
          <thead className="bg-slate-50/75 border-b border-slate-100 text-xs text-slate-500 uppercase">
            <tr>
              <th className="px-6 py-4">Ảnh</th>
              <th className="px-6 py-4">SKU</th>
              <th className="px-6 py-4">Tên</th>
              <th className="px-6 py-4 text-right">Giá</th>
              <th className="px-6 py-4 text-right">Thao tác</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {products.length === 0 ? (
              <tr>
                <td colSpan={5} className="px-6 py-12 text-center text-slate-400">
                  Chưa có sản phẩm.
                </td>
              </tr>
            ) : (
              products.map((p) => (
                <tr key={p.id} className="hover:bg-slate-50/80">
                  <td className="px-6 py-4">
                    {p.imageUrl ? (
                      <img src={p.imageUrl} alt={p.name} className="w-12 h-12 object-cover rounded-lg" />
                    ) : (
                      <div className="w-12 h-12 bg-slate-100 rounded-lg flex items-center justify-center text-slate-400 text-xs">
                        No img
                      </div>
                    )}
                  </td>
                  <td className="px-6 py-4 font-mono text-slate-600">{p.sku}</td>
                  <td className="px-6 py-4 font-medium text-slate-900">{p.name}</td>
                  <td className="px-6 py-4 text-right">{p.price.toLocaleString('vi-VN')} ₫</td>
                  <td className="px-6 py-4">
                    <div className="flex justify-end gap-1">
                      <Button variant="ghost" size="sm" onClick={() => openEditProduct(p)}>Sửa</Button>
                      <Button variant="danger-ghost" size="sm" onClick={() => setDeleteTarget(p)}>Xóa</Button>
                    </div>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      <Modal
        open={categoryModalOpen}
        onClose={closeCategoryModal}
        title="Thêm danh mục"
        description="Danh mục dùng để nhóm sản phẩm trong tenant."
        footer={
          <ModalFooterButtons onCancel={closeCategoryModal} submitLabel="Tạo danh mục" loading={submitting} submitForm="category-form" />
        }
      >
        {formError && <div className="mb-4"><Alert variant="error" message={formError} /></div>}
        <form id="category-form" onSubmit={handleCreateCategory} className="space-y-4">
          <FormField label="Tên danh mục" required>
            <TextInput 
              value={categoryName} 
              onChange={(e) => setCategoryName(e.target.value)} 
              placeholder="Nhập tên danh mục (ví dụ: Điện tử, Thời trang...)"
              required 
            />
          </FormField>
        </form>
      </Modal>

      <Modal
        open={productModalOpen}
        onClose={closeProductModal}
        title={editingProduct ? 'Chỉnh sửa sản phẩm' : 'Thêm sản phẩm'}
        description="Sản phẩm gắn với tenant và danh mục."
        footer={
          <ModalFooterButtons onCancel={closeProductModal} submitLabel={editingProduct ? 'Lưu thay đổi' : 'Tạo sản phẩm'} loading={submitting} submitForm="product-form" />
        }
      >
        {formError && <div className="mb-4"><Alert variant="error" message={formError} /></div>}
        <form id="product-form" onSubmit={handleCreateProduct} className="space-y-4">
          <FormField label="Danh mục" required>
            <SelectInput 
              value={productForm.categoryId} 
              onChange={(e) => setProductForm({ ...productForm, categoryId: e.target.value })} 
              required
            >
              <option value="">-- Chọn danh mục sản phẩm --</option>
              {categories.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </SelectInput>
          </FormField>
          <FormField label="SKU" required>
            <TextInput 
              value={productForm.sku} 
              onChange={(e) => setProductForm({ ...productForm, sku: e.target.value })} 
              placeholder="Nhập mã SKU (ví dụ: SP-IP15-PRO)"
              required 
            />
          </FormField>
          <FormField label="Tên sản phẩm" required>
            <TextInput 
              value={productForm.name} 
              onChange={(e) => setProductForm({ ...productForm, name: e.target.value })} 
              placeholder="Nhập tên đầy đủ của sản phẩm"
              required 
            />
          </FormField>
          <FormField label="Giá (VND)" required>
            <TextInput 
              type="number" 
              min={0} 
              value={productForm.price} 
              onChange={(e) => setProductForm({ ...productForm, price: Number(e.target.value) })} 
              placeholder="Nhập giá bán (ví dụ: 25000000)"
              required 
            />
          </FormField>
          {editingProduct && (
            <FormField label="Ảnh sản phẩm">
              <div className="space-y-3">
                {productForm.imageUrl && (
                  <img src={productForm.imageUrl} alt="Product" className="w-32 h-32 object-cover rounded-lg" />
                )}
                <div>
                  <input
                    type="file"
                    accept="image/*"
                    onChange={handleImageUpload}
                    disabled={uploadingImage}
                    className="block w-full text-sm text-slate-500 file:mr-4 file:py-2 file:px-4 file:rounded-lg file:border-0 file:text-sm file:font-medium file:bg-slate-100 file:text-slate-700 hover:file:bg-slate-200"
                  />
                  {uploadingImage && <p className="text-xs text-slate-500 mt-1">Đang upload ảnh...</p>}
                </div>
              </div>
            </FormField>
          )}
        </form>
      </Modal>

      <ConfirmModal
        open={deleteTarget !== null}
        onClose={() => !deleting && setDeleteTarget(null)}
        onConfirm={handleDeleteProduct}
        title="Xóa sản phẩm?"
        message={`Sản phẩm "${deleteTarget?.name ?? ''}" sẽ không còn hiển thị trong danh sách.`}
        confirmLabel="Xóa sản phẩm"
        loading={deleting}
      />
    </div>
  )
}
