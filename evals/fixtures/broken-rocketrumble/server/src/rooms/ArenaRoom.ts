import { Room, Client } from "colyseus";
import { Schema, MapSchema, type } from "@colyseus/schema";
import { playerJoined, playerLeft } from "../edgegap";

const LAST_WAVE = 10;

export class Pilot extends Schema {
  @type("string") name = "";
  @type("number") x = 0;
  @type("number") y = 0;
  @type("number") hp = 100;
}

export class ArenaState extends Schema {
  @type("string") phase = "playing"; // playing | gameover
  @type("number") wave = 1;
  @type("number") enemiesLeft = 8;
  @type("boolean") victory = false;
  @type({ map: Pilot }) pilots = new MapSchema<Pilot>();
}

export class ArenaRoom extends Room {
  maxClients = 4;
  state = new ArenaState();

  onCreate() {
    this.onMessage("move", (client, msg: { x: number; y: number }) => {
      const p = this.state.pilots.get(client.sessionId);
      if (p && p.hp > 0) { p.x = msg.x; p.y = msg.y; }
    });
    this.onMessage("kill", () => this.enemyKilled());
    this.onMessage("hit", (client, dmg: number) => {
      const p = this.state.pilots.get(client.sessionId);
      if (!p) return;
      p.hp = Math.max(0, p.hp - dmg);
      if ([...this.state.pilots.values()].every((x) => x.hp === 0)) this.gameOver(false);
    });
  }

  onJoin(client: Client, options: { name?: string }) {
    const p = new Pilot();
    p.name = options.name ?? "Pilot";
    this.state.pilots.set(client.sessionId, p);
    playerJoined();
    console.log(`[arena] ${client.sessionId} joined (${this.clients.length} players)`);
  }

  // connection lost without leaving: keep the seat 30 s for mobile players
  onDrop(client: Client) {
    console.log(`[arena] ${client.sessionId} dropped (consented=false), waiting 30s for reconnection`);
    playerLeft();
    this.allowReconnection(client, 30);
  }

  onReconnect(client: Client) {
    console.log(`[arena] ${client.sessionId} reconnected`);
    playerJoined();
  }

  onLeave(client: Client) {
    playerLeft();
    this.state.pilots.delete(client.sessionId);
  }

  private enemyKilled() {
    if (this.state.phase !== "playing") return;
    this.state.enemiesLeft--;
    if (this.state.enemiesLeft > 0) return;
    if (this.state.wave === LAST_WAVE) return this.gameOver(true);
    this.state.wave++;
    this.state.enemiesLeft = 8 + this.state.wave * 2;
  }

  private gameOver(victory: boolean) {
    this.state.phase = "gameover";
    this.state.victory = victory;
    console.log(`[arena] game over, victory=${victory}`);
  }
}
