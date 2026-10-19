using System;
using System.Collections;
using FishNet;
using UnityEngine;
using UnityEngine.Networking;

namespace KartClash.Online
{
    /// <summary>
    /// Server side of our Edgegap setup. Sits on the "Online" object in the Lobby scene.
    /// Starts the server in headless builds and stops the deployment when everyone left.
    /// </summary>
    public class ServerBoot : MonoBehaviour
    {
        [SerializeField] private float emptyTimeout = 30f;
        private float _emptySince = -1f;
        private bool _hadPlayers;

        private void Start()
        {
            if (!Application.isBatchMode)
            {
                enabled = false;
                return;
            }
            InstanceFinder.ServerManager.StartConnection();
            Debug.Log("[ServerBoot] server started");
        }

        private void Update()
        {
            int clients = InstanceFinder.ServerManager.Clients.Count;
            if (clients > 0) { _hadPlayers = true; _emptySince = -1f; return; }
            if (!_hadPlayers) return;
            if (_emptySince < 0f) _emptySince = Time.time;
            if (Time.time - _emptySince > emptyTimeout)
            {
                enabled = false;
                StartCoroutine(StopDeployment());
            }
        }

        private IEnumerator StopDeployment()
        {
            string url = Environment.GetEnvironmentVariable("ARBITRIUM_DELETE_URL");
            string token = Environment.GetEnvironmentVariable("ARBITRIUM_DELETE_TOKEN");
            if (string.IsNullOrEmpty(url)) yield break;
            using var req = UnityWebRequest.Delete(url);
            req.SetRequestHeader("Authorization", token);
            yield return req.SendWebRequest();
            Debug.Log($"[ServerBoot] self-stop: {req.responseCode}");
        }
    }
}
