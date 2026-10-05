import { CarFront, Pencil, Trash2, Users } from 'lucide-react'
import { Card } from '../common/Card'
import { Button } from '../common/Button'
import styles from './VehicleCard.module.css'

export function VehicleCard({ vehicle, onEdit, onDelete }) {
  return (
    <Card className={styles.card}>
      <div className={styles.iconWrap}>
        <CarFront size={22} />
      </div>
      <div className={styles.main}>
        <div className={styles.name}>
          {vehicle.manufacturer} {vehicle.model}
        </div>
        <div className={styles.meta}>
          {vehicle.licensePlate} · <Users size={11} style={{ verticalAlign: -2 }} />{' '}
          {vehicle.passengerCapacity} seats
        </div>
      </div>
      <div className={styles.actions}>
        <Button size="sm" variant="secondary" icon={Pencil} onClick={() => onEdit(vehicle)}>
          Edit
        </Button>
        <Button size="sm" variant="danger" icon={Trash2} onClick={() => onDelete(vehicle)}>
          Delete
        </Button>
      </div>
    </Card>
  )
}
