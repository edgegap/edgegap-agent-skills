# Edgegap setup (in progress)

- App `sky-arena`, version `v1`: image `registry.edgegap.com/skyarena-team/sky-arena:latest`, port 9050 **TCP** named `gameport`, 0.5 vCPU / 1 GB.
- Each new build: export "Linux Server", `docker build -t registry.edgegap.com/skyarena-team/sky-arena:latest .`, push. Version v1 stays the same.
- Matchmaker `sky-arena-mm` from `matchmaker.json`, profile `default`.
- Server export: preset "Linux Server" (Dedicated Server mode).

## What we see

- In the editor, Find Match gets a server. In the exported Windows build the Find Match button does nothing at all.
- When a client gets a server it says "Joining…" forever. The server never logs anyone.
- Deployment logs on the dashboard are empty, even the "starting" line.
- Two players take about 5 minutes to get matched.
- Deployments keep running until the 60-minute free-tier limit.
- Tested a direct connect with a dev build once (typed the IP): got into the lobby list, but the game never started.
- Security review: "the server image prints secrets to stdout".
