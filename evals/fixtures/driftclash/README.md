# Drift Clash

Small 4-player arcade racer in Godot 4.4 (GDScript, high-level multiplayer, ENet UDP 9050).

- Players log in to PlayFab (title `D41F7`) with a custom ID through the REST API (`scripts/playfab.gd`); there is no PlayFab SDK.
- Today the "Race" button connects to an IP typed in the menu, and someone runs `godot --headless -- --server` on a VPS.
- The server already decides laps and the winner (`scripts/race.gd`).
- `export_presets.cfg` has a "Linux Server" preset (dedicated server export).
