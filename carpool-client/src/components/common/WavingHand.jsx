import styles from './WavingHand.module.css'

/**
 * A small flat-illustration waving hand — replaces the 👋 emoji next to the dashboard
 * greeting with something that matches the app's own visual style, animated with a
 * gentle CSS wave (see WavingHand.module.css).
 */
export function WavingHand({ size = 30 }) {
  return (
    <svg
      className={styles.hand}
      width={size}
      height={size}
      viewBox="0 0 48 48"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      aria-hidden="true"
    >
      {/* Thumb */}
      <rect x="2" y="24" width="18" height="9" rx="4.5" fill="#FDBB4B" transform="rotate(-32 11 28.5)" />

      {/* Palm */}
      <path
        d="M12 26c0-7.2 5.4-12 13-12 7.8 0 13 5 13 12v6c0 7.7-6.3 13-15 13s-15-5.3-15-13z"
        fill="#FFC966"
      />

      {/* Fingers */}
      <rect x="13.5" y="4" width="7.5" height="20" rx="3.75" fill="#FFC966" transform="rotate(-11 17.25 14)" />
      <rect x="20.3" y="1.5" width="7.5" height="22" rx="3.75" fill="#FDBB4B" transform="rotate(-2 24.05 12.5)" />
      <rect x="27" y="2.5" width="7.5" height="21" rx="3.75" fill="#FFC966" transform="rotate(7 30.75 13)" />
      <rect x="33.4" y="6" width="7" height="18" rx="3.5" fill="#FDBB4B" transform="rotate(18 36.9 15)" />

      {/* Palm crease details */}
      <g stroke="#E8A33B" strokeWidth="1.4" strokeLinecap="round" opacity="0.6">
        <path d="M18 31c2 1.6 5 1.8 7 0.4" />
        <path d="M18 36c3 1.8 8 2 11 0.2" />
      </g>
    </svg>
  )
}
