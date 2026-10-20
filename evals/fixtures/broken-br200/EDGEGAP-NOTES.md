# Our Edgegap setup (notes)

- App `br200`, version `v2`
  - Image: registry.edgegap.com/br200/server:latest
  - Port: name `gameport`, 7777, UDP
  - Env: `EXTRA_ARGS=-ip 0.0.0.0`
  - Registry credentials: set
- Build: Linux Dedicated Server -> Builds/Server, then
  `docker build -t registry.edgegap.com/br200/server:latest . && docker push registry.edgegap.com/br200/server:latest`
- Testing: tools/test-clients.bat <session> on a Windows PC in Montreal.

## What happened
1. IL2CPP server build failed (logs/il2cpp-build.log), so we switched the Server and Standalone backends to Mono. That built in 2 minutes.
2. First deployment (start script with `-ip 127.0.0.1`): the server started, but test clients got `MethodAccessException` and never got a character (logs/server-v1-mono.log).
3. Someone said clients need the server's public address, so the start script now passes `-ip $ARBITRIUM_PUBLIC_IP`. Now the server never starts: "Failed to bind socket" (logs/server-v2.log).
4. We added `EXTRA_ARGS=-ip 0.0.0.0` on the app version to override it and re-pushed `latest`. Same error.
5. Every test-clients.bat run opens 10 players per window, and they all get GameNotFound (logs/client-test.log).
