import { useState } from 'react'
import { createRating } from '../../api/ratings'
import { Button } from '../common/Button'
import { FormField, Textarea } from '../common/FormField'
import { RatingStars } from '../common/RatingStars'
import { useToast } from '../../hooks/useToast'

export function RatingForm({ rideId, onRated }) {
  const { showToast } = useToast()
  const [score, setScore] = useState(5)
  const [comment, setComment] = useState('')
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    setIsSubmitting(true)
    try {
      const rating = await createRating(rideId, { score, comment: comment || undefined })
      showToast({ type: 'success', message: 'Thanks — your rating was submitted.' })
      onRated?.(rating)
    } catch (err) {
      // Surfaces "already rated" (409) and eligibility (400) messages verbatim.
      setError(err.message)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
      <FormField label="Score">
        <RatingStars score={score} size={24} onChange={setScore} />
      </FormField>
      <FormField label="Comment (optional)">
        <Textarea
          maxLength={500}
          placeholder="How was the ride?"
          value={comment}
          onChange={(e) => setComment(e.target.value)}
        />
      </FormField>
      {error && <p style={{ color: 'var(--color-danger-text)', fontSize: 13 }}>{error}</p>}
      <div>
        <Button type="submit" isLoading={isSubmitting}>
          Submit Rating
        </Button>
      </div>
    </form>
  )
}
