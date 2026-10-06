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

/**
 * True specifically for "you already have an active booking for this ride" (spec §26) — a
 * 409 from the same POST /api/rides/{id}/bookings endpoint as a real concurrency conflict,
 * but a different situation entirely (nobody raced you; your own earlier booking is still
 * active). The backend's error body has no machine-readable error code (the fixed
 * {statusCode, message, correlationId} shape), so this is distinguished by message text —
 * which is a stable, hard-coded literal in BookingService.CreateAsync, not user input.
 */
export function isDuplicateBookingConflict(error) {
  return error instanceof ApiError && error.status === 409 && /already have an active booking/i.test(error.message)
}

/** True for the "seat taken by someone else" optimistic-concurrency conflict (spec extension UX). */
export function isConcurrencyConflict(error) {
  return error instanceof ApiError && error.status === 409 && !isDuplicateBookingConflict(error)
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
