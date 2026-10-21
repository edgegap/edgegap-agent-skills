import { WebSocketServer, WebSocket } from "ws";
import { Simulation } from "./simulation.js";

const PORT = Number(process.env.PORT ?? 9100);
const wss = new WebSocketServer({ port: PORT });
const sim = new Simulation();
const clients = new Map<WebSocket, string>();

wss.on("connection", (ws) => {
  ws.on("message", (raw) => {
    const msg = JSON.parse(raw.toString());
    if (msg.type === "join") {
      const id = sim.addPlayer(msg.name ?? "pilot");
      clients.set(ws, id);
      ws.send(JSON.stringify({ type: "welcome", id }));
    } else if (msg.type === "input") {
      const id = clients.get(ws);
      if (id) sim.applyInput(id, msg.input);
    }
  });
  ws.on("close", () => {
    const id = clients.get(ws);
    if (id) sim.removePlayer(id);
    clients.delete(ws);
  });
});

setInterval(() => {
  sim.step(1 / 20);
  const snapshot = JSON.stringify({ type: "state", state: sim.snapshot() });
  for (const ws of clients.keys()) ws.send(snapshot);
  if (sim.runOver) {
    for (const ws of clients.keys()) ws.send(JSON.stringify({ type: "run_over", salvage: sim.salvage }));
  }
}, 50);

console.log("Starhaul server listening on " + PORT);
