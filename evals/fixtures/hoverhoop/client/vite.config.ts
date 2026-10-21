import { defineConfig } from "vite";

// base "./" so the same build works on play.hoverhoop.gg and inside the itch.io iframe
export default defineConfig({
  base: "./",
  server: { port: 5173 },
});
