import { Search } from 'lucide-react'
import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Button } from '../common/Button'
import { FormField, Select, TextInput } from '../common/FormField'
import styles from './SearchPanel.module.css'

/**
 * Dashboard quick-search card. The reference image has a "Departure date" field here,
 * but GET /api/rides has no date filter (only origin, destination, tagId, price range,
 * availableOnly, sortBy/sortDirection) — inventing one would silently do nothing or lie
 * about results, so this uses the real sortBy instead, defaulting to soonest departure.
 */
export function SearchPanel() {
  const navigate = useNavigate()
  const [origin, setOrigin] = useState('')
  const [destination, setDestination] = useState('')
  const [sortBy, setSortBy] = useState('departureTime')

  function handleSubmit(e) {
    e.preventDefault()
    const params = new URLSearchParams()
    if (origin) params.set('origin', origin)
    if (destination) params.set('destination', destination)
    params.set('sortBy', sortBy)
    params.set('sortDirection', 'asc')
    params.set('page', '1')
    navigate(`/rides?${params.toString()}`)
  }

  return (
    <form className={styles.grid} onSubmit={handleSubmit}>
      <FormField label="From" htmlFor="search-from">
        <TextInput
          id="search-from"
          placeholder="Tel Aviv, Israel"
          value={origin}
          onChange={(e) => setOrigin(e.target.value)}
        />
      </FormField>
      <FormField label="To" htmlFor="search-to">
        <TextInput
          id="search-to"
          placeholder="Haifa, Israel"
          value={destination}
          onChange={(e) => setDestination(e.target.value)}
        />
      </FormField>
      <FormField label="Sort by" htmlFor="search-sort">
        <Select id="search-sort" value={sortBy} onChange={(e) => setSortBy(e.target.value)}>
          <option value="departureTime">Departure time</option>
          <option value="price">Price</option>
          <option value="availableSeats">Available seats</option>
        </Select>
      </FormField>
      <Button type="submit" icon={Search}>
        Search Rides
      </Button>
    </form>
  )
}
