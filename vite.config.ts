import { defineConfig } from 'vite';
import { resolve } from 'path';

export default defineConfig({
  base: './', // 使用相对路径，支持任意路径部署
  resolve: {
    alias: {
      '@': resolve(__dirname, './src'),
    },
  },
  server: {
    port: 3000,
    open: true,
  },
  build: {
    target: 'es2020',
    outDir: 'dist',
    assetsDir: 'assets',
    sourcemap: true,
  },
});

