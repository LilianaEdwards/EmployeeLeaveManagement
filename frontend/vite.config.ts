
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    proxy: {
      // EmployeeService: authentication and employee endpoints
      '/api': {
        target: 'http://localhost:5076',
        changeOrigin: true,
      },

      // LeaveService: leave requests and balance endpoints
      '/leave-api': {
        target: 'http://localhost:5080',
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/leave-api/, '/api'),
      },
    },
  },
})
