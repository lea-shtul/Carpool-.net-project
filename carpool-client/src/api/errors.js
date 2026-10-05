/**
 * Normalized API error. The backend's consistent error body is
 * `{ statusCode, message, correlationId }` (ExceptionHandlingMiddleware) — every
 * catch site in this app works with this shape instead of raw Axios errors.
 */
export class ApiError extends Error {
  constructor({ status, message, correlationId, isNetworkError = false }) {
    super(message)
    this.name = 'ApiError'
    this.status = status
    this.correlationId = correlationId
    this.isNetworkError = isNetworkError
  }
}

/** True for the "seat taken by someone else" optimistic-concurrency conflict (spec extension UX). */
export function isConcurrencyConflict(error) {
  return error instanceof ApiError && error.status === 409
}

export function toApiError(axiosError) {
  const response = axiosError.response
  if (!response) {
    return new ApiError({
      status: 0,
      message: 'Could not reach the server. Check your connection and try again.',
      isNetworkError: true,
    })
  }

  const body = response.data
  return new ApiError({
    status: response.status,
    message: body?.message || 'Something went wrong. Please try again.',
    correlationId: body?.correlationId,
  })
}
