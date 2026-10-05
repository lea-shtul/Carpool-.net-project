import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { getMyVehicles } from '../../api/vehicles'
import { createRide } from '../../api/rides'
import { Button } from '../common/Button'
import { FormField, Select, TextInput } from '../common/FormField'
import { EmptyState, LoadingState } from '../common/States'
import { useToast } from '../../hooks/useToast'
import { TagPicker } from './TagPicker'
import styles from './CreateRideForm.module.css'
import { CarFront } from 'lucide-react'

/** `min` for the datetime-local input — "now" in the input's own YYYY-MM-DDTHH:mm format. */
function nowAsDateTimeLocal() {
  const d = new Date(Date.now() - new Date().getTimezoneOffset() * 60000)
  return d.toISOString().slice(0, 16)
}

const EMPTY_FORM = {
  vehicleId: '',
  origin: '',
  destination: '',
  departureTime: '',
  totalSeats: 1,
  pricePerSeat: '',
  estimatedDurationMinutes: '',
}

export function CreateRideForm() {
  const navigate = useNavigate()
  const { showToast } = useToast()
  const [vehicles, setVehicles] = useState(null)
  const [form, setForm] = useState(EMPTY_FORM)
  const [tagIds, setTagIds] = useState([])
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  useEffect(() => {
    getMyVehicles()
      .then((data) => {
        setVehicles(data)
        if (data.length > 0) setForm((f) => ({ ...f, vehicleId: data[0].id }))
      })
      .catch(() => setVehicles([]))
  }, [])

  function set(key, value) {
    setForm((f) => ({ ...f, [key]: value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')

    // datetime-local has no timezone; interpret it in the browser's local time and
    // convert to the UTC instant the backend expects (spec: DepartureTime is UTC).
    const departureTime = new Date(form.departureTime).toISOString()

    // Convenience check mirroring the backend's rule (RideService.CreateAsync) — catches
    // it before a round trip, but the backend remains the authoritative check.
    if (new Date(departureTime) <= new Date()) {
      setError('Departure time must be in the future.')
      return
    }

    setIsSubmitting(true)
    try {
      const ride = await createRide({
        vehicleId: Number(form.vehicleId),
        origin: form.origin,
        destination: form.destination,
        departureTime,
        totalSeats: Number(form.totalSeats),
        pricePerSeat: Number(form.pricePerSeat),
        estimatedDurationMinutes: Number(form.estimatedDurationMinutes),
        tagIds,
      })
      showToast({ type: 'success', message: 'Ride created — you are now offering this ride.' })
      navigate(`/rides/${ride.id}`)
    } catch (err) {
      setError(err.message)
    } finally {
      setIsSubmitting(false)
    }
  }

  if (vehicles === null) return <LoadingState count={2} />

  if (vehicles.length === 0) {
    return (
      <EmptyState
        icon={CarFront}
        title="Add a vehicle first"
        subtitle="You need at least one registered vehicle before you can offer a ride."
        action={
          <Button onClick={() => navigate('/vehicles')} size="sm">
            Go to My Vehicles
          </Button>
        }
      />
    )
  }

  const selectedVehicle = vehicles.find((v) => v.id === Number(form.vehicleId))

  return (
    <form onSubmit={handleSubmit}>
      <div className={styles.grid}>
        <FormField label="Vehicle">
          <Select value={form.vehicleId} onChange={(e) => set('vehicleId', e.target.value)}>
            {vehicles.map((v) => (
              <option key={v.id} value={v.id}>
                {v.manufacturer} {v.model} ({v.licensePlate}) · {v.passengerCapacity} seats
              </option>
            ))}
          </Select>
        </FormField>

        <FormField label="Total seats offered">
          <TextInput
            type="number"
            min={1}
            max={selectedVehicle?.passengerCapacity || 20}
            required
            value={form.totalSeats}
            onChange={(e) => set('totalSeats', e.target.value)}
          />
        </FormField>

        <FormField label="From">
          <TextInput
            required
            placeholder="Tel Aviv, Israel"
            value={form.origin}
            onChange={(e) => set('origin', e.target.value)}
          />
        </FormField>

        <FormField label="To">
          <TextInput
            required
            placeholder="Haifa, Israel"
            value={form.destination}
            onChange={(e) => set('destination', e.target.value)}
          />
        </FormField>

        <FormField label="Departure date &amp; time">
          <TextInput
            type="datetime-local"
            required
            min={nowAsDateTimeLocal()}
            value={form.departureTime}
            onChange={(e) => set('departureTime', e.target.value)}
          />
        </FormField>

        <FormField label="Estimated duration (minutes)">
          <TextInput
            type="number"
            min={1}
            max={1440}
            required
            value={form.estimatedDurationMinutes}
            onChange={(e) => set('estimatedDurationMinutes', e.target.value)}
          />
        </FormField>

        <FormField label="Price per seat">
          <TextInput
            type="number"
            min={0}
            step="0.01"
            required
            value={form.pricePerSeat}
            onChange={(e) => set('pricePerSeat', e.target.value)}
          />
        </FormField>

        <div className={styles.span2}>
          <FormField label="Tags">
            <TagPicker selectedIds={tagIds} onChange={setTagIds} />
          </FormField>
        </div>
      </div>

      {error && <p style={{ color: 'var(--color-danger-text)', fontSize: 13, marginTop: 'var(--space-4)' }}>{error}</p>}

      <div className={styles.actions}>
        <Button type="submit" isLoading={isSubmitting}>
          Offer This Ride
        </Button>
      </div>
    </form>
  )
}
