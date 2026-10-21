# Rift Rumble: PlayFab + Edgegap bridge

Unity 2022.3 FishNet game. PlayFab (title `7C2E1`) does login and matchmaking (queue `rumble`, 2 players, server allocation off). An Azure Function on the `playfab.matchmaking.match_found` rule starts an Edgegap server and stores its address in the players' data, and the client polls it.

- `azure-functions/`: Node 22 Function App (Flex Consumption, westus2). `appsettings.json` is an export of its settings with secrets masked.
- `Assets/Scripts/PlayFabMatch.cs`: client matchmaking.
- `edgegap/app-version.json`: the Edgegap app version `rift-rumble` / `v3` as exported from the dashboard.
- `server/Dockerfile`: server image.
- `logs/`: what we saw in the last playtest.
