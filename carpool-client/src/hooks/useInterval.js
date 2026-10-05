import { useEffect, useRef } from 'react'

/**
 * Declarative setInterval: re-runs the latest `callback` every `delayMs`, without
 * restarting the timer just because `callback` is a new closure each render. Pass
 * `delayMs: null` to pause.
 */
export function useInterval(callback, delayMs) {
  const savedCallback = useRef(callback)

  useEffect(() => {
    savedCallback.current = callback
  }, [callback])

  useEffect(() => {
    if (delayMs === null) return undefined
    const id = setInterval(() => savedCallback.current(), delayMs)
    return () => clearInterval(id)
  }, [delayMs])
}
