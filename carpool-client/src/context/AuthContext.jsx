import { createContext, useCallback, useMemo, useState } from 'react'
import * as authApi from '../api/auth'
import { getMe } from '../api/users'
import { clearStoredAuth, getStoredAuth, setStoredAuth } from '../utils/authStorage'

export const AuthContext = createContext(null)

function readValidStoredAuth() {
  const stored = getStoredAuth()
  if (!stored) return null
  // A token whose expiry has already passed is treated as no session — the next
  // authenticated request would 401 anyway, so fail fast instead of flashing
  // protected UI before the first request clears it.
  if (stored.expiresAtUtc && new Date(stored.expiresAtUtc).getTime() <= Date.now()) {
    clearStoredAuth()
    return null
  }
  return stored
}

export function AuthProvider({ children }) {
  const [auth, setAuth] = useState(readValidStoredAuth)
  const [isLoading, setIsLoading] = useState(false)

  const login = useCallback(async (email, password) => {
    setIsLoading(true)
    try {
      const result = await authApi.login({ email, password })
      setStoredAuth(result)
      setAuth(result)
      return result
    } finally {
      setIsLoading(false)
    }
  }, [])

  const register = useCallback(async (payload) => {
    setIsLoading(true)
    try {
      // Registration does not return a session (spec: guide the user to login after).
      return await authApi.register(payload)
    } finally {
      setIsLoading(false)
    }
  }, [])

  const logout = useCallback(() => {
    clearStoredAuth()
    setAuth(null)
  }, [])

  const refreshUser = useCallback(async () => {
    const user = await getMe()
    setAuth((prev) => {
      if (!prev) return prev
      const next = { ...prev, user }
      setStoredAuth(next)
      return next
    })
    return user
  }, [])

  const value = useMemo(
    () => ({
      user: auth?.user ?? null,
      token: auth?.accessToken ?? null,
      isAuthenticated: Boolean(auth?.accessToken),
      isLoading,
      login,
      logout,
      register,
      refreshUser,
    }),
    [auth, isLoading, login, logout, register, refreshUser],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}
