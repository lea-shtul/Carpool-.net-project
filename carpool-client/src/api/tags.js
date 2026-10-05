import { apiClient } from './client'

/** Global tags + the caller's own private tags (spec extension — see backend SPEC-COMPLIANCE.md). */
export function getTags() {
  return apiClient.get('/tags').then((r) => r.data)
}

/** Creates a private tag owned by the caller. */
export function createTag(name) {
  return apiClient.post('/tags', { name }).then((r) => r.data)
}
