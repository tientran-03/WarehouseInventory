export interface ApiErrorResponse {
  success: false
  errorCode?: string
  message?: string
}

export interface ApiSuccessResponse<T> {
  success: true
  data: T
}
