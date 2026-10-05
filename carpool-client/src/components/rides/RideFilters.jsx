import { SlidersHorizontal } from 'lucide-react'
import { Button } from '../common/Button'
import { FormField, Select, TextInput } from '../common/FormField'
import styles from './RideFilters.module.css'

/**
 * Full filter/sort bar for the Find Rides page. Every field maps 1:1 to a real
 * RideQueryParameters field — no "Date" or exact-seats pill here, since the backend
 * supports neither (only availableOnly, a boolean "still bookable" filter).
 */
export function RideFilters({ filters, tags, onChange, onSubmit }) {
  function set(key, value) {
    onChange({ ...filters, [key]: value })
  }

  return (
    <form
      onSubmit={(e) => {
        e.preventDefault()
        onSubmit()
      }}
    >
      <div className={styles.grid}>
        <FormField label="From">
          <TextInput
            placeholder="Origin"
            value={filters.origin}
            onChange={(e) => set('origin', e.target.value)}
          />
        </FormField>
        <FormField label="To">
          <TextInput
            placeholder="Destination"
            value={filters.destination}
            onChange={(e) => set('destination', e.target.value)}
          />
        </FormField>
        <FormField label="Tag">
          <Select value={filters.tagId} onChange={(e) => set('tagId', e.target.value)}>
            <option value="">Any</option>
            {tags.map((tag) => (
              <option key={tag.id} value={tag.id}>
                {tag.name}
                {tag.isPrivate ? ' (private)' : ''}
              </option>
            ))}
          </Select>
        </FormField>
        <FormField label="Sort by">
          <Select value={filters.sortBy} onChange={(e) => set('sortBy', e.target.value)}>
            <option value="departureTime">Departure time</option>
            <option value="price">Price</option>
            <option value="availableSeats">Available seats</option>
          </Select>
        </FormField>
      </div>

      <div className={styles.row2}>
        <FormField label="Min price">
          <TextInput
            type="number"
            min="0"
            placeholder="Any"
            value={filters.minPrice}
            onChange={(e) => set('minPrice', e.target.value)}
          />
        </FormField>
        <FormField label="Max price">
          <TextInput
            type="number"
            min="0"
            placeholder="Any"
            value={filters.maxPrice}
            onChange={(e) => set('maxPrice', e.target.value)}
          />
        </FormField>
        <FormField label="Direction">
          <Select value={filters.sortDirection} onChange={(e) => set('sortDirection', e.target.value)}>
            <option value="asc">Ascending</option>
            <option value="desc">Descending</option>
          </Select>
        </FormField>
        <div className={styles.checkboxField}>
          <input
            type="checkbox"
            id="available-only"
            checked={filters.availableOnly}
            onChange={(e) => set('availableOnly', e.target.checked)}
          />
          <label htmlFor="available-only">Available only</label>
        </div>
        <Button type="submit" icon={SlidersHorizontal}>
          Apply Filters
        </Button>
      </div>
    </form>
  )
}
