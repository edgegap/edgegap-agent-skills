# Our Edgegap setup (notes)

- App `kartclash`, version `v3`
  - Image: registry.edgegap.com/kartclash/server:latest
  - Port: name `gameport`, 7777, TCP
  - Registry credentials: set
- Build: Unity Linux Dedicated Server build to Builds/LinuxServer, then
  `docker build -t registry.edgegap.com/kartclash/server:latest . && docker push registry.edgegap.com/kartclash/server:latest`
- Matchmaker `kartclash-mm` running matchmaker.json from this folder.

## Problems
1. Find Match always fails right away (logs/client.log). It then falls back to QuickPlay.
2. Deploying v3 directly from the dashboard: deployment goes Ready but no client can connect (timeout).
3. Earlier test servers ran for hours until they hit the 24h limit, even after everyone left.
4. I fixed something in the server and re-pushed `latest`; new deployments behave exactly the same.
