# Changelog

Each skill also carries its own `metadata.version` in its frontmatter, and the `edgegap` skill states the pack version (`Pack version:`), so an agent can tell which copy it has.

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
