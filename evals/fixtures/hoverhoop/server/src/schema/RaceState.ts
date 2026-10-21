import { Schema, MapSchema, type } from "@colyseus/schema";

export class Racer extends Schema {
  @type("string") name = "";
  @type("number") x = 0;
  @type("number") y = 0;
  @type("number") angle = 0;
  @type("number") lap = 0;
  @type("number") hoop = 0;
  @type("boolean") connected = true;
  @type("number") finishedAt = 0;
}

export class RaceState extends Schema {
  @type("string") phase = "waiting"; // waiting | countdown | racing | results
  @type("number") timeLeft = 180;
  @type({ map: Racer }) racers = new MapSchema<Racer>();
  @type("string") winner = "";
}
