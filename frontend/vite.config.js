import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { fileURLToPath, URL } from 'node:url';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': 'http://localhost:5179'
    }
  },
  build: {
    outDir: fileURLToPath(new URL('../ShortUrl/wwwroot', import.meta.url)),
    emptyOutDir: true,
    rollupOptions: {
      output: {
        entryFileNames: 'app.js',
        assetFileNames: 'styles.css'
      }
    }
  }
});
