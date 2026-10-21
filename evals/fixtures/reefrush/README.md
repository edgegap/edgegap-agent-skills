# Reef Rush

2-4 player arena brawler (fish with weapons). Godot 4.4, nakama-godot 3.x client addon (not included in this fixture: `addons/com.heroiclabs.nakama/`).

Online play today: players sign in with a device ID, Nakama's matchmaker groups them, and the
match is relayed through Nakama with `NakamaMultiplayerBridge`. The first player in the match
becomes the host (peer 1) and runs the game: hits, deaths, rounds and the winner. The host's
client also writes the win to the `reefrush_wins` leaderboard.

Nakama runs locally with docker compose (`nakama/`). Go runtime module in `nakama/main.go`.

Run a client: `godot --path .`
