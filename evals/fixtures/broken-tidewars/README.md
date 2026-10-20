# Tidewars

2-4 player naval arena, Godot 4.4. Accounts, matchmaking and leaderboards on Nakama 3.39
(Go module in `nakama/`). Matches run on Edgegap dedicated servers started by Nakama through
the Edgegap Fleet Manager for Nakama (github.com/edgegap/nakama-edgegap). See EDGEGAP-NOTES.md.

- `nakama/`: Go module, Dockerfile, docker compose, `.env`.
- `server/EdgegapServer.gd`: dedicated server mode (headless), ENet on UDP 7777.
- `client/Online.gd`: sign-in (email) and matchmaking.
- `game/Battle.gd`: round flow.
- `logs/`: what we captured.
