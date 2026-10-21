import { Client, Room } from "colyseus.js";

// Colyseus Cloud endpoint (us-east). Local dev uses ws://localhost:2567.
const ENDPOINT = import.meta.env.DEV
  ? "ws://localhost:2567"
  : "wss://us-ord-a1b2c3d4.colyseus.cloud";

export const client = new Client(ENDPOINT);

export async function joinRace(name: string): Promise<Room> {
  const room = await client.joinOrCreate("race", { name });
  localStorage.setItem("hoverhoop.reconnect", room.reconnectionToken);
  return room;
}

export async function tryReconnect(): Promise<Room | null> {
  const token = localStorage.getItem("hoverhoop.reconnect");
  if (!token) return null;
  try {
    return await client.reconnect(token);
  } catch {
    localStorage.removeItem("hoverhoop.reconnect");
    return null;
  }
}
