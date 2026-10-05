import { useContext } from 'react'
import { ToastContext } from '../context/ToastContext'

export function useToast() {
  const ctx = useContext(ToastContext)
  if (!ctx) {
    throw new Error('useToast must be used within a ToastProvider')
  }
  return ctx
}

/** Convenience: turn a caught ApiError into a toast, keeping the correlationId visible. */
export function useApiErrorToast() {
  const { showToast } = useToast()
  return (error) =>
    showToast({
      type: 'error',
      message: error?.message || 'Something went wrong.',
      correlationId: error?.correlationId,
    })
}
