import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      // Proxy API calls to backend (adjusted to actual dev HTTP port 52244)
      '/api': {
        // Use env var if injected (VITE_API_PROXY) else default to backend HTTP port
        target: (globalThis as any).process?.env?.VITE_API_PROXY || 'http://localhost:52244',
        changeOrigin: true,
        secure: false
      }
    }
  }
});
