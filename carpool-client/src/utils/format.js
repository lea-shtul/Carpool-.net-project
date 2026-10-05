const dateFormatter = new Intl.DateTimeFormat('en-US', { month: 'short', day: 'numeric', year: 'numeric' })
const timeFormatter = new Intl.DateTimeFormat('en-US', { hour: '2-digit', minute: '2-digit', hour12: false })

export function formatDate(isoString) {
  if (!isoString) return ''
  return dateFormatter.format(new Date(isoString))
}

export function formatTime(isoString) {
  if (!isoString) return ''
  return timeFormatter.format(new Date(isoString))
}

export function formatDateTime(isoString) {
  if (!isoString) return ''
  return `${formatDate(isoString)} · ${formatTime(isoString)}`
}

export function formatCurrency(amount) {
  if (amount === null || amount === undefined) return ''
  return `₪${Number(amount).toFixed(2)}`
}

export function formatDuration(minutes) {
  if (!minutes && minutes !== 0) return ''
  const h = Math.floor(minutes / 60)
  const m = minutes % 60
  if (h === 0) return `${m} min`
  if (m === 0) return `${h} hr`
  return `${h} hr ${m} min`
}
