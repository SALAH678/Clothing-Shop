import { defineConfig, type ServerOptions } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import fs from "fs";

// HTTPS certs are optional and dev-only: missing files must not break build/preview.
function getDevHttps(): ServerOptions["https"] {
  if (process.env.NODE_ENV === "production") return undefined;
  try {
    return {
      key: fs.readFileSync("./localhost+2-key.pem"),
      cert: fs.readFileSync("./localhost+2.pem"),
    };
  } catch {
    console.warn("[vite] Local HTTPS certs not found, falling back to HTTP.");
    return undefined;
  }
}

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    https: getDevHttps(),
  },
});
