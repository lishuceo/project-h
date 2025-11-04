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
    host: '0.0.0.0', // 监听所有网络接口（允许局域网访问）
    port: 3005,
    open: true,
  },
  build: {
    target: 'es2020',
    outDir: 'dist',
    assetsDir: 'assets',
    sourcemap: true,
  },
  test: {
    globals: true,
    environment: 'happy-dom',
    coverage: {
      provider: 'v8',
      reporter: ['text', 'json', 'html'],
    },
  },
});

