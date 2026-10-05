import { Star } from 'lucide-react'

/**
 * Read-only star display (score out of 5).
 * Pass `onChange` to make it an interactive picker (used by the rating form).
 */
export function RatingStars({ score, size = 16, onChange }) {
  const interactive = Boolean(onChange)
  return (
    <span style={{ display: 'inline-flex', gap: 2 }}>
      {[1, 2, 3, 4, 5].map((n) => (
        <Star
          key={n}
          size={size}
          fill={n <= score ? '#f5b014' : 'none'}
          color={n <= score ? '#f5b014' : 'var(--color-border-strong)'}
          style={interactive ? { cursor: 'pointer' } : undefined}
          onClick={interactive ? () => onChange(n) : undefined}
        />
      ))}
    </span>
  )
}
