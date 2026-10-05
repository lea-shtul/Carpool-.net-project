import { useEffect, useState } from 'react'
import { getAllUsers, setUserStatus } from '../api/users'
import { Badge } from '../components/common/Badge'
import { Button } from '../components/common/Button'
import { Card } from '../components/common/Card'
import { ErrorState, LoadingState } from '../components/common/States'
import { useApiErrorToast, useToast } from '../hooks/useToast'
import styles from './AdminUsersPage.module.css'

export function AdminUsersPage() {
  const { showToast } = useToast()
  const showApiError = useApiErrorToast()
  const [users, setUsers] = useState(null)
  const [error, setError] = useState(null)
  const [togglingId, setTogglingId] = useState(null)

  function load() {
    setError(null)
    getAllUsers()
      .then(setUsers)
      .catch((err) => setError(err))
  }

  useEffect(load, [])

  async function toggleStatus(u) {
    setTogglingId(u.id)
    try {
      await setUserStatus(u.id, !u.isActive)
      showToast({ type: 'success', message: `${u.firstName} ${u.lastName} is now ${!u.isActive ? 'active' : 'inactive'}.` })
      load()
    } catch (err) {
      showApiError(err)
    } finally {
      setTogglingId(null)
    }
  }

  return (
    <div>
      <h1 style={{ fontSize: 22, fontWeight: 800, marginBottom: 'var(--space-5)' }}>Admin · Users</h1>

      <Card>
        {users === null && !error && <LoadingState count={3} />}
        {error && <ErrorState message={error.message} correlationId={error.correlationId} onRetry={load} />}

        {users && (
          <div>
            <div className={`${styles.row} ${styles.headerRow}`}>
              <span>Name</span>
              <span>Email</span>
              <span>Role</span>
              <span>Status</span>
              <span />
            </div>
            {users.map((u) => (
              <div className={styles.row} key={u.id}>
                <span>
                  {u.firstName} {u.lastName}
                </span>
                <span>{u.email}</span>
                <span>
                  <Badge tone={u.role === 'Admin' ? 'info' : 'neutral'}>{u.role}</Badge>
                </span>
                <span>
                  <Badge tone={u.isActive ? 'success' : 'danger'}>{u.isActive ? 'Active' : 'Inactive'}</Badge>
                </span>
                <Button
                  size="sm"
                  variant="secondary"
                  isLoading={togglingId === u.id}
                  onClick={() => toggleStatus(u)}
                >
                  {u.isActive ? 'Deactivate' : 'Activate'}
                </Button>
              </div>
            ))}
          </div>
        )}
      </Card>
    </div>
  )
}
