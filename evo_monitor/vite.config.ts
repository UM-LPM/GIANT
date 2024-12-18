import { fileURLToPath, URL } from 'node:url'
import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import { quasar, transformAssetUrls } from '@quasar/vite-plugin'
import { resolve } from "path";

// https://vite.dev/config/
export default defineConfig({
  build: {
    lib: {
      entry: resolve(__dirname, "src/main.ts"),
      name: "evo-monitor",
      fileName: "evo-monitor",
    },
    rollupOptions: {
      // make sure to externalize deps that shouldn't be bundled
      // into your library
      external: ['vue','pinia'],
      output: {
        // Provide global variables to use in the UMD build
        // for externalized deps
        globals: {
          vue: "Vue",
          pinia: 'Pinia',
        },
      },
    },
  },
  plugins: [
    vue(),
    vueDevTools(),
    quasar({
      sassVariables: '/src/quasar-variables.sass'
    })
  ],
  server: {
    port: 3000,
  },
  resolve: {
    alias: {
      'pinia': 'pinia/dist/pinia.esm-browser.js',
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
})
