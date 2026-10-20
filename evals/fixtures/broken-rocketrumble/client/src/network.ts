import { Client, Room } from "@colyseus/sdk";
import type { Assignment } from "./matchmaking";

export function endpointFromAssignment(a: Assignment): string {
  const port = a.ports["gameport"]?.external;
  return `ws://${a.public_ip}:${port}`;
}

export function endpointFromQuery(): string | null {
  const s = new URLSearchParams(location.search).get("server");
  return s ? `ws://${s}` : null;
}

export async function join(endpoint: string, name: string): Promise<Room> {
  const client = new Client(endpoint);
  try {
    const room = await client.joinOrCreate("arena", { name });
    console.log(`joined arena room ${room.roomId} as ${room.sessionId}`);
    return room;
  } catch (e) {
    console.log("join failed", e);
    throw e;
  }
}
