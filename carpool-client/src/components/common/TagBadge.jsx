import { Lock } from 'lucide-react'
import { Badge } from './Badge'

/**
 * Displays one ride tag. Global and private tags are visually distinct (per the
 * backend's global/private tags extension) so a driver can tell at a glance which
 * tags are shared and which are their own.
 */
export function TagBadge({ tag }) {
  return (
    <Badge tone={tag.isPrivate ? 'private' : 'global'} icon={tag.isPrivate ? Lock : undefined}>
      {tag.name}
    </Badge>
  )
}
