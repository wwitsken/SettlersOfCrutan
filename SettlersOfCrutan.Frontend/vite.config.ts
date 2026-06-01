import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");

  return {
    plugins: [react(), tailwindcss()],
    server: {
      port: parseInt(env.VITE_PORT),
      proxy: {
        "/api": {
          target:
            process.env.services__api__https__0 ||
            process.env.services__api__http__0,
          changeOrigin: true,
          secure: false,
          ws: true,
        },
      },
    },
    build: {
      outDir: "dist",
      rollupOptions: {
        input: "./index.html",
      },
    },
  };
});
