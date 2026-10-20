import Phaser from "phaser";
import type { Room } from "colyseus.js";
import { getStateCallbacks } from "colyseus.js";

export class RaceScene extends Phaser.Scene {
  private boards = new Map<string, Phaser.GameObjects.Triangle>();
  private hud!: Phaser.GameObjects.Text;

  constructor(private room: Room) {
    super("race");
  }

  create() {
    this.hud = this.add.text(16, 16, "", { fontSize: "20px" });
    const $ = getStateCallbacks(this.room);
    $(this.room.state).racers.onAdd((racer: any, id: string) => {
      const board = this.add.triangle(racer.x, racer.y, 0, 20, 10, 0, 20, 20, id === this.room.sessionId ? 0xffcc00 : 0x66ccff);
      this.boards.set(id, board);
      $(racer).onChange(() => {
        board.setPosition(racer.x, racer.y);
        board.setRotation(racer.angle + Math.PI / 2);
        board.setAlpha(racer.connected ? 1 : 0.4);
      });
    });
    $(this.room.state).racers.onRemove((_racer: any, id: string) => {
      this.boards.get(id)?.destroy();
      this.boards.delete(id);
    });
  }

  update() {
    const keys = this.input.keyboard!.createCursorKeys();
    this.room.send("input", {
      thrust: keys.up.isDown ? 1 : 0,
      turn: (keys.right.isDown ? 1 : 0) - (keys.left.isDown ? 1 : 0),
    });
    const s = this.room.state as any;
    this.hud.setText(`${s.phase}  ${Math.ceil(s.timeLeft)}s  ${s.winner ? "Winner: " + s.winner : ""}`);
  }
}
