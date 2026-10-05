import { apiClient } from './client'
import { cleanParams } from '../utils/queryParams'

/**
 * GET /api/rides with filtering/sorting/paging (spec §20). All paging happens on the
 * backend (Skip/Take) — the client never fetches everything and paginates locally.
 */
export function searchRides(query) {
  const { page, pageSize, origin, destination, tagId, minPrice, maxPrice, availableOnly, sortBy, sortDirection } =
    query
  return apiClient
    .get('/rides', {
      params: cleanParams({
        page,
        pageSize,
        origin,
        destination,
        tagId,
        minPrice,
        maxPrice,
        availableOnly: availableOnly ? true : undefined,
        sortBy,
        sortDirection,
      }),
    })
    .then((r) => r.data)
}

export function getRide(id) {
  return apiClient.get(`/rides/${id}`).then((r) => r.data)
}

export function createRide(payload) {
  return apiClient.post('/rides', payload).then((r) => r.data)
}

export function cancelRide(id) {
  return apiClient.patch(`/rides/${id}/cancel`).then((r) => r.data)
}

export function completeRide(id) {
  return apiClient.patch(`/rides/${id}/complete`).then((r) => r.data)
}
