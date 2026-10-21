import config from "@colyseus/tools";
import { monitor } from "@colyseus/monitor";
import { playground } from "@colyseus/playground";
import { RedisPresence } from "@colyseus/redis-presence";
import { RedisDriver } from "@colyseus/redis-driver";
import { RaceRoom } from "./rooms/RaceRoom";

export default config({
  options: {
    presence: new RedisPresence(process.env.REDIS_URL),
    driver: new RedisDriver(process.env.REDIS_URL),
  },

  initializeGameServer: (gameServer) => {
    gameServer.define("race", RaceRoom);
  },

  initializeExpress: (app) => {
    app.get("/health", (_req, res) => {
      res.send("ok");
    });

    // TODO: password-protect before launch
    app.use("/monitor", monitor());

    app.use("/", playground());
  },

  beforeListen: () => {
    console.log("Hover Hoop server starting");
  },
});
