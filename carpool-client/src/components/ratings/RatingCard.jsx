import { RatingStars } from '../common/RatingStars'
import { formatDate } from '../../utils/format'
import styles from './RatingCard.module.css'

export function RatingCard({ rating }) {
  return (
    <div className={styles.card}>
      <div className={styles.header}>
        <RatingStars score={rating.score} />
        <span className={styles.meta}>
          Ride #{rating.rideId} · {formatDate(rating.createdAt)}
        </span>
      </div>
      {rating.comment && <div className={styles.comment}>"{rating.comment}"</div>}
    </div>
  )
}
