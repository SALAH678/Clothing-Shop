import { defineConfig, type ServerOptions } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";
import fs from "fs";

// Location of the mkcert files. Overridable so the certs can be mounted from the
// host (compose mounts them under /certs) instead of being baked into the image.
const keyPath = process.env.DEV_HTTPS_KEY ?? "./localhost+2-key.pem";
const certPath = process.env.DEV_HTTPS_CERT ?? "./localhost+2.pem";

// HTTPS certs are optional and dev-only: missing files must not break build/preview.
function getDevHttps(): ServerOptions["https"] {
  if (process.env.NODE_ENV === "production") return undefined;
  try {
    return {
      key: fs.readFileSync(keyPath),
      cert: fs.readFileSync(certPath),
    };
  } catch {
    console.warn(
      `[vite] Local HTTPS certs not found (${keyPath}, ${certPath}), falling back to HTTP. ` +
        "The API only allows the https://localhost:5173 origin, so browser requests will fail until valid certs are available."
    );
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
