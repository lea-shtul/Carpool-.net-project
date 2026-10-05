import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// Proxies /api -> the ASP.NET Core backend so the browser never talks cross-origin
// (avoids any backend CORS changes, per the client spec). `secure: false` lets the
// Node proxy agent accept the local ASP.NET Core HTTPS dev certificate, which is
// self-signed and otherwise fails Node's certificate validation.
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:7276',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
