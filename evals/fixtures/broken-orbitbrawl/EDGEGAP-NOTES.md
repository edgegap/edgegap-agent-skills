# Our Edgegap setup (notes)

- App `orbitbrawl`, version `v2`
  - Image: registry.edgegap.com/orbitbrawl/server:latest
  - Port: name `gameport`, 7777, UDP
  - Registry credentials: set
- Server build: Linux Dedicated Server -> Builds/Server, then
  `docker build -t registry.edgegap.com/orbitbrawl/server:latest . && docker push registry.edgegap.com/orbitbrawl/server:latest`
- Matchmaker `orbitbrawl-mm` running matchmaker.json (we added a latency rule so EU and NA don't mix).

## Problems
1. Find Match fails straight away with HTTP 400 (logs/client.log).
2. Direct deploy from the dashboard, then connecting to it by hand: the client log says "connect attempt N failed" five times in a row within a second and gives up.
3. When two of us did get into a server last week (before the latency rule), the arena was empty: no pickups, and the round never ended.
4. Servers keep running until the 24h limit after everyone leaves.
5. Our security review flagged something in the server image.
6. Re-pushed `latest` after a fix, nothing changed.
7. Players who launch the client build sometimes see it "connecting to 127.0.0.1" before they press anything.
