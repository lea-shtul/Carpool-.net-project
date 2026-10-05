import { Lock, Plus } from 'lucide-react'
import { useEffect, useState } from 'react'
import { createTag, getTags } from '../../api/tags'
import { LoadingState } from '../common/States'
import styles from './TagPicker.module.css'

/**
 * Tag selector for Create Ride. Shows global tags and the driver's own private tags as
 * visually distinct chip groups (backend's global/private tags extension), and lets the
 * driver create a new private tag inline without leaving the form.
 */
export function TagPicker({ selectedIds, onChange }) {
  const [tags, setTags] = useState(null)
  const [newTagName, setNewTagName] = useState('')
  const [addError, setAddError] = useState('')
  const [isAdding, setIsAdding] = useState(false)

  useEffect(() => {
    getTags().then(setTags).catch(() => setTags([]))
  }, [])

  function toggle(id) {
    onChange(selectedIds.includes(id) ? selectedIds.filter((x) => x !== id) : [...selectedIds, id])
  }

  async function handleAddTag(e) {
    e.preventDefault()
    e.stopPropagation()
    const name = newTagName.trim()
    if (!name) return
    setIsAdding(true)
    setAddError('')
    try {
      const tag = await createTag(name)
      setTags((prev) => [...prev, tag])
      onChange([...selectedIds, tag.id])
      setNewTagName('')
    } catch (err) {
      setAddError(err.message)
    } finally {
      setIsAdding(false)
    }
  }

  if (tags === null) return <LoadingState count={1} />

  const globalTags = tags.filter((t) => !t.isPrivate)
  const privateTags = tags.filter((t) => t.isPrivate)

  return (
    <div className={styles.wrapper}>
      <div className={styles.group}>
        <div className={styles.groupLabel}>Global tags</div>
        <div className={styles.chipRow}>
          {globalTags.map((tag) => (
            <button
              key={tag.id}
              type="button"
              className={`${styles.chip} ${selectedIds.includes(tag.id) ? styles.chipSelected : ''}`}
              onClick={() => toggle(tag.id)}
            >
              {tag.name}
            </button>
          ))}
        </div>
      </div>

      <div className={styles.group}>
        <div className={styles.groupLabel}>Your private tags</div>
        {privateTags.length === 0 && (
          <div className={styles.emptyHint}>None yet — add one below, only you can use it.</div>
        )}
        <div className={styles.chipRow}>
          {privateTags.map((tag) => (
            <button
              key={tag.id}
              type="button"
              className={`${styles.chip} ${styles.chipPrivate} ${
                selectedIds.includes(tag.id) ? styles.chipPrivateSelected : ''
              }`}
              onClick={() => toggle(tag.id)}
            >
              <Lock size={11} />
              {tag.name}
            </button>
          ))}
        </div>

        {/* A plain div, not a <form> — this already lives inside CreateRideForm's
            outer <form>, and nested <form> elements are invalid HTML (the browser
            would silently misattribute this button's submit to the outer form). */}
        <div className={styles.addRow}>
          <input
            className={styles.addInput}
            placeholder="New private tag, e.g. Quiet ride"
            maxLength={50}
            value={newTagName}
            onChange={(e) => setNewTagName(e.target.value)}
            onKeyDown={(e) => {
              if (e.key === 'Enter') handleAddTag(e)
            }}
          />
          <button type="button" className={styles.chip} onClick={handleAddTag} disabled={isAdding || !newTagName.trim()}>
            <Plus size={12} />
            Add
          </button>
        </div>
        {addError && <div className={styles.addError}>{addError}</div>}
      </div>
    </div>
  )
}
