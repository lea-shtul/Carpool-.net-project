import { apiClient } from './client'

export function createRating(rideId, payload) {
  // { score, comment? }
  return apiClient.post(`/rides/${rideId}/ratings`, payload).then((r) => r.data)
}

export function getRatingsForUser(userId) {
  return apiClient.get(`/users/${userId}/ratings`).then((r) => r.data)
}
