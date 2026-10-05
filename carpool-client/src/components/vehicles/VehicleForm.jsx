import { useState } from 'react'
import { createVehicle, updateVehicle } from '../../api/vehicles'
import { Button } from '../common/Button'
import { FormField, TextInput } from '../common/FormField'
import { Modal } from '../common/Modal'

const emptyForm = { manufacturer: '', model: '', licensePlate: '', passengerCapacity: 4 }

export function VehicleForm({ vehicle, onClose, onSaved }) {
  const isEdit = Boolean(vehicle)
  const [form, setForm] = useState(
    vehicle
      ? {
          manufacturer: vehicle.manufacturer,
          model: vehicle.model,
          licensePlate: vehicle.licensePlate,
          passengerCapacity: vehicle.passengerCapacity,
        }
      : emptyForm,
  )
  const [error, setError] = useState('')
  const [isSubmitting, setIsSubmitting] = useState(false)

  function set(key, value) {
    setForm((f) => ({ ...f, [key]: value }))
  }

  async function handleSubmit(e) {
    e.preventDefault()
    setError('')
    setIsSubmitting(true)
    try {
      const payload = { ...form, passengerCapacity: Number(form.passengerCapacity) }
      const saved = isEdit ? await updateVehicle(vehicle.id, payload) : await createVehicle(payload)
      onSaved(saved)
    } catch (err) {
      // Surfaces the backend's duplicate-license-plate 409 message verbatim.
      setError(err.message)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <Modal title={isEdit ? 'Edit Vehicle' : 'Add Vehicle'} onClose={onClose}>
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
        <FormField label="Manufacturer">
          <TextInput
            required
            maxLength={60}
            value={form.manufacturer}
            onChange={(e) => set('manufacturer', e.target.value)}
          />
        </FormField>
        <FormField label="Model">
          <TextInput required maxLength={60} value={form.model} onChange={(e) => set('model', e.target.value)} />
        </FormField>
        <FormField label="License plate">
          <TextInput
            required
            maxLength={20}
            value={form.licensePlate}
            onChange={(e) => set('licensePlate', e.target.value)}
          />
        </FormField>
        <FormField label="Passenger capacity">
          <TextInput
            type="number"
            min={1}
            max={20}
            required
            value={form.passengerCapacity}
            onChange={(e) => set('passengerCapacity', e.target.value)}
          />
        </FormField>

        {error && <p style={{ color: 'var(--color-danger-text)', fontSize: 13 }}>{error}</p>}

        <div style={{ display: 'flex', justifyContent: 'flex-end', gap: 'var(--space-2)', marginTop: 'var(--space-2)' }}>
          <Button type="button" variant="secondary" onClick={onClose} disabled={isSubmitting}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isSubmitting}>
            {isEdit ? 'Save Changes' : 'Add Vehicle'}
          </Button>
        </div>
      </form>
    </Modal>
  )
}
