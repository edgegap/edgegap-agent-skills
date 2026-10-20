import { Room, Client } from "colyseus";
import { RaceState, Racer } from "../schema/RaceState";

const LAPS = 3;
const HOOPS_PER_LAP = 8;
const RACE_SECONDS = 180;
const MIN_PLAYERS = 2;

type Input = { thrust: number; turn: number };

export class RaceRoom extends Room<RaceState> {
  maxClients = 6;
  state = new RaceState();
  private inputs = new Map<string, Input>();

  onCreate() {
    this.onMessage("input", (client, input: Input) => {
      this.inputs.set(client.sessionId, input);
    });
    this.onMessage("hoop", (client, hoop: number) => this.passHoop(client.sessionId, hoop));
    this.setSimulationInterval((dt) => this.update(dt), 1000 / 30);
  }

  onJoin(client: Client, options: { name?: string }) {
    const racer = new Racer();
    racer.name = (options.name || "Pilot").slice(0, 16);
    racer.x = 100 + this.state.racers.size * 40;
    racer.y = 500;
    this.state.racers.set(client.sessionId, racer);

    if (this.state.phase === "waiting" && this.state.racers.size >= MIN_PLAYERS) {
      this.startCountdown();
    }
  }

  async onLeave(client: Client, consented: boolean) {
    const racer = this.state.racers.get(client.sessionId);
    if (!racer) return;
    racer.connected = false;

    if (consented) {
      this.state.racers.delete(client.sessionId);
      return;
    }

    try {
      // mobile players drop a lot; give them 20 s to come back
      await this.allowReconnection(client, 20);
      racer.connected = true;
    } catch {
      this.state.racers.delete(client.sessionId);
    }
  }

  private startCountdown() {
    this.state.phase = "countdown";
    this.clock.setTimeout(() => {
      this.state.phase = "racing";
      this.state.timeLeft = RACE_SECONDS;
    }, 3000);
  }

  private passHoop(sessionId: string, hoop: number) {
    const racer = this.state.racers.get(sessionId);
    if (!racer || this.state.phase !== "racing" || hoop !== racer.hoop) return;
    racer.hoop = (racer.hoop + 1) % HOOPS_PER_LAP;
    if (racer.hoop === 0) racer.lap++;
    if (racer.lap >= LAPS && !racer.finishedAt) {
      racer.finishedAt = Date.now();
      if (!this.state.winner) this.state.winner = racer.name;
    }
  }

  private update(dtMs: number) {
    if (this.state.phase !== "racing") return;
    const dt = dtMs / 1000;
    this.state.racers.forEach((racer, id) => {
      const input = this.inputs.get(id);
      if (!input || racer.finishedAt) return;
      racer.angle += input.turn * 3 * dt;
      racer.x += Math.cos(racer.angle) * input.thrust * 220 * dt;
      racer.y += Math.sin(racer.angle) * input.thrust * 220 * dt;
    });

    this.state.timeLeft = Math.max(0, this.state.timeLeft - dt);
    const everyoneDone = [...this.state.racers.values()].every((r) => r.finishedAt > 0);
    if (this.state.timeLeft === 0 || everyoneDone) {
      this.showResults();
    }
  }

  private showResults() {
    this.state.phase = "results";
    // back to the lobby so the same group can race again
    this.clock.setTimeout(() => {
      this.state.racers.forEach((r) => { r.lap = 0; r.hoop = 0; r.finishedAt = 0; });
      this.state.winner = "";
      this.state.phase = "waiting";
      if (this.state.racers.size >= MIN_PLAYERS) this.startCountdown();
    }, 10000);
  }
}
