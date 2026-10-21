// Salvage simulation (trimmed for this fixture).
export class Simulation {
  players = new Map<string, { name: string; x: number; y: number; cargo: number }>();
  salvage = 0;
  runOver = false;
  private elapsed = 0;
  private nextId = 1;

  addPlayer(name: string): string {
    const id = "p" + this.nextId++;
    this.players.set(id, { name, x: 0, y: 0, cargo: 0 });
    return id;
  }
  removePlayer(id: string) { this.players.delete(id); }
  applyInput(id: string, input: { dx: number; dy: number }) {
    const p = this.players.get(id);
    if (p) { p.x += input.dx; p.y += input.dy; }
  }
  step(dt: number) {
    if (this.runOver) return;
    this.elapsed += dt;
    for (const p of this.players.values()) { p.cargo += dt; this.salvage += Math.floor(dt * 10); }
    if (this.elapsed > 600) this.runOver = true; // 10 minute run
  }
  snapshot() { return { salvage: this.salvage, players: [...this.players.entries()] }; }
}
