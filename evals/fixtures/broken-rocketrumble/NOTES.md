# Rocket Rumble on Edgegap - what's broken (team notes)

Co-op wave survival, 1-4 players, Colyseus 0.17 server + Vite/TS browser client. Web build is live at https://rocketrumble.example.com. We moved the server to Edgegap last sprint, one deployment per match, with the Edgegap matchmaker (profile `coop`).

What we see:

1. Find Match on the live site does nothing. Console in `logs/browser-console-prod.txt`.
2. Find Match from `npm run dev` (localhost:5173) gets a ticket, then sits on "Searching..." forever. `logs/browser-console-dev.txt`.
3. When we connect by hand to a deployment (copied the IP from the dashboard), the live site refuses to connect, and the first try from localhost usually fails with a 502 and the game shows "Server error". Pressing Play again a few seconds later sometimes works.
4. Several deployments ran until the max duration (24h) and billed. Their logs show nobody ever joined. `logs/deployment-idle.txt`.
5. Mobile players whose phone locks for ~10 seconds come back to "room not found": the deployment is already gone. `logs/deployment-mobile.txt`.
6. When the last wave is cleared or everyone dies, the game over screen shows, and the deployment keeps running as long as anyone has the tab open.
7. Stopping a deployment from the dashboard takes the full grace period, and our "match ended" log line in the SIGTERM handler never prints.
8. Our very first app version (v1) never started: `exec format error`. Tim builds on his M2 MacBook. v2 was built on the CI box and runs.
9. Security review (`security-review.md`) flagged the server image and the web bundle.

Edgegap app: `rocket-rumble`, versions `v1` (broken), `v2` (current). App version settings exported in `edgegap/app-version-v2.json`, matchmaker config in `edgegap/matchmaker.json`.
