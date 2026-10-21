# Edgegap notes (team wiki export)

Setup: Edgegap app `tidewars`, version `v3` (image registry.edgegap.com/<project>/tidewars:v3,
port `gameport` 7777 UDP, 0.5 vCPU / 512 MB, max duration 24 h). Nakama runs on our office
box with docker compose; players and the office use it at 127.0.0.1 / LAN.

Problems, in the order we hit them:

1. Rebuilding the Nakama image now fails at `go mod download` (see logs/nakama-build.log). The
   image that is running was built from a vendored copy someone had locally.
2. Players get matched, then right away the client shows "Unable to start a game server".
   logs/nakama.log.
3. Last week (when we still signed in with custom IDs) deployments were created but failed;
   logs/deployment-v3.log.
4. Once, a deployment did start and Nakama crashed at the same moment (panic in our module).
5. Security review: our Edgegap API token shows up in `docker compose logs nakama`.
6. The deployments that ran never stopped by themselves; one ran 24 h.
7. The `tidewars_wins` leaderboard has players with 99999 wins.

Please fix what you can.
