# Edgegap Agent Skills

Skills that let a coding agent (Claude Code, and other agents that read `SKILL.md` skills) take a multiplayer game from "works on my machine" to **online play with dedicated servers and matchmaking on [Edgegap](https://edgegap.com)**, end to end.

```
Edgegap Agent Skills
│
├─ Start here
│  └─ edgegap ····················· platform overview, server shape per game, safety rules for the MCP tools
│
├─ Engines
│  ├─ edgegap-unity ··············· Unity end to end: server build, matchmaker, client, e2e test
│  ├─ edgegap-fishnet ············· Unity + FishNet end to end, with templates verified on Edgegap
│  ├─ edgegap-purrnet ············· Unity + PurrNet end to end, with templates verified on Edgegap
│  └─ edgegap-unreal ·············· Unreal Engine end to end: source engine, Linux server, matchmaker, e2e test
│
├─ Any engine
│  ├─ edgegap-server-design ······· authoritative server and the deployment lifecycle contract
│  ├─ edgegap-containerize ········ the container contract, Dockerfiles, registries, app versions
│  ├─ edgegap-local-testing ······· parity tests on your machine, then one smoke-test deployment
│  └─ edgegap-troubleshoot ········ pull errors, crash loops, OOM, clients that can't connect
│
├─ Your backend
│  └─ edgegap-deployments-api ····· Deploy API, webhooks, cleanup; Nakama, PlayFab, UGS, EOS
│
└─ Going live
   └─ edgegap-launch ·············· launch checklist, rollouts, caching, cost control, operations
```

| Skill | What it does |
|---|---|
| [`edgegap`](skills/edgegap/SKILL.md) | Starting point for any engine. Picks the server shape for any game, including always-online single-player games whose logic should stay off the client, explains apps, versions, deployments and the matchmaker, sets the safety rules for acting on an Edgegap account, and routes to the other skills. |
| [`edgegap-unity`](skills/edgegap-unity/SKILL.md) | Unity games (Netcode for GameObjects, Mirror, WebGL; FishNet and PurrNet projects go to `edgegap-fishnet` and `edgegap-purrnet`). Converts host/client code to a headless dedicated server, builds and containerizes the Linux server, deploys it on Edgegap, integrates the Edgegap matchmaker SDK in the client, writes the matchmaker config, then debugs until real players get matched onto a server. |
| [`edgegap-fishnet`](skills/edgegap-fishnet/SKILL.md) | Unity games built on FishNet (Tugboat, Bayou, WebGL). Ships the FishNet-ready matchmaker client, server self-stop and scene-change persistence from the end-to-end run of [netcode-sample-unity-fishnet](https://github.com/edgegap/netcode-sample-unity-fishnet), and covers Start On Headless, Bayou `wss` with TLS Upgrade, removing client-side Edgegap API calls from older samples, and the SDK fix that Unity 2021.3 needs for matchmaker tickets. |
| [`edgegap-purrnet`](skills/edgegap-purrnet/SKILL.md) | Unity games built on PurrNet. Ships the templates from the end-to-end run of [PurrNet-Incremental-Sample](https://github.com/edgegap/PurrNet-Incremental-Sample): they add themselves at startup without scene edits (server start and self-stop, a connect panel with direct and matchmaker modes, a settings asset, command-line test flags). Covers PurrNet's auto start flags, the async `StartClient`, and `ObserversRpc` setup code that never runs on a dedicated server. |
| [`edgegap-unreal`](skills/edgegap-unreal/SKILL.md) | Unreal Engine 5 games, C++ or Blueprint-only. Sets up the source engine and Linux cross-compile toolchain, makes the project dedicated-server ready, drops in two C++ subsystems (matchmaker client with latency beacons, server self-stop), packages and containerizes the Linux server, deploys it on Edgegap, writes the matchmaker config, then tests two clients end to end. |
| [`edgegap-server-design`](skills/edgegap-server-design/SKILL.md) | Engine-agnostic server design: authority, injected env vars, fixed port on `0.0.0.0`, readiness, self-stop at match end, SIGTERM draining, logging. |
| [`edgegap-containerize`](skills/edgegap-containerize/SKILL.md) | Packages any server into a `linux/amd64` image, pushes it to the Edgegap registry or another one, and registers the app version with the right ports, credentials and restart policy. |
| [`edgegap-local-testing`](skills/edgegap-local-testing/SKILL.md) | Parity `docker run`, the random-external-port test, the drain test, two servers on one host, resource sizing, then one tagged smoke-test deployment. |
| [`edgegap-deployments-api`](skills/edgegap-deployments-api/SKILL.md) | Backend code that deploys servers for matches: player-based placement, env vars, tags, webhooks, rate limits, error handling, reconciliation. Covers keeping Nakama, PlayFab, Unity Matchmaker or EOS lobbies. |
| [`edgegap-launch`](skills/edgegap-launch/SKILL.md) | Production checklist: tier limits, tokens, version settings, caching, log storage, blue/green rollouts, cost control, observability, Private Fleets for persistent servers. |
| [`edgegap-troubleshoot`](skills/edgegap-troubleshoot/SKILL.md) | Symptom → cause → fix for images, registries, app versions, deployments, server processes, networking and the matchmaker. |

Dedicated (authoritative) servers by default. Relays only when the developer explicitly wants host/client.

API facts in the skills come from Edgegap's published [OpenAPI spec](https://github.com/edgegap/openapi-specification). The commit they were checked against is pinned in `scripts/spec-pin.txt`, and a weekly CI job opens an issue when the published spec changes.

## What it looks like

You ask, in your own words:

> My Unity game works when I play with my friend on the same wifi, but not over the internet. How do I fix that?

The agent then:

1. **Inspects the project**: Unity version, netcode, transport and port, scenes, target platforms. It asks only what it can't infer (players per match, browser build or not).
2. **Makes the game dedicated-server ready**: the server starts headless and binds `0.0.0.0`, players spawn on connect, and host-only logic moves to the server.
3. **Adds drop-in scripts**: an Edgegap matchmaking client (*Find Match* → connect) and a server lifecycle component. The lifecycle component reads match data and **stops its own deployment when the match ends**, so empty servers don't bill you.
4. **Builds the Linux dedicated server**, builds the Docker image and pushes it to your Edgegap registry.
5. **Creates the app and version** through the Edgegap MCP, and deploys a test server to prove the image and ports work.
6. **Writes the matchmaker config** and walks you through creating the matchmaker in the dashboard.
7. **Runs an end-to-end test**: two clients click *Find Match*, get matched, connect to the same server, and the server shuts down after they leave.

You only handle the parts that need a human: secrets, a few dashboard clicks, and playtesting.

## Install

### Claude Code: plugin (recommended)

```bash
/plugin marketplace add edgegap/edgegap-agent-skills
/plugin install edgegap@edgegap
```

The plugin installs all the skills **and** the hosted Edgegap MCP server. When you install and enable it, Claude Code asks for your [Edgegap API token](https://app.edgegap.com/user-settings?tab=tokens) (paste just the UUID). It is stored in your system's secure credential store (it's a `sensitive` plugin setting), never in your project, and sent only to the Edgegap MCP server.

### Any agent: the `skills` CLI

Install every skill into the current project (Claude Code, Cursor, Copilot, Codex and other agents that read `SKILL.md`):

```bash
npx skills add edgegap/edgegap-agent-skills --skill "*"
```

Add `-g` to install for your user instead of one project, or pick skills, e.g. `--skill edgegap --skill edgegap-containerize`. Update later with `npx skills update`.

### Claude Code: skills only, by hand

Copy the skills into your user skills folder:

```bash
git clone https://github.com/edgegap/edgegap-agent-skills
cp -r edgegap-agent-skills/skills/* ~/.claude/skills/
```

Manual copies don't update themselves; copy again after pulling.

Then add the Edgegap MCP server yourself:

```bash
claude mcp add --transport http edgegap https://mcp.edgegap.dev/mcp -H "Authorization: token <YOUR_EDGEGAP_API_TOKEN>" --scope user
```

### Other agents

Each skill is a plain folder: `SKILL.md`, plus `references/` and sometimes `assets/`. Use the `skills` CLI above, or point your agent at `skills/edgegap/SKILL.md` (or `skills/edgegap-unity/SKILL.md` for Unity, `skills/edgegap-fishnet/SKILL.md` for FishNet, `skills/edgegap-purrnet/SKILL.md` for PurrNet, `skills/edgegap-unreal/SKILL.md` for Unreal Engine). For the MCP server in other clients (Cursor, VS Code, Codex, Claude Desktop), see [edgegap-mcp](https://github.com/edgegap/edgegap-mcp).

> claude.ai and Claude Desktop *custom connectors* can't send an `Authorization` header, so they can't use the hosted MCP server. In Claude Desktop, use the local server (`npx -y @edgegap/mcp`) as described in the edgegap-mcp README.

## What the plugin runs, sends and fetches

Everything here happens only when you ask the agent to work on your game, and most steps show you the command first.

**Network**
- **Hosted Edgegap MCP server**, `https://mcp.edgegap.dev/mcp`, operated by Edgegap. Every tool call sends your Edgegap API token in the `Authorization` header. The server uses it for that request against the Edgegap API (`https://api.edgegap.com`) and does not store it. Tools list and create applications and app versions, start, inspect and stop deployments, read deployment logs, list registry tags, and generate Dockerfiles and matchmaker configs. Deployments are billed to your Edgegap account outside the free tier.
- **Edgegap API from your backend**, `https://api.edgegap.com`: code the `edgegap-deployments-api` skill writes for your backend calls it at runtime, with the token from your backend's secret manager. The agent itself never calls it directly or reads a token from your machine.
- **Edgegap container registry**, `registry.edgegap.com`: `docker push` of your game server image, using the registry login you enter yourself with `docker login`.
- **GitHub**: Unity Package Manager downloads the Edgegap Unity SDK (`https://github.com/edgegap/edgegap-unity-sdk.git#3.5.5`) when the skill adds it to your project. For Unreal, you clone the engine source (`https://github.com/EpicGames/UnrealEngine`) yourself, and its `Setup.bat` downloads engine dependencies from Epic.
- **Docker Hub**: `docker build` pulls the `ubuntu:22.04` base image.
- **Your matchmaker** (`https://<id>.edgegap.net`, created by you in the Edgegap dashboard): the game client code the skill adds calls it at runtime with the matchmaker auth token.
- **Optional checks**: `https://api.ipify.org`, to find your public IP so a test server is placed near you, and an HTTPS/WebSocket request to a test deployment to confirm its port answers.

**Local commands** (in your project folder): Unity in batch mode (`-executeMethod EdgegapServerBuild.Build`, `BuildClient`, `EdgegapSceneSetup.Setup`), the same for FishNet and PurrNet projects; for Unreal, the engine's `Build.bat`, `RunUAT BuildCookRun` (Linux server package) and `UnrealEditor -game` test clients; `docker build`, `docker run` (local smoke test), `docker push`, `git`, `curl`, and a local static web server for WebGL tests.

**Files it writes to your project**: C# scripts under `Assets/Edgegap/`, a `link.xml`, a `Dockerfile` and `.dockerignore`, `matchmaker-config.json`, a line in `Packages/manifest.json`, and edits to your scenes and netcode scripts (Unity). FishNet projects also get `EdgegapPersistence.cs`, and on Unity before 2022.3 an embedded copy of the Edgegap SDK with a one-line fix in `Packages/com.edgegap.unity-sdk`. PurrNet projects get `EdgegapBootstrap.cs`, `EdgegapConnectPanel.cs`, `EdgegapSettings.cs` and `Resources/EdgegapSettings.asset` instead of scene edits. For Unreal: two C++ subsystems in `Source/<Module>/`, module dependencies in `<Module>.Build.cs`, build settings in `*.Target.cs`, ini sections in `Config/DefaultGame.ini`, a `matchmaker-config.json`, a `Dockerfile` and `.dockerignore`, and for Blueprint-only projects a minimal C++ module. For other engines: a `Dockerfile`, `.dockerignore`, an optional start script, server lifecycle code (self-stop, SIGTERM handling), and, if you ask for it, backend code that calls the Deploy API.

**Inside your game server (on Edgegap, not on your machine)**: the `EdgegapServerLifecycle` script (Unity) or `EdgegapServerSubsystem` (Unreal) added to your game reads the deployment variables Edgegap injects into the server container, including a one-time self-stop key, and uses them only to call Edgegap's own self-stop endpoint for that deployment when the match ends. The game client script calls only your matchmaker. Neither script talks to the MCP server.

**Not collected**: the plugin itself sends no telemetry and no conversation data anywhere. Your Edgegap API token and registry credentials are never written to project files. The matchmaker auth token is written into the game client by design: it is meant to ship to players and grants no access to your Edgegap account.

## Requirements

- Unity: 2021.3+ (tested on Unity 6000.3) with the **Linux Build Support** and **Linux Dedicated Server Build Support** modules, and git (for the Edgegap Unity SDK package).
- Unreal Engine: 5.3+ **built from source** (tested on 5.5.4), Visual Studio 2022 with the C++ game workloads, and the Linux cross-compile toolchain for your engine version. Launcher builds can't build dedicated servers.
- [Docker](https://www.docker.com/products/docker-desktop/) running locally.
- A free [Edgegap account](https://app.edgegap.com/auth/register). Free tier limits: one deployment at a time, 60-minute deployments, and the matchmaker stops 3 hours after each start.

## What's verified

| Area | Status |
|---|---|
| Mirror + SimpleWebTransport (WebGL) | **End-to-end verified** on Edgegap: two browser clients matched by the matchmaker, connected over `wss` with TLS Upgrade, and the server stopped itself after they left. |
| Netcode for GameObjects 2.4 | Templates compile and the Linux dedicated server builds. Not yet verified end to end on Edgegap. |
| FishNet 4.1 + Bayou (WebGL) | **End-to-end verified** on Edgegap with [netcode-sample-unity-fishnet](https://github.com/edgegap/netcode-sample-unity-fishnet) (Unity 2021.3.45f2): two browser clients matched by the matchmaker, connected over `wss` with TLS Upgrade, played, and the server stopped itself; the no-player self-stop works too. The `edgegap-fishnet` templates are the files from that run. Tugboat (UDP) and Multipass compile but have not been run on Edgegap; FishNet 2.x/3.x are best effort. |
| PurrNet + UDPTransport | **End-to-end verified** on Edgegap with [PurrNet-Incremental-Sample](https://github.com/edgegap/PurrNet-Incremental-Sample) (Unity 6000.3.11f1): two desktop clients played on the same server by direct connection and through the matchmaker (matched in about 35 s), and the server stopped itself both times. The `edgegap-purrnet` templates are the files from that run. `WebTransport` (WebGL) has not been run on Edgegap. |
| Unreal Engine 5.5 (C++ subsystems) | **End-to-end verified** on Edgegap with the [unreal-sample](https://github.com/edgegap/unreal-sample) top-down project: two clients pinged the beacons, were matched under a latency rule in about 8 s, joined the same server, and the server stopped itself. The drop-in templates compile for the Win64 editor and as a Linux server (clang). Task evals: 3 scenarios (C++ project, Blueprint-only project, debugging a broken setup), 100% of checks with the skill versus 70.5% without, and only the with-skill outputs all compile against UE 5.5. |
| Edgegap Unity SDK | 3.5.5+ (3.5.5 is the first that compiles for WebGL). |
| Edgegap MCP | 0.3.3+ (`tls_upgrade`, `allowed_cors_origins`). Older servers work with dashboard fallbacks. |
| Triggering | `edgegap-unity`: 20/20 on its eval set (10 should-trigger prompts, 10 near-misses). `edgegap-unreal`, `edgegap-fishnet` and `edgegap-purrnet`: not yet measured. The new engine-agnostic skills ship with a routing eval set (`evals/routing-eval.json`) and safety scenarios (`evals/guardrails.json`), not yet run. |
| Engine-agnostic skills (0.2.0) | Written from the Edgegap docs and the published OpenAPI spec, and checked against the Unity end-to-end run. Exercised end to end with Unity and Unreal Engine servers; not yet with Godot or Node. |

## Security and cost

- Your **Edgegap API token** is organization-wide and can't be scoped. The skill never writes it to files, code or chat. It goes in the plugin's sensitive setting (prompted at install) or your own MCP config.
- The **matchmaker Auth Token** is designed to ship in game clients and grants no account access.
- Deployments cost money outside the free tier. The skill tags its test deployments, stops the ones it starts, and makes servers stop themselves when matches end.

## Repository layout

```
.claude-plugin/        plugin + marketplace manifests
.mcp.json              hosted Edgegap MCP server (token from the plugin's sensitive user setting)
skills/edgegap/        entry skill: routing, vocabulary, agent safety rules
skills/edgegap-unity/  Unity skill: SKILL.md, references/, assets/ (C# templates, Dockerfile, matchmaker configs)
skills/edgegap-fishnet/ FishNet skill: SKILL.md, references/, assets/ (FishNet-ready C# templates, Dockerfile, matchmaker config)
skills/edgegap-purrnet/ PurrNet skill: SKILL.md, references/, assets/ (runtime-bootstrapped C# templates, settings asset, Dockerfile, matchmaker config)
skills/edgegap-unreal/ Unreal Engine skill: SKILL.md, references/, assets/ (C++ subsystems, ini, Dockerfile, matchmaker config)
skills/edgegap-*/      engine-agnostic skills (server design, containerize, testing, API, launch, troubleshoot)
evals/                 task evals, trigger and routing sets, safety scenarios, a small fixture project
scripts/               validate.py, scan_private_refs.py, check_spec.py (all run by CI)
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Issues and pull requests are welcome, especially end-to-end reports for netcodes and platforms not yet verified.

## License

[Apache 2.0](LICENSE)
