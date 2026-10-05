import { apiClient } from './client'

export function getMe() {
  return apiClient.get('/users/me').then((r) => r.data)
}

/** Admin only. */
export function getAllUsers() {
  return apiClient.get('/users').then((r) => r.data)
}

/** Admin only. */
export function setUserStatus(userId, isActive) {
  return apiClient.patch(`/users/${userId}/status`, { isActive }).then((r) => r.data)
}
