// steamworks.js wrapper (trimmed): returns the hex session ticket for Nakama.
export async function getSteamTicket(): Promise<string> {
  const steamworks = await import("steamworks.js");
  const api = steamworks.init(480);
  const ticket = await api.auth.getSessionTicket();
  return ticket.getBytes().toString("hex");
}
