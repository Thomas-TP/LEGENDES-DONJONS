import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

export default defineConfig({
  plugins: [react()],
  // 🚀 Important pour GitHub Pages: le chemin de base est le nom du repo
  base: '/LEGENDES-DONJONS/',
  
  build: {
    // Note: Pour une publication GitHub Pages, on va builder dans un dossier dist séparé
    // Dans le cas d'un deploiement avec le backend .NET, on utilise ../wwwroot
    // Ici on garde dist pour GitHub Pages
    outDir: 'dist', 
    emptyOutDir: true,
  },
  server: {
    proxy: {
      '/api': 'http://localhost:5100'
    }
  }
})
