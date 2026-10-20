// Edgegap helpers: stop our own deployment when the arena is empty.
export const requestId = process.env.ARBITRIUM_REQUEST_ID;

const EMPTY_STOP_MS = 5000;
let players = 0;
let emptyTimer: NodeJS.Timeout | undefined;

export function playerJoined() {
  players++;
  if (emptyTimer) {
    clearTimeout(emptyTimer);
    emptyTimer = undefined;
  }
}

export function playerLeft() {
  players = Math.max(0, players - 1);
  if (players === 0 && requestId) {
    console.log(`[edgegap] 0 players, stopping in ${EMPTY_STOP_MS / 1000}s`);
    emptyTimer = setTimeout(stopDeployment, EMPTY_STOP_MS);
  }
}

export async function stopDeployment() {
  if (!requestId) return;
  console.log(`[edgegap] stopping deployment ${requestId}`);
  const res = await fetch(`https://api.edgegap.com/v1/stop/${requestId}`, {
    method: "DELETE",
    headers: { authorization: process.env.EDGEGAP_API_TOKEN ?? "" },
  });
  console.log(`[edgegap] stop request: ${res.status}`);
}
