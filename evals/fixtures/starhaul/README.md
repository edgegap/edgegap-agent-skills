# Starhaul

Co-op space salvage for 2-6 players. Electron desktop client (TypeScript, nakama-js); players sign in
with Steam (the client gets a Steam session ticket through steamworks.js and calls
`authenticateSteam`).

- `nakama/`: Nakama 3.39 with a TypeScript runtime module (`src/main.ts`, built to `build/index.js`).
  Today the matchmaker hook creates an authoritative Nakama match (`salvage_match`) that only
  relays positions; the real simulation runs on whichever client joined first.
- `server/`: a headless Node.js simulation server (WebSocket, `ws`, port 9100) we wrote to replace
  the client-side simulation. It works locally: clients connect to ws://localhost:9100 and send
  `{"type":"join","name":"..."}`. It has no auth and never stops on its own.
- `client/`: the Electron client.

The salvage total of a run is written by the client to the `salvage_best` leaderboard.
