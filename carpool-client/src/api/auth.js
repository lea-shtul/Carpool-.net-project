import { apiClient } from './client'

export function register(payload) {
  // { firstName, lastName, email, password, phoneNumber } -> UserResponse
  return apiClient.post('/auth/register', payload).then((r) => r.data)
}

export function login(payload) {
  // { email, password } -> { accessToken, expiresAtUtc, user }
  return apiClient.post('/auth/login', payload).then((r) => r.data)
}
