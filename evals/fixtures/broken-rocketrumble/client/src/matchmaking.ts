const MM_URL = import.meta.env.VITE_MM_URL as string;
const MM_TOKEN = import.meta.env.VITE_MM_TOKEN as string;

export type Assignment = {
  public_ip: string;
  fqdn: string;
  ports: Record<string, { internal: number; external: number; protocol: string }>;
};

const headers = { "Content-Type": "application/json", Authorization: MM_TOKEN };

export async function findMatch(onStatus: (s: string) => void): Promise<Assignment> {
  const res = await fetch(`${MM_URL}/tickets`, {
    method: "POST",
    headers,
    body: JSON.stringify({ profile: "coop", attributes: {} }),
  });
  if (!res.ok) throw new Error(`ticket: ${res.status}`);
  const ticket = await res.json();
  console.log(`ticket ${ticket.id} created (${ticket.status})`);

  for (;;) {
    await new Promise((r) => setTimeout(r, 2000));
    const poll = await fetch(`${MM_URL}/memberships/${ticket.id}`, { headers });
    if (!poll.ok) continue;
    const t = await poll.json();
    onStatus(t.status);
    if (t.status === "HOST_ASSIGNED") return t.assignment as Assignment;
  }
}

// Dev helper: start a deployment directly, skipping the matchmaker.
export async function spawnTestServer(): Promise<string> {
  const res = await fetch("https://api.edgegap.com/v2/deploy", {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: import.meta.env.VITE_EDGEGAP_API_TOKEN as string,
    },
    body: JSON.stringify({ application: "rocket-rumble", version: "v2", users: [{ user_type: "ip_address", user_data: { ip_address: "1.2.3.4" } }] }),
  });
  const d = await res.json();
  return d.request_id;
}
