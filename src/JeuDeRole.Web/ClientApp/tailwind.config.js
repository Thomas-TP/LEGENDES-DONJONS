/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{js,ts,jsx,tsx}"],
  theme: {
    extend: {
      colors: {
        rpg: {
          dark: '#0f0e17',
          panel: '#1a1a2e',
          border: '#3a3a5c',
          gold: '#f5c542',
          red: '#e74c3c',
          green: '#2ecc71',
          blue: '#3498db',
          purple: '#9b59b6',
          mana: '#5dade2',
        }
      },
      fontFamily: {
        rpg: ['"Press Start 2P"', 'monospace'],
        body: ['Inter', 'system-ui', 'sans-serif'],
      },
      animation: {
        'pulse-slow': 'pulse 3s infinite',
        'shake': 'shake 0.5s ease-in-out',
      },
      keyframes: {
        shake: {
          '0%, 100%': { transform: 'translateX(0)' },
          '25%': { transform: 'translateX(-4px)' },
          '75%': { transform: 'translateX(4px)' },
        }
      }
    },
  },
  plugins: [],
}
