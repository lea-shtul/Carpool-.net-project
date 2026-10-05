/** Drops undefined/null/'' entries so unset filters are never sent as empty query params. */
export function cleanParams(params) {
  const out = {}
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined && value !== null && value !== '') {
      out[key] = value
    }
  }
  return out
}
