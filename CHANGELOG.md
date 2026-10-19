# Changelog

Each skill also carries its own `metadata.version` in its frontmatter, and the `edgegap` skill states the pack version (`Pack version:`), so an agent can tell which copy it has.

## 0.7.1 (2026-10-19)

Matchmaker expansion stages, and the 0.6.1 fixes carried over to `edgegap-unity` and `edgegap-purrnet`, which share the FishNet templates.

- Matchmaker templates: a profile with `min_team_size < max_team_size` needs an expansion stage, or a partial match only starts when the tickets expire. Measured on matchmaker 3.3.2: two tickets waited 5 minutes without one, and matched within 40 s with `"15": {"match_size": {"min_team_size": 2}}`. The FishNet, Unity and PurrNet `casual` templates now have that stage. It keeps `min_team_size` at 2, so a match still never starts below the requested size. The matchmaker references explain the rule, and troubleshooting has a row for two players stuck in `SEARCHING` for about 5 minutes.
- `edgegap-fishnet` (0.2.1): the expansion stage in `casual-webgl.json`, `references/matchmaker.md` §3 and troubleshooting.
- `edgegap-unity` (0.1.9):
  - The expansion stage in `casual.json`. `teams-latency.json` is unchanged: at 2v2, min and max are equal, so it can't make a partial match.
  - Pins Edgegap Unity SDK 3.5.6, and a new troubleshooting row for HTTP 400 on every ticket on Unity before 2022.3 (SDK 3.5.5 or older): replace an embedded, hand-patched SDK with the git package.
  - The `casual` and `teams-latency` matchmaker templates no longer lower `min_team_size` to 1; the matchmaker reference and the end-to-end test no longer promise a solo match.
  - `EdgegapSceneSetup` uses explicit `== null` checks instead of `??` on Unity objects.
- `edgegap-purrnet` (0.1.1):
  - The expansion stage in `casual.json`.
  - Pins Edgegap Unity SDK 3.5.6; the embed-and-patch fix for Unity before 2022.3 is gone, replaced by the SDK version.
  - The matchmaker template no longer lowers `min_team_size` to 1 after 30 s.
  - Transport log messages use explicit `!= null` checks instead of `?.` on the transport component.
- `edgegap` (0.7.1): pack version.
- Evals: `evals/purrnet-evals.json` (3 tasks, with fixtures `orbitbrawl` and `broken-orbitbrawl`) and `evals/purrnet-routing-eval.json` (20 queries). Results with `edgegap-purrnet` 0.1.1 (before the expansion stage): routing 60/60; tasks 100% with the skill vs 85% without, all outputs compile as Linux dedicated servers.

## 0.7.0 (2026-10-19)

Adds Netcode for GameObjects.

- New skill `edgegap-ngo` (0.1.0): Unity games built on Netcode for GameObjects, end to end, from the verified run of [netcode-sample-unity-ngo-bossroom](https://github.com/edgegap/netcode-sample-unity-ngo-bossroom) (Boss Room, Unity 6000.0.54f1, NGO 1.13.0, UnityTransport UDP, SDK 3.5.6).
  - Templates that add themselves at startup so scenes stay unchanged: `EdgegapBootstrap`, `EdgegapServerLifecycle` (starts the NGO server on the Edgegap internal port, or only watches a server the game starts itself; self-stop), `EdgegapMatchmakingClient` (Find Match panel, connects to the public IP), an `EdgegapSettings` asset, and command-line flags for scripted two-client tests. The generic plain-NGO server start and direct connection were tested locally (Docker server, two clients) and the generic matchmaker adapter compiles; the Boss Room `ConnectionManager` adapter, server starter and test driver in `Examples/BossRoom/` are the files from the Edgegap run.
  - NGO pitfalls: host-only logic, connection payloads and approval, debug/release build checks, Relay leftovers, UnityTransport needing an IP, client-only code throwing on the server, two local clients sharing a player id, a UGS Matchmaker client that breaks compilation.
  - Matchmaker: a profile with `min_team_size < max_team_size` needs an expansion stage, or a partial match only starts when the tickets expire (measured: 5 minutes without one, under 40 s with one).
- `edgegap` (0.7.0): routes NGO projects to `edgegap-ngo`.
- `edgegap-unity` (0.1.8): points NGO projects to `edgegap-ngo`; keeps Mirror, WebGL and other netcodes.
- README, plugin and marketplace descriptions name NGO; the verified table lists the Boss Room end-to-end run.
- Evals: NGO queries in `evals/routing-eval.json`.

## 0.6.1 (2026-10-19)

Fixes in `edgegap-fishnet` found by its first task evals.

- `edgegap-fishnet` (0.2.0):
  - Pins Edgegap Unity SDK 3.5.6, which fixes the form-encoded ticket body on Unity before 2022.3. The embed-and-patch steps for 3.5.5 are gone; an embedded, hand-patched SDK should be replaced by the git package.
  - New guidance for `LoadGlobalScenes` with `ReplaceScenes.All`: the game's match manager and players die with the replaced scene on a dedicated server. Scene objects can't be moved with `MovedNetworkObjects`, so spawn the manager as a global prefab (`SetIsGlobal(true)` after `Instantiate`) or give the next scene its own; move or respawn players.
  - The matchmaker template no longer lowers `min_team_size` to 1 after 30 s, so it never starts a match below the requested size.
  - `EdgegapSceneSetup` and the example bridge use explicit `== null` checks instead of `??` / `?.` on Unity objects.
- Evals: `evals/fishnet-evals.json` (3 tasks, with fixtures `kartclash` and `broken-kartclash`) and `evals/fishnet-routing-eval.json` (20 queries). Results: routing 60/60; tasks 100% with the skill vs 86% without (iteration 2), all outputs compile as Linux dedicated servers.

## 0.6.0 (2026-10-15)

Adds Manus browser games.

- New skill `edgegap-manus` (0.1.0): browser games built with Manus, taken to an authoritative dedicated server and the Edgegap matchmaker.
  - The Manus backend owns matchmaking: Steam OpenID sign-in verified server-side, join / status / cancel / leave routes that create tickets with `player_ip` (so servers are placed near players, not near the backend), modes as profiles, regions as an `intersection` rule, parties as group tickets, and a queue table whose primary key prevents duplicate tickets. Timeouts, cancel, reconnect and allocation failures (MATCH_FOUND ↔ SEARCHING flips, allocation timeout) are handled and mapped to UI states.
  - Ed25519 join tokens: the backend signs, the game server verifies with a public key on the app version and checks request ID, `MM_TICKET_IDS`, expiry and replay.
  - Node.js `ws` game server template (authoritative loop, input validation, reconnect, self-stop, SIGTERM), browser `MatchClient`, Dockerfile, GitHub Actions build to the Edgegap registry, matchmaker config, and phase-by-phase prompts to paste into Manus.
  - Tested locally against a mock matchmaker and in Docker; not yet run end to end on Edgegap and Manus hosting.
- `edgegap` (0.6.0): routes Manus games to `edgegap-manus`, and points JavaScript browser games at its templates.
- README, plugin and marketplace descriptions name Manus.

## 0.5.0 (2026-10-15)

Adds PurrNet.

- New skill `edgegap-purrnet` (0.1.0): Unity games built on PurrNet, end to end, from the verified run of [PurrNet-Incremental-Sample](https://github.com/edgegap/PurrNet-Incremental-Sample) (Unity 6000.3.11f1, PurrNet `dev`, `UDPTransport`, SDK 3.5.5).
  - Templates from that run, which add themselves at startup so scenes stay unchanged: `EdgegapBootstrap` (turns PurrNet's auto start off in builds, keeps the Editor's host + clone flow), `EdgegapServerLifecycle` (UDP 5000, self-stop), `EdgegapConnectPanel` (direct connection by default, Find Match when the matchmaker is on), `EdgegapMatchmakingClient`, and an `EdgegapSettings` asset, plus command-line flags for scripted two-client tests.
  - PurrNet pitfalls: `[ObserversRpc]` server setup that needs `runLocally: true` on a dedicated server (the sample spawned no trees without it), the async `StartClient` that breaks naive retry loops, PurrNet's own Edgegap auto-setup, permissive network rules, and the untested WebGL path.
- `edgegap` (0.5.0): routes PurrNet projects to `edgegap-purrnet`.
- `edgegap-unity` (0.1.7): points PurrNet projects to `edgegap-purrnet`.
- README, plugin and marketplace descriptions name PurrNet; the verified table lists the PurrNet end-to-end run.

## 0.4.0 (2026-10-15)

Adds FishNet.

- New skill `edgegap-fishnet` (0.1.0): Unity games built on FishNet, end to end, from the verified run of [netcode-sample-unity-fishnet](https://github.com/edgegap/netcode-sample-unity-fishnet) (`Fishnet_v4.1.6R_WebGL_Matchmaker`, Unity 2021.3.45f2, FishNet 4.1.6R, Bayou, SDK 3.5.5).
  - FishNet-ready templates from that run: `EdgegapMatchmakingClient` (Bayou `wss` to the fqdn, Tugboat to the IP), `EdgegapServerLifecycle` (port 7770, starts the server in every server build), `EdgegapPersistence` (keeps the Edgegap object across FishNet scene loads, so self-stop survives the menu → game switch), `EdgegapSceneSetup` that adds all three, and an example menu bridge with a direct-connection mode and an opt-in matchmaker mode.
  - Covers Start On Headless versus the lifecycle auto start, ServerManager's frame rate, NetworkHudCanvas auto start, Multipass ports, FishNet 2.x/3.x, and removing the client-side Edgegap API calls and token from older FishNet samples.
  - The SDK 3.5.5 fix Unity before 2022.3 needs (form-encoded POST bodies make every matchmaker ticket fail with HTTP 400), the lone-ticket timing seen on matchmaker 3.3.2, and WebSocket probes that keep a server alive.
- `edgegap` (0.4.0): routes FishNet projects to `edgegap-fishnet`.
- `edgegap-unity` (0.1.6): points FishNet projects to `edgegap-fishnet`; its FishNet adapter is now the verified one (sets Use WSS on Bayou), with notes on scene persistence and the SDK fix.
- README, plugin and marketplace descriptions name FishNet; the verified table lists the FishNet end-to-end run.

## 0.3.1 (2026-10-14)

Shrinks the plugin so the directory scanner accepts it.

- `.claude-plugin/icon.png`: 256×256, 256-colour palette, 23 KB (was 512×512 full colour, 162 KB). The pack unpacks to about 331 KB instead of 470 KB.
- No skill content changes.

## 0.3.0 (2026-10-14)

Adds Unreal Engine.

- New skill `edgegap-unreal` (0.1.0): Unreal Engine 5 games, C++ or Blueprint-only, end to end.
  - Source engine and Linux cross-compile setup, with the fixes that cost hours: compiler pinning when a newer Visual Studio is installed, UE 5.5's missing `<chrono>` include, ShaderCompileWorker, the Windows path limit and old target build settings.
  - Two drop-in C++ subsystems that need no changes to the game mode or player controller. The matchmaker client measures latency to the beacons, creates and polls a ticket and travels to the server; it adds a default overlay, console commands, Blueprint functions and an event. The server subsystem stops matchmaker deployments once they are empty.
  - Linux server packaging, a Dockerfile that runs the server binary directly with `-stdout -FullStdOutLogOutput`, and a matchmaker template with a latency rule.
  - Verified end to end on Edgegap with UE 5.5.4 and the unreal-sample project. Task evals: 100% of checks with the skill versus 70.5% without.
- `edgegap` (0.3.0): routes Unreal Engine projects to `edgegap-unreal`.
- `edgegap-unity` (0.1.5): description points Unreal projects to `edgegap-unreal`.
- README, plugin and marketplace descriptions name Unreal Engine.

## 0.2.1 (2026-10-13)

Fixes the Anthropic Directory hold on 0.2.0.

- The OpenAPI spec copies in `spec/` are gone: their examples contain token-shaped values, which the directory treats as shipped secrets. `scripts/spec-pin.txt` now records the upstream commit the skills were checked against, and `check_spec.py` diffs the published spec against it.
- `scan_private_refs.py` also flags long literal values assigned to token, password or secret fields.
- `edgegap` (0.2.1): without the MCP tools, the agent guides the developer through the dashboard instead of asking for the API token or reading it from their machine.
- `edgegap-deployments-api` (0.2.1): the backend sketch loads the token from a secret manager instead of an environment variable.

## 0.2.0 (2026-10-13)

Pack version 0.2.0. The pack now covers any engine, not only Unity.

- New skills:
  - `edgegap` (entry point): server shape per game (per match, per player session for always-online single-player games, persistent), vocabulary, tiers and limits, and `references/agent-rules.md`, the one place that states when to use MCP tools versus HTTP and the safety rules (confirm before spending, tag what you start, stop only your own deployments by exact ID, secrets never in files, clients or servers).
  - `edgegap-server-design`: the deployment lifecycle contract for any engine (injected `ARBITRIUM_*`/`MM_*` variables, `0.0.0.0` binding, readiness, self-stop, restart policy, SIGTERM and the termination grace period, logs, persistent worlds).
  - `edgegap-containerize`: the container contract, a generic Dockerfile and variants, registry push, tags, app version fields, TLS Upgrade for browser clients.
  - `edgegap-local-testing`: parity run, random-external-port test, drain test, two servers on one host, sizing, and a tagged smoke-test deployment.
  - `edgegap-deployments-api`: backend allocation with `POST /v2/deployments`, webhooks, rate limits, error handling, retry safety without an idempotency key, reconciliation, and the Nakama, UGS, PlayFab and EOS integrations. `references/api-reference.md` is written from the published OpenAPI spec.
  - `edgegap-launch`: launch checklist, blue/green rollouts, caching, cost control, observability, Private Fleets and persistence.
  - `edgegap-troubleshoot`: symptom → cause → fix tables for every layer, any engine.
- `edgegap-unity` (0.1.4): `metadata.version` added, a pointer to the backend and launch skills, always-online single-player games, and the free registry is now 10 GB.
- `scripts/check_spec.py` and a weekly workflow (`spec-drift.yml`) open an issue when Edgegap's published OpenAPI spec changes from the commit pinned in `scripts/spec-pin.txt`.
- `scripts/scan_private_refs.py` (run in CI): fails on personal paths, personal emails, real matchmaker or deployment hosts, registry robot accounts, and token-shaped strings.
- `scripts/validate.py`: requires `metadata.version` on every skill, and checks relative links and skill names mentioned across skills.
- Evals: `evals/routing-eval.json` (which skill should load for 20 prompts) and `evals/guardrails.json` (7 safety scenarios). Not yet run.
- `SECURITY.md`, `CODE_OF_CONDUCT.md`, issue templates. README: skill map, `npx skills add` install for any agent.

## 0.1.3 (2026-10-09)

- Directory review fixes: no example matchmaker URLs next to token arguments in the C# templates, the self-stop comment no longer names the injected key, the changelog no longer names the icon file, and the README explains which deployment variables the game server reads inside its Edgegap container.

## 0.1.2 (2026-10-09)

- The Edgegap API token is now a `sensitive` plugin setting (`userConfig.edgegap_api_token`), prompted when the plugin is enabled and kept in the system credential store, instead of the `EDGEGAP_API_TOKEN` environment variable.
- README: new "What the plugin runs, sends and fetches" section listing every network endpoint, local command and project file.

## 0.1.1 (2026-10-09)

- Plugin icon (Edgegap logo, 512×512) and `privacyPolicyUrl` for the Anthropic Directory listing.

## 0.1.0 (2026-10-09)

First release.

- `edgegap-unity` skill: from Unity host/client or local multiplayer to dedicated servers and matchmaking on Edgegap, end to end.
  - Drop-in C# templates: `EdgegapMatchmakingClient` (Group Up flow, connect with retries, cancel/409 handling), `EdgegapServerLifecycle` (headless start, injected match data, self-stop), ticket DTOs, `link.xml`.
  - Editor tools: `EdgegapServerBuild` (Linux dedicated server and test-client builds from the command line) and `EdgegapSceneSetup` (adds and configures the components without hand-editing scenes).
  - Netcode adapters for Netcode for GameObjects, Mirror (KCP, Telepathy, SimpleWeb) and FishNet (Tugboat, Bayou).
  - Dockerfile (no `env` dump of the self-stop token), matchmaker config templates, troubleshooting tables.
  - Uses Edgegap MCP 0.3.3+ (`tls_upgrade`, `allowed_cors_origins`), with dashboard fallbacks for older servers.
- Claude Code plugin and marketplace manifests, bundling the hosted Edgegap MCP server.
- Verified end to end on Edgegap with the Mirror WebGL sample.
