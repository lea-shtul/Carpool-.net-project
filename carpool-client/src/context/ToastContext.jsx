import { createContext, useCallback, useMemo, useRef, useState } from 'react'

export const ToastContext = createContext(null)

let nextId = 1

export function ToastProvider({ children }) {
  const [toasts, setToasts] = useState([])
  const timers = useRef(new Map())

  const dismissToast = useCallback((id) => {
    setToasts((prev) => prev.filter((t) => t.id !== id))
    const timer = timers.current.get(id)
    if (timer) {
      clearTimeout(timer)
      timers.current.delete(id)
    }
  }, [])

  const showToast = useCallback(
    ({ type = 'info', message, correlationId, duration = 5000 }) => {
      const id = nextId++
      setToasts((prev) => [...prev, { id, type, message, correlationId }])
      const timer = setTimeout(() => dismissToast(id), duration)
      timers.current.set(id, timer)
      return id
    },
    [dismissToast],
  )

  const value = useMemo(() => ({ toasts, showToast, dismissToast }), [toasts, showToast, dismissToast])

  return <ToastContext.Provider value={value}>{children}</ToastContext.Provider>
}
