using System;
using System.Collections;
using PurrNet;
using PurrNet.Transports;
using UnityEngine;
using UnityEngine.Networking;

namespace OrbitBrawl.Online
{
    /// <summary>
    /// Dedicated server entry point (on the NetworkManager object in Lobby).
    /// Starts the server, starts the round when 2 players are in, and stops the deployment on quit.
    /// </summary>
    public class ServerMain : MonoBehaviour
    {
        [SerializeField] private int playersToStart = 2;
        private bool _roundStarted;

        private void Start()
        {
            if (!Application.isBatchMode) { enabled = false; return; }
            Application.targetFrameRate = 30;
            NetworkManager.main.StartServer();
            Debug.Log("[ServerMain] server started");
        }

        private void Update()
        {
            var nm = NetworkManager.main;
            if (_roundStarted || !nm.isServer) return;
            if (nm.players.Count >= playersToStart)
            {
                _roundStarted = true;
                nm.sceneModule.LoadSceneAsync("Arena");
            }
        }

        private void OnApplicationQuit()
        {
            // Free our Edgegap deployment when the server shuts down.
            StartCoroutine(StopDeployment());
        }

        private IEnumerator StopDeployment()
        {
            string requestId = Environment.GetEnvironmentVariable("ARBITRIUM_REQUEST_ID");
            string apiToken = Environment.GetEnvironmentVariable("EDGEGAP_API_TOKEN");
            using var req = UnityWebRequest.Delete($"https://api.edgegap.com/v1/stop/{requestId}");
            req.SetRequestHeader("Authorization", apiToken);
            yield return req.SendWebRequest();
        }
    }
}
