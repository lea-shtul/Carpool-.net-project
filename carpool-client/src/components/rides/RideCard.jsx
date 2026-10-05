import { useNavigate } from 'react-router-dom'
import { Button } from '../common/Button'
import { RideStatusBadge } from '../common/Badge'
import { TagBadge } from '../common/TagBadge'
import { Avatar } from '../common/Avatar'
import { formatDate, formatTime, formatCurrency } from '../../utils/format'
import styles from './RideCard.module.css'

export function RideCard({ ride }) {
  const navigate = useNavigate()

  return (
    <div className={styles.card}>
      <div className={styles.timeCol}>
        <div className={styles.time}>{formatTime(ride.departureTime)}</div>
        <div className={styles.date}>{formatDate(ride.departureTime)}</div>
      </div>

      <div className={styles.routeCol}>
        <div className={styles.routeLine}>
          <span className={styles.dotOrigin} />
          {ride.origin}
        </div>
        <div className={styles.routeLine}>
          <span className={styles.dotDestination} />
          {ride.destination}
        </div>
        {ride.tags?.length > 0 && (
          <div className={styles.tagRow}>
            {ride.tags.map((tag) => (
              <TagBadge key={tag.id} tag={tag} />
            ))}
          </div>
        )}
      </div>

      <div className={styles.driverCol}>
        <div className={styles.colLabel}>Driver</div>
        <div className={styles.driver}>
          <Avatar firstName={ride.driver.firstName} lastName={ride.driver.lastName} size={30} />
          <div>
            <div className={styles.driverName}>
              {ride.driver.firstName} {ride.driver.lastName}
            </div>
            <div className={styles.vehicleText}>
              {ride.vehicle.manufacturer} {ride.vehicle.model}
            </div>
          </div>
        </div>
      </div>

      <div className={styles.seatsCol}>
        <div className={styles.colLabel}>Seats</div>
        <div className={styles.seatsValue}>{ride.availableSeats}</div>
        <div className={styles.seatsOf}>of {ride.totalSeats}</div>
      </div>

      <div className={styles.priceCol}>
        <div className={styles.colLabel}>Per seat</div>
        <div className={styles.priceValue}>{formatCurrency(ride.pricePerSeat)}</div>
      </div>

      <div className={styles.actionCol}>
        <RideStatusBadge status={ride.status} />
        <Button size="sm" onClick={() => navigate(`/rides/${ride.id}`)}>
          View Details
        </Button>
      </div>
    </div>
  )
}
