export const ERROR_MESSAGES: Record<string, string> = {
  INVALID_CREDENTIALS: 'Tên đăng nhập hoặc mật khẩu không chính xác.',
  RESOURCE_NOT_FOUND: 'Không tìm thấy dữ liệu yêu cầu.',
  VALIDATION_ERROR: 'Dữ liệu không hợp lệ. Kiểm tra userName và password.',
  INSUFFICIENT_STOCK: 'Không đủ tồn kho để thực hiện thao tác.',
  INVENTORY_NOT_FOUND: 'Không tìm thấy bản ghi tồn kho.',
  INTERNAL_ERROR: 'Đã xảy ra lỗi hệ thống. Vui lòng thử lại sau.',
  JWT_CONFIG_ERROR: 'Lỗi cấu hình xác thực trên máy chủ. Liên hệ quản trị viên.',
  NETWORK_ERROR: 'Không thể kết nối máy chủ. Kiểm tra backend đã chạy chưa.',
  UNAUTHORIZED: 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.',
}

export function resolveErrorMessage(errorCode?: string, fallbackMessage?: string): string {
  if (fallbackMessage) return fallbackMessage
  if (errorCode && ERROR_MESSAGES[errorCode]) return ERROR_MESSAGES[errorCode]
  return ERROR_MESSAGES.INTERNAL_ERROR
}
