# Our Edgegap setup (notes)

- App `skyraid`, version `v3`
  - Image: registry.edgegap.com/skyraid/server:v3
  - Port: name `gameport`, 7777, UDP
  - Registry credentials: set
- Server build: Linux Dedicated Server (Server subtarget, IL2CPP) -> Builds/Server, then the Dockerfile in the project root.
- We deploy from our backend when a party of 4-8 is ready, then tell the clients to join session `skyraid-main`.

## Problems
1. Players get in, but everyone has ~230 ms and our debug overlay (`runner.CurrentConnectionType`) says Relayed, even a player in the same city as the server (logs/client.log).
2. We ran the image in Docker Desktop on a dev PC and joined from that same PC: also Relayed. So we think it's Edgegap's network or Photon.
3. When two parties start at the same time (two deployments), players from one party sometimes end up in the other party's match.
4. Our security review says the server logs show secrets.
5. Deployments keep running after a match ends, until the max duration.
