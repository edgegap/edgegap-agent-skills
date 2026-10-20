import Phaser from "phaser";
import type { Room } from "colyseus.js";
import { joinRace, tryReconnect } from "./network";
import { RaceScene } from "./RaceScene";

const status = document.getElementById("status")!;
const playButton = document.getElementById("play") as HTMLButtonElement;
const nameInput = document.getElementById("name") as HTMLInputElement;

function startGame(room: Room) {
  document.getElementById("menu")!.style.display = "none";
  new Phaser.Game({
    type: Phaser.AUTO,
    parent: "game",
    width: 1280,
    height: 720,
    scene: new RaceScene(room),
  });
}

playButton.onclick = async () => {
  playButton.disabled = true;
  status.textContent = "Joining...";
  try {
    startGame(await joinRace(nameInput.value || "Pilot"));
  } catch (e) {
    status.textContent = "Could not join: " + (e as Error).message;
    playButton.disabled = false;
  }
};

tryReconnect().then((room) => room && startGame(room));
