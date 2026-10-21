# Skirmish

Top-down 2v2 arena brawler (Unity 2022.3, Netcode for GameObjects 1.11, UnityTransport UDP 7777).

- PlayFab title: login with custom ID (one per device), matchmaking queue `skirmish-2v2` (4 players, skill rule). The queue as exported from Game Manager is in `playfab/queue.json`.
- Servers: PlayFab Multiplayer Servers (build "skirmish-linux-v14", East US + West Europe), GSDK in the server.
- The PlayFab Unity SDK is imported in Assets/PlayFabSDK (not included here).
