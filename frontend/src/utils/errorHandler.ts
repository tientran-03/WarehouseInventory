import axios from 'axios'
import { resolveErrorMessage } from '../constants/errorMessages'
import type { ApiErrorResponse } from '../types'

export interface ParsedError {
  message: string
  errorCode?: string
  statusCode?: number
}

function extractMessageFromText(text: string): string {
  const trimmed = text.trim()
  const exceptionMatch = trimmed.match(/(?:Exception|Error):\s*(.+?)(?:\s+at\s|$)/)
  if (exceptionMatch?.[1]) return exceptionMatch[1].trim()
  return trimmed.split('\n')[0].slice(0, 300)
}

export function parseApiError(error: unknown, fallback = 'Đã xảy ra lỗi. Vui lòng thử lại.'): ParsedError {
  if (axios.isAxiosError(error)) {
    const statusCode = error.response?.status
    const data = error.response?.data

    if (typeof data === 'object' && data !== null) {
      const body = data as ApiErrorResponse
      const message = resolveErrorMessage(body.errorCode, body.message)
      return { message, errorCode: body.errorCode, statusCode }
    }

    if (typeof data === 'string' && data.length > 0) {
      return { message: extractMessageFromText(data), statusCode }
    }

    if (error.code === 'ERR_NETWORK') {
      return { message: resolveErrorMessage('NETWORK_ERROR'), errorCode: 'NETWORK_ERROR' }
    }

    if (statusCode === 401) {
      return { message: resolveErrorMessage('INVALID_CREDENTIALS'), errorCode: 'INVALID_CREDENTIALS', statusCode }
    }

    return { message: error.message || fallback, statusCode }
  }

  if (error instanceof Error) {
    return { message: extractMessageFromText(error.message) }
  }

  return { message: fallback }
}

export function getErrorMessage(error: unknown, fallback?: string): string {
  return parseApiError(error, fallback).message
}
