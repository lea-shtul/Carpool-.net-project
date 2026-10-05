const STORAGE_KEY = 'carpool.auth'

/**
 * Single source of truth for the persisted session ({ accessToken, expiresAtUtc, user }).
 * Lives outside React so the axios interceptor (api/client.js) can read/clear it without
 * importing AuthContext and creating a cycle.
 */
export function getStoredAuth() {
  try {
    const raw = localStorage.getItem(STORAGE_KEY)
    return raw ? JSON.parse(raw) : null
  } catch {
    return null
  }
}

export function setStoredAuth(auth) {
  localStorage.setItem(STORAGE_KEY, JSON.stringify(auth))
}

export function clearStoredAuth() {
  localStorage.removeItem(STORAGE_KEY)
}
