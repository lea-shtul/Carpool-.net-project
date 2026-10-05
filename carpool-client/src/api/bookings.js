import { apiClient } from './client'

export function createBooking(rideId, numberOfSeats) {
  return apiClient.post(`/rides/${rideId}/bookings`, { numberOfSeats }).then((r) => r.data)
}

export function getMyBookings() {
  return apiClient.get('/bookings/my').then((r) => r.data)
}

export function getBooking(id) {
  return apiClient.get(`/bookings/${id}`).then((r) => r.data)
}

export function cancelBooking(id) {
  return apiClient.patch(`/bookings/${id}/cancel`).then((r) => r.data)
}
