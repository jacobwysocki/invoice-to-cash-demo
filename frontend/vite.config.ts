import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// The dev server proxies /api -> the .NET backend so the frontend
// can call the API without CORS friction and without hardcoding the port.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5088',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, ''),
      },
    },
  },
});
