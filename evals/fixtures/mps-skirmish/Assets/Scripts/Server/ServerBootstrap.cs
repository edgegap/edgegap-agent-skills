using System.Collections.Generic;
using PlayFab;
using PlayFab.MultiplayerAgent.Model;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Skirmish.Server
{
    // Dedicated server entry point for PlayFab Multiplayer Servers (GSDK).
    public class ServerBootstrap : MonoBehaviour
    {
        private readonly List<ConnectedPlayer> _players = new();

        private void Start()
        {
#if UNITY_SERVER
            PlayFabMultiplayerAgentAPI.Start();
            PlayFabMultiplayerAgentAPI.OnServerActiveCallback += OnServerActive;
            PlayFabMultiplayerAgentAPI.OnShutDownCallback += () => Application.Quit();

            var config = PlayFabMultiplayerAgentAPI.GetConfigSettings();
            ushort port = 7777;
            if (config.TryGetValue("gameport", out string p))
                port = ushort.Parse(p);

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData("0.0.0.0", port);
            NetworkManager.Singleton.OnClientConnectedCallback += id =>
            {
                _players.Add(new ConnectedPlayer(id.ToString()));
                PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_players);
            };
            NetworkManager.Singleton.OnClientDisconnectCallback += id =>
            {
                _players.RemoveAll(x => x.PlayerId == id.ToString());
                PlayFabMultiplayerAgentAPI.UpdateConnectedPlayers(_players);
            };
            NetworkManager.Singleton.StartServer();

            PlayFabMultiplayerAgentAPI.ReadyForPlayers();
#endif
        }

        private void OnServerActive() => Debug.Log("Server allocated by PlayFab, waiting for players.");
    }
}
