import type { Room } from "@colyseus/sdk";
import { findMatch, spawnTestServer } from "./matchmaking";
import { endpointFromAssignment, endpointFromQuery, join } from "./network";

const status = document.getElementById("status")!;
const nameInput = document.getElementById("name") as HTMLInputElement;
const canvas = document.getElementById("arena") as HTMLCanvasElement;

function play(room: Room) {
  status.textContent = "";
  const ctx = canvas.getContext("2d")!;
  canvas.onmousemove = (e) => room.send("move", { x: e.offsetX, y: e.offsetY });
  room.onStateChange((state: any) => {
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.fillText(`wave ${state.wave}  enemies ${state.enemiesLeft}  ${state.phase}`, 10, 20);
    state.pilots.forEach((p: any) => ctx.fillRect(p.x - 5, p.y - 5, 10, 10));
  });
}

async function start() {
  const name = nameInput.value || "Pilot";
  const direct = endpointFromQuery();
  try {
    if (direct) return play(await join(direct, name));
    console.log("Find Match clicked");
    status.textContent = "Searching...";
    const assignment = await findMatch((s) => (status.textContent = s));
    play(await join(endpointFromAssignment(assignment), name));
  } catch (e) {
    console.log(e);
    status.textContent = "Server error";
  }
}

document.getElementById("find")!.onclick = start;
document.getElementById("spawn")!.onclick = async () => {
  status.textContent = "spawned " + (await spawnTestServer());
};
