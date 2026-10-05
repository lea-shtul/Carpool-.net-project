/**
 * Decorative scene for the dashboard's promo banner — clouds, a small skyline, trees and a
 * car — echoing the reference design's hero illustration. Hand-drawn inline SVG (not a
 * cropped screenshot) so it stays crisp at any size and its colors match the app's palette.
 */
export function RoadTripIllustration({ className }) {
  return (
    <svg
      className={className}
      viewBox="0 0 320 170"
      fill="none"
      xmlns="http://www.w3.org/2000/svg"
      aria-hidden="true"
    >
      {/* Clouds */}
      <g opacity="0.8" fill="#ffffff">
        <ellipse cx="60" cy="28" rx="26" ry="12" />
        <ellipse cx="80" cy="22" rx="18" ry="10" />
        <ellipse cx="250" cy="20" rx="22" ry="10" />
        <ellipse cx="268" cy="27" rx="15" ry="8" />
      </g>

      {/* Skyline */}
      <g opacity="0.55">
        <rect x="18" y="70" width="22" height="60" rx="2" fill="#9db7ef" />
        <rect x="44" y="50" width="26" height="80" rx="2" fill="#7fa0e8" />
        <rect x="74" y="85" width="18" height="45" rx="2" fill="#aec3f0" />
        <rect x="226" y="60" width="20" height="70" rx="2" fill="#aec3f0" />
        <rect x="250" y="80" width="24" height="50" rx="2" fill="#7fa0e8" />
        <rect x="278" y="95" width="16" height="35" rx="2" fill="#9db7ef" />
        {/* windows */}
        <g fill="#eef3ff">
          <rect x="22" y="78" width="4" height="4" />
          <rect x="30" y="78" width="4" height="4" />
          <rect x="22" y="90" width="4" height="4" />
          <rect x="30" y="90" width="4" height="4" />
          <rect x="50" y="60" width="4" height="4" />
          <rect x="60" y="60" width="4" height="4" />
          <rect x="50" y="72" width="4" height="4" />
          <rect x="60" y="72" width="4" height="4" />
          <rect x="256" y="90" width="4" height="4" />
          <rect x="266" y="90" width="4" height="4" />
        </g>
      </g>

      {/* Trees */}
      <g>
        <rect x="104" y="112" width="5" height="18" rx="1.5" fill="#a9794f" />
        <circle cx="106.5" cy="104" r="14" fill="#8fcf9a" />
        <circle cx="96" cy="112" r="10" fill="#7cc289" />
        <circle cx="118" cy="112" r="10" fill="#7cc289" />

        <rect x="206" y="118" width="4" height="14" rx="1.5" fill="#a9794f" />
        <circle cx="208" cy="112" r="11" fill="#8fcf9a" />
      </g>

      {/* Road */}
      <rect x="0" y="134" width="320" height="4" fill="#c7d2ea" />
      <g fill="#ffffff">
        <rect x="10" y="135.2" width="14" height="2" rx="1" />
        <rect x="40" y="135.2" width="14" height="2" rx="1" />
        <rect x="70" y="135.2" width="14" height="2" rx="1" />
        <rect x="240" y="135.2" width="14" height="2" rx="1" />
        <rect x="270" y="135.2" width="14" height="2" rx="1" />
        <rect x="300" y="135.2" width="14" height="2" rx="1" />
      </g>

      {/* Car */}
      <g transform="translate(140 102)">
        <rect x="0" y="16" width="66" height="18" rx="6" fill="#3366f3" />
        <path d="M8 16 C10 5 20 0 33 0 C46 0 54 5 58 16 Z" fill="#3366f3" />
        <path d="M14 15 C16 8 22 4 33 4 C42 4 48 8 51 15 Z" fill="#dce7ff" />
        <circle cx="16" cy="36" r="7" fill="#101832" />
        <circle cx="50" cy="36" r="7" fill="#101832" />
        <circle cx="16" cy="36" r="3" fill="#aab4cf" />
        <circle cx="50" cy="36" r="3" fill="#aab4cf" />
        <rect x="-3" y="22" width="6" height="4" rx="2" fill="#ffd166" />
        <rect x="63" y="22" width="6" height="4" rx="2" fill="#e8513f" />
      </g>
    </svg>
  )
}
