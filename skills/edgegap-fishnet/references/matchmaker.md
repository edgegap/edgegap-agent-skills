# Edgegap matchmaker for a FishNet game

Contents
1. How it fits together
2. The config
3. Lone tickets and testing
4. Creating the matchmaker in the dashboard
5. Ticket ↔ profile contract
6. What the server receives
7. Shipping a new build

## 1. How it fits together

```
Client ──POST /groups (profile, attributes)──▶ Matchmaker ──match found──▶ Edgegap deploys the app version near the players
Client ◀──poll: SEARCHING → MATCH_FOUND → HOST_ASSIGNED {fqdn, public_ip, ports{gameport:{external}}}
Client ──Bayou wss://fqdn:external  /  Tugboat public_ip:external──▶ FishNet server (env: MM_MATCH_ID, MM_TICKET_IDS, ...)
```

`EdgegapMatchmakingClient` runs this flow with the SDK's Group Up client (a group of one), retries the connection while the server boots, and handles a cancel that races a match (409 → resume). The server's `EdgegapServerLifecycle` logs the match and stops the deployment when it empties.

## 2. The config

Start from `assets/matchmaker/casual-webgl.json`:
- `version` `3.3.2`, `inspect: true` for development (turn off for live), `max_deployment_retry_count: 3`.
- `allowed_cors_origins`: every origin that serves a WebGL build, exact scheme + host + port, no path (e.g. `http://localhost:8080`, `https://<user>.itch.io`, `https://html-classic.itch.zone`).
- Profile `casual`: one team of 2-4 players (`player_count` rule), tickets expire after 5 minutes, and an expansion lowers `min_team_size` to 1 after 30 s.
- `application.name` / `version`: the app and version from Phase 4. They must exist and be active, or the dashboard rejects the config.

Desktop-only (Tugboat) games can add a `latencies` rule for better placement; the client must then send beacons (`UseLatencyBeacons = true`). Never for WebGL, which can't ping beacons. `edgegap_build_matchmaker_config` generates configs too (pass `allowed_cors_origins` for WebGL). Save the result as `matchmaker-config.json` in the project root.

## 3. Lone tickets and testing

In the verified run (matchmaker 3.3.2, free tier), a single ticket got `HOST_ASSIGNED` only when it neared its 5-minute expiration, not at the 30-second expansion. Two tickets matched within seconds. So:
- Run the end-to-end test with **two clients** (two browser tabs, or two builds).
- Don't promise solo matchmaking after 30 s. If the game needs it, test it on the developer's tier, and consider a shorter `ticket_expiration_period` together with a client that retries.

## 4. Creating the matchmaker in the dashboard

There is no API for this. Guide the developer:
1. https://app.edgegap.com/matchmaker-management-v2/matchmakers/list → **Create Matchmaker**, a name like `mygame-dev`, upload `matchmaker-config.json`.
2. `The application configuration is not valid for profile X`: the app name/version doesn't match an active version. `Docker image ... is not cached` is only a warning on the free tier.
3. **Create and Start**. When ONLINE, copy the **API URL** and **Auth Token**. The Auth Token is client-safe and grants no account access.
4. DNS can take ~5 minutes: `curl -s -H "Authorization: <auth token>" <API URL>/monitor` until it answers. The token is sent raw, with no `Bearer` or `token` prefix.

Then set them on `EdgegapMatchmakingClient` (`BaseUrl`, `AuthToken`), or rerun `EdgegapSceneSetup` with `-edgegapBaseUrl` and `-edgegapAuthToken`. Free-tier matchmakers stop 3 hours after each start; restart from the list, the URL and token stay the same. To delete the app or version later, delete the matchmaker first.

## 5. Ticket ↔ profile contract

- The ticket's `profile` must equal a profile name (`EdgegapMatchmakingClient.Profile`).
- Every rule except `player_count` needs a ticket attribute with the rule's name, and unknown attributes are rejected. A mismatch is HTTP 400 on `POST /groups`.
- On Unity before 2022.3 with the stock SDK 3.5.5, **every** ticket gets HTTP 400 even with a correct profile: that is the SDK's form-encoded POST body (`troubleshooting.md` §1), not the config.

## 6. What the server receives

Matchmaker deployments get `MM_MATCH_ID`, `MM_MATCH_PROFILE`, `MM_EXPANSION`, `MM_TICKET_IDS` (JSON array), `MM_TICKET_<id>`, `MM_GROUPS`, `MM_TEAMS` and the usual `ARBITRIUM_*` variables. `EdgegapServerLifecycle.MatchEnv` parses them; `ExpectedPlayers` is the ticket count, and the startup log prints `N matched player(s)`.

To validate joiners, send `EdgegapMatchmakingClient.TicketId` from the client in a FishNet authenticator or a first `ServerRpc`, and kick connections whose ticket isn't in `MM_TICKET_IDS`. Optional for a first playtest.

## 7. Shipping a new build

Build the server → push a new tag → new app version → matchmaker **Stop → Edit** `application.version` **→ Start**. Forgetting the last step keeps matches on the old build.
