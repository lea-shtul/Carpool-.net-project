import { apiClient } from './client'

export function getMyVehicles() {
  return apiClient.get('/vehicles/my').then((r) => r.data)
}

export function getVehicle(id) {
  return apiClient.get(`/vehicles/${id}`).then((r) => r.data)
}

export function createVehicle(payload) {
  return apiClient.post('/vehicles', payload).then((r) => r.data)
}

export function updateVehicle(id, payload) {
  return apiClient.put(`/vehicles/${id}`, payload).then((r) => r.data)
}

export function deleteVehicle(id) {
  return apiClient.delete(`/vehicles/${id}`)
}
