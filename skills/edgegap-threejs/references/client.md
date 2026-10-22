# Client: Three.js page, Find Match, connection

Contents
1. No-build setup
2. The shell (menu, HUD, flow)
3. Rendering server state
4. Sending input
5. Find Match, direct mode, local mode
6. CORS and hosting the pages
7. Other renderers and bundlers

## 1. No-build setup

Three.js loads from a CDN through an import map, so the page is plain ES modules served by any static server:

```html
<script type="importmap">
  { "imports": { "three": "https://cdn.jsdelivr.net/npm/three@0.170.0/build/three.module.js" } }
</script>
<script type="module" src="./game-client.js"></script>
```

Pin the version. A project that already uses Vite or webpack keeps its bundler: copy `matchmaker.js` and `net.js` (or `shell.js`) into its sources and `import * as THREE from "three"` as usual.

Use relative paths (`./shell.js`, `../../client/lib/...`) so the pages also work under a sub-path such as GitHub Pages `https://<org>.github.io/<repo>/`.

## 2. The shell

`assets/client/shell.js` is the verified UI shared by the five sample games. A game calls:

```js
runGame({
  game: "coins",            // matchmaker profile name, and LOCAL_PORTS key
  title, tagline, help,
  teams: false,             // colour the scoreboard by team
  scoreText(p, msg), bannerText(msg), bigText(msg, myId),  // optional HUD text
  create(ctx) { return { state(msg), frame(dt), dispose() }; },
});
```

`ctx` = `{ THREE, scene, camera, renderer, sun, myId, info, input(obj) }`. `create` runs after `welcome`, so `info` (map, sizes) is there to build the level. `state(msg)` gets every state message; `frame(dt)` runs every animation frame. `dispose()` removes the level when the match ends, and the menu comes back ("Match finished. Play again?").

The shell also gives: `createStage` (renderer, lights, shadows, resize), `makeLabel` (name sprites), `EntityMap` (one mesh per server entity id, created and removed with the snapshot), `smooth(dt, rate)`, `lerpAngle`, `keys`/`axis()` for keyboard input, `colorOf(id)`, `TEAM_COLORS`, and `window.__game` (`phase`, `server`, `players`, `error`) for browser automation.

## 3. Rendering server state

State arrives at 20 Hz; the screen runs at 60+. Ease each object toward its latest server position every frame:

```js
const t = smooth(dt, 14);          // 1 - exp(-14 dt)
g.position.x += (e.x - g.position.x) * t;
```

Snap instead of easing when the jump is large (respawn, kickoff, new round), otherwise objects fly across the map. This adds roughly 50–100 ms of visual delay, fine for arcade games. For the player's own aim (mouse direction), show the local value immediately and send it; the server still decides what the shot hits.

Don't predict game rules on the client. If the game needs client-side prediction for movement (fast shooters), predict only the local player's movement with the same constants as the server and reconcile on each snapshot.

## 4. Sending input

`ctx.input(obj)` sends `{t:"input", ...obj}` only when it changed (at most every 33 ms) and at least every 250 ms. Send intents, not results: axes in [-1, 1], an aim angle, booleans for buttons, an enum for a direction. Never positions, damage or scores.

## 5. Find Match, direct mode, local mode

- **Find match**: `new Matchmaker({url, token, profile}).findMatch()` → `{url: "wss://<fqdn>:<port>", ticketId}`; then `connect(url, {name, ticket: ticketId})`. The ticket goes in `hello` so the server can put the player on their matchmaker team and, with `REQUIRE_TICKET=1`, refuse strangers.
- Poll every 1.5–2 s. `SEARCHING` → `MATCH_FOUND` (a server is starting) → `HOST_ASSIGNED`. `CANCELLED` = the ticket expired. Cancel with `DELETE /tickets/{id}` when the player clicks Cancel or closes the tab (`pagehide`, `keepalive: true`); after `HOST_ASSIGNED` a delete only returns 409, so the client forgets the ticket ID.
- The matchmaker answers **429** to bursts of calls (seen when 14 players queued in the same second). `matchmaker.js` backs off 1, 2, 4, 8 s and retries.
- **First connection**: a deployment can refuse connections for a few seconds after `HOST_ASSIGNED`. `net.js` retries 10 times, 1 s apart, until the server's `welcome`. An `error` message from the server (full, finished) is final and not retried.
- **Direct**: `?server=wss://<fqdn>:<port>` connects straight to a deployment started by `edgegap_deploy` or `edgegap-deploy.mjs`.
- **Local**: a "Local server" button connects to `ws://<page host>:<LOCAL_PORTS[game]>`. Plain `ws://` only works from an `http://` page (localhost); from `https` pages browsers block it.
- Settings: `config.js` (committed, empty or production values), `config.local.js` (git-ignored, same shape, loaded with a dynamic `import()` whose 404 is ignored), or `?mm=<url>&token=<token>`. `?autoplay=1` clicks Find Match on load (handy for tests); `?autoplay=local` connects to the local server.

## 6. CORS and hosting the pages

The browser calls the matchmaker cross-origin, so the matchmaker config's `allowed_cors_origins` must list each page origin exactly: scheme + host + port, no path, no trailing slash (`http://localhost:8080`, `https://<org>.github.io`, `https://html-classic.itch.zone` for itch.io embeds). The game server's WebSocket is not subject to CORS.

The pages can live anywhere static: GitHub Pages, itch.io, a CDN. They never need the Edgegap API token.

## 7. Other renderers and bundlers

Nothing above depends on Three.js except `shell.js`'s stage helpers. For Babylon.js, PlayCanvas engine, Phaser or a 2D canvas, keep `matchmaker.js` and `net.js`, keep the protocol and the easing, and draw with that engine. Not run on Edgegap with those engines.
