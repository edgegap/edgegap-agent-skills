import { defineServer, defineRoom, playground, monitor } from "colyseus";
import { ArenaRoom } from "./rooms/ArenaRoom";
import { requestId, stopDeployment } from "./edgegap";

const port = Number(process.env.PORT || 2567);

const server = defineServer({
  rooms: {
    arena: defineRoom(ArenaRoom),
  },
  express: (app) => {
    app.use("/", playground());
    app.use("/monitor", monitor());
  },
});

server.listen(port).then(() => {
  console.log(`Rocket Rumble listening on ${port}`);
  if (requestId) console.log(`[edgegap] request id ${requestId}`);
});

process.on("SIGTERM", async () => {
  console.log("[edgegap] match ended, stopping");
  await stopDeployment();
  process.exit(0);
});
