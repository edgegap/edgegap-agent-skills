# Security review - Rocket Rumble (excerpt)

- HIGH: `docker history registry.edgegap.com/rr-studio/rocket-rumble:v2` shows `ENV EDGEGAP_API_TOKEN=token ...`. Anyone who can pull the image gets an organization-wide Edgegap API token.
- HIGH: the production JS bundle (`assets/index-*.js`) contains a string starting with `token 1f3a`, used by a "Spawn test server" button that calls `https://api.edgegap.com/v2/deploy`. The button is hidden with CSS but the code and token ship to every player.
- MEDIUM: `https://<deployment>:<port>/monitor` is reachable without auth on live deployments and can list and dispose rooms. `/` serves the Colyseus playground.
- LOW: the matchmaker auth token is in the bundle too. Please confirm whether that one is meant to be public.
