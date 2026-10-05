import axios from 'axios'
import { clearStoredAuth, getStoredAuth } from '../utils/authStorage'
import { toApiError } from './errors'

/**
 * The single Axios instance for the whole app (spec: "do not duplicate Axios logic
 * across components"). Requests go to /api, which vite.config.js proxies to the real
 * ASP.NET Core backend — no CORS setup needed in dev.
 */
export const apiClient = axios.create({
  baseURL: '/api',
})

apiClient.interceptors.request.use((config) => {
  const auth = getStoredAuth()
  if (auth?.accessToken) {
    config.headers.Authorization = `Bearer ${auth.accessToken}`
  }
  return config
})

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    const hadAuthHeader = Boolean(error.config?.headers?.Authorization)
    const isAuthRoute = error.config?.url?.startsWith('/auth/')

    // An expired/invalid token on an authenticated request: clear the stale session and
    // send the user back to login. A 401 from /auth/login itself (bad credentials) is a
    // normal form error, not a session expiry, so it must not trigger this.
    if (error.response?.status === 401 && hadAuthHeader && !isAuthRoute) {
      clearStoredAuth()
      if (window.location.pathname !== '/login') {
        window.location.assign('/login')
      }
    }

    return Promise.reject(toApiError(error))
  },
)
