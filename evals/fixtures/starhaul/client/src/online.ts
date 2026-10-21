import { Client, Session, Socket } from "@heroiclabs/nakama-js";
import { getSteamTicket } from "./steam";

const client = new Client("defaultkey", "127.0.0.1", "7350", false);
let session: Session;
let socket: Socket;

export async function signIn() {
  const ticket = await getSteamTicket();
  session = await client.authenticateSteam(ticket, true);
  socket = client.createSocket(false);
  await socket.connect(session, true);
}

export async function findMatch(onJoined: (matchId: string) => void) {
  socket.onmatchmakermatched = async (matched) => {
    const match = await socket.joinMatch(undefined, matched.token);
    onJoined(match.match_id);
  };
  await socket.addMatchmaker("*", 2, 6);
}

export async function submitSalvage(total: number) {
  await client.writeLeaderboardRecord(session, "salvage_best", { score: String(total) });
}
