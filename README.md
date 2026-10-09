# Edgegap Agent Skills

Skills that let a coding agent (Claude Code, and other agents that read `SKILL.md` skills) take a multiplayer game from "works on my machine" to **online play with dedicated servers and matchmaking on [Edgegap](https://edgegap.com)**, end to end.

| Skill | What it does |
|---|---|
| [`edgegap-unity`](skills/edgegap-unity/SKILL.md) | Unity games (Netcode for GameObjects, Mirror, FishNet, WebGL). Converts host/client code to a headless dedicated server, builds and containerizes the Linux server, deploys it on Edgegap, integrates the Edgegap matchmaker SDK in the client, writes the matchmaker config, then debugs until real players get matched onto a server. |

Dedicated (authoritative) servers only. No relays.

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

The plugin installs the skill **and** the hosted Edgegap MCP server. When you install and enable it, Claude Code asks for your [Edgegap API token](https://app.edgegap.com/user-settings?tab=tokens) (paste just the UUID). It is stored in your system's secure credential store (it's a `sensitive` plugin setting), never in your project, and sent only to the Edgegap MCP server.

### Claude Code: skill only

Copy the skill into your user skills folder:

```bash
git clone https://github.com/edgegap/edgegap-agent-skills
cp -r edgegap-agent-skills/skills/edgegap-unity ~/.claude/skills/
```

Then add the Edgegap MCP server yourself:

```bash
claude mcp add --transport http edgegap https://mcp.edgegap.dev/mcp -H "Authorization: token <YOUR_EDGEGAP_API_TOKEN>" --scope user
```

### Other agents

The skill is a plain folder: `SKILL.md`, plus `references/` and `assets/`. Point your agent at `skills/edgegap-unity/SKILL.md`. For the MCP server in other clients (Cursor, VS Code, Codex, Claude Desktop), see [edgegap-mcp](https://github.com/edgegap/edgegap-mcp).

> claude.ai and Claude Desktop *custom connectors* can't send an `Authorization` header, so they can't use the hosted MCP server. In Claude Desktop, use the local server (`npx -y @edgegap/mcp`) as described in the edgegap-mcp README.

## What the plugin runs, sends and fetches

Everything here happens only when you ask the agent to work on your game, and most steps show you the command first.

**Network**
- **Hosted Edgegap MCP server**, `https://mcp.edgegap.dev/mcp`, operated by Edgegap. Every tool call sends your Edgegap API token in the `Authorization` header. The server uses it for that request against the Edgegap API (`https://api.edgegap.com`) and does not store it. Tools list and create applications and app versions, start, inspect and stop deployments, read deployment logs, list registry tags, and generate Dockerfiles and matchmaker configs. Deployments are billed to your Edgegap account outside the free tier.
- **Edgegap container registry**, `registry.edgegap.com`: `docker push` of your game server image, using the registry login you enter yourself with `docker login`.
- **GitHub**: Unity Package Manager downloads the Edgegap Unity SDK (`https://github.com/edgegap/edgegap-unity-sdk.git#3.5.5`) when the skill adds it to your project.
- **Docker Hub**: `docker build` pulls the `ubuntu:22.04` base image.
- **Your matchmaker** (`https://<id>.edgegap.net`, created by you in the Edgegap dashboard): the game client code the skill adds calls it at runtime with the matchmaker auth token.
- **Optional checks**: `https://api.ipify.org`, to find your public IP so a test server is placed near you, and an HTTPS/WebSocket request to a test deployment to confirm its port answers.

**Local commands** (in your project folder): Unity in batch mode (`-executeMethod EdgegapServerBuild.Build`, `BuildClient`, `EdgegapSceneSetup.Setup`), `docker build`, `docker run` (local smoke test), `docker push`, `git`, `curl`, and a local static web server for WebGL tests.

**Files it writes to your project**: C# scripts under `Assets/Edgegap/`, a `link.xml`, a `Dockerfile` and `.dockerignore`, `matchmaker-config.json`, a line in `Packages/manifest.json`, and edits to your scenes and netcode scripts.

**Inside your game server (on Edgegap, not on your machine)**: the `EdgegapServerLifecycle` script added to your game reads the deployment variables Edgegap injects into the server container, including a one-time self-stop key, and uses them only to call Edgegap's own self-stop endpoint for that deployment when the match ends. The game client script calls only your matchmaker. Neither script talks to the MCP server.

**Not collected**: the plugin itself sends no telemetry and no conversation data anywhere. Your Edgegap API token and registry credentials are never written to project files. The matchmaker auth token is written into the game client by design: it is meant to ship to players and grants no access to your Edgegap account.

## Requirements

- Unity 2021.3+ (tested on Unity 6000.3) with the **Linux Build Support** and **Linux Dedicated Server Build Support** modules.
- [Docker](https://www.docker.com/products/docker-desktop/) running locally.
- git (for the Edgegap Unity SDK package).
- A free [Edgegap account](https://app.edgegap.com/auth/register). Free tier limits: one deployment at a time, 60-minute deployments, and the matchmaker stops 3 hours after each start.

## What's verified

| Area | Status |
|---|---|
| Mirror + SimpleWebTransport (WebGL) | **End-to-end verified** on Edgegap: two browser clients matched by the matchmaker, connected over `wss` with TLS Upgrade, and the server stopped itself after they left. |
| Netcode for GameObjects 2.4 | Templates compile and the Linux dedicated server builds. Not yet verified end to end on Edgegap. |
| FishNet 4 | Adapter provided, not yet compile-tested. |
| Edgegap Unity SDK | 3.5.5+ (3.5.5 is the first that compiles for WebGL). |
| Edgegap MCP | 0.3.3+ (`tls_upgrade`, `allowed_cors_origins`). Older servers work with dashboard fallbacks. |
| Triggering | 20/20 on the included eval set: 10 should-trigger prompts, 10 near-misses. |

## Security and cost

- Your **Edgegap API token** is organization-wide and can't be scoped. The skill never writes it to files, code or chat. It goes in the plugin's sensitive setting (prompted at install) or your own MCP config.
- The **matchmaker Auth Token** is designed to ship in game clients and grants no account access.
- Deployments cost money outside the free tier. The skill tags its test deployments, stops the ones it starts, and makes servers stop themselves when matches end.

## Repository layout

```
.claude-plugin/        plugin + marketplace manifests
.mcp.json              hosted Edgegap MCP server (token from the plugin's sensitive user setting)
skills/edgegap-unity/  the skill: SKILL.md, references/, assets/ (C# templates, Dockerfile, matchmaker configs)
evals/                 test prompts, trigger eval set and a small fixture project
scripts/validate.py    checks used by CI
```

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Issues and pull requests are welcome, especially end-to-end reports for netcodes and platforms not yet verified.

## License

[Apache 2.0](LICENSE)
