# Changelog

## 0.1.2 (2026-10-09)

- The Edgegap API token is now a `sensitive` plugin setting (`userConfig.edgegap_api_token`), prompted when the plugin is enabled and kept in the system credential store, instead of the `EDGEGAP_API_TOKEN` environment variable.
- README: new "What the plugin runs, sends and fetches" section listing every network endpoint, local command and project file.

## 0.1.1 (2026-10-09)

- Plugin icon (`.claude-plugin/icon.png`, Edgegap logo, 512×512) and `privacyPolicyUrl` for the Anthropic Directory listing.

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
