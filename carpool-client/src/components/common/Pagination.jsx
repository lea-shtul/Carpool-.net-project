import { ChevronLeft, ChevronRight } from 'lucide-react'
import { Button } from './Button'
import styles from './Pagination.module.css'

export function Pagination({ page, totalPages, totalCount, pageSize, onPageChange }) {
  if (totalCount === 0) return null

  const from = (page - 1) * pageSize + 1
  const to = Math.min(page * pageSize, totalCount)

  return (
    <div className={styles.bar}>
      <span className={styles.summary}>
        Showing {from}-{to} of {totalCount}
      </span>
      <div className={styles.controls}>
        <Button
          variant="secondary"
          size="sm"
          icon={ChevronLeft}
          disabled={page <= 1}
          onClick={() => onPageChange(page - 1)}
        >
          Previous
        </Button>
        <span className={styles.pageIndicator}>
          Page {page} of {totalPages}
        </span>
        <Button
          variant="secondary"
          size="sm"
          disabled={page >= totalPages}
          onClick={() => onPageChange(page + 1)}
        >
          Next
          <ChevronRight size={16} />
        </Button>
      </div>
    </div>
  )
}
