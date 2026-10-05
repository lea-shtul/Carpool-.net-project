import { Plus } from 'lucide-react'
import { useEffect, useState } from 'react'
import { deleteVehicle, getMyVehicles } from '../api/vehicles'
import { Button } from '../components/common/Button'
import { ConfirmDialog } from '../components/common/ConfirmDialog'
import { EmptyState, ErrorState, LoadingState } from '../components/common/States'
import { VehicleCard } from '../components/vehicles/VehicleCard'
import { VehicleForm } from '../components/vehicles/VehicleForm'
import { useApiErrorToast, useToast } from '../hooks/useToast'

export function MyVehiclesPage() {
  const { showToast } = useToast()
  const showApiError = useApiErrorToast()
  const [vehicles, setVehicles] = useState(null)
  const [error, setError] = useState(null)
  const [editing, setEditing] = useState(null) // null = closed, {} = new, vehicle = edit
  const [pendingDelete, setPendingDelete] = useState(null)
  const [isDeleting, setIsDeleting] = useState(false)

  function load() {
    setError(null)
    getMyVehicles()
      .then(setVehicles)
      .catch((err) => setError(err))
  }

  useEffect(load, [])

  function handleSaved() {
    showToast({ type: 'success', message: `Vehicle ${editing?.id ? 'updated' : 'added'}.` })
    setEditing(null)
    load()
  }

  async function confirmDelete() {
    setIsDeleting(true)
    try {
      await deleteVehicle(pendingDelete.id)
      showToast({ type: 'success', message: 'Vehicle deleted.' })
      setPendingDelete(null)
      load()
    } catch (err) {
      // Surfaces the backend's "referenced by a ride" 409 verbatim.
      showApiError(err)
      setPendingDelete(null)
    } finally {
      setIsDeleting(false)
    }
  }

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 'var(--space-5)' }}>
        <h1 style={{ fontSize: 22, fontWeight: 800 }}>My Vehicles</h1>
        <Button icon={Plus} onClick={() => setEditing({})}>
          Add Vehicle
        </Button>
      </div>

      {vehicles === null && !error && <LoadingState count={2} />}
      {error && <ErrorState message={error.message} correlationId={error.correlationId} onRetry={load} />}
      {vehicles && vehicles.length === 0 && (
        <EmptyState
          title="No vehicles yet"
          subtitle="Add a vehicle to start offering rides."
          action={
            <Button size="sm" onClick={() => setEditing({})}>
              Add Vehicle
            </Button>
          }
        />
      )}

      <div style={{ display: 'flex', flexDirection: 'column', gap: 'var(--space-4)' }}>
        {vehicles?.map((vehicle) => (
          <VehicleCard
            key={vehicle.id}
            vehicle={vehicle}
            onEdit={setEditing}
            onDelete={setPendingDelete}
          />
        ))}
      </div>

      {editing && (
        <VehicleForm
          vehicle={editing.id ? editing : null}
          onClose={() => setEditing(null)}
          onSaved={handleSaved}
        />
      )}

      {pendingDelete && (
        <ConfirmDialog
          title="Delete this vehicle?"
          message={`${pendingDelete.manufacturer} ${pendingDelete.model} (${pendingDelete.licensePlate}) will be permanently removed.`}
          confirmLabel="Delete"
          tone="danger"
          isLoading={isDeleting}
          onConfirm={confirmDelete}
          onCancel={() => setPendingDelete(null)}
        />
      )}
    </div>
  )
}
