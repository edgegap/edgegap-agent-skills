using System;
using System.Collections;
using System.Text;
using PurrNet;
using PurrNet.Transports;
using UnityEngine;
using UnityEngine.Networking;

namespace OrbitBrawl.Online
{
    /// <summary>
    /// "Find Match" button on the Lobby canvas. Creates a matchmaker ticket, polls it, connects.
    /// </summary>
    public class FindMatch : MonoBehaviour
    {
        [SerializeField] private string matchmakerUrl = "https://om-p7q2w9z4x1.edgegap.net";
        [SerializeField] private string authToken = "PASTE_MATCHMAKER_AUTH_TOKEN";
        [SerializeField] private string profile = "brawl";
        [SerializeField] private int connectAttempts = 5;

        public string Status { get; private set; } = "Idle";

        public void Search() => StartCoroutine(SearchRoutine());

        private IEnumerator SearchRoutine()
        {
            Status = "Searching";
            string body = "{\"profile\":\"" + profile + "\",\"attributes\":{}}";
            using var create = new UnityWebRequest(matchmakerUrl + "/tickets", "POST");
            create.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(body));
            create.downloadHandler = new DownloadHandlerBuffer();
            create.SetRequestHeader("Authorization", authToken);
            create.SetRequestHeader("Content-Type", "application/json");
            yield return create.SendWebRequest();
            if (create.result != UnityWebRequest.Result.Success)
            {
                Status = "Error";
                Debug.LogError($"[FindMatch] ticket failed: {create.responseCode} {create.downloadHandler.text}");
                yield break;
            }

            var ticket = JsonUtility.FromJson<Ticket>(create.downloadHandler.text);
            while (true)
            {
                yield return new WaitForSeconds(2f);
                using var poll = UnityWebRequest.Get($"{matchmakerUrl}/tickets/{ticket.id}");
                poll.SetRequestHeader("Authorization", authToken);
                yield return poll.SendWebRequest();
                ticket = JsonUtility.FromJson<Ticket>(poll.downloadHandler.text);
                Status = ticket.status;
                if (ticket.status == "HOST_ASSIGNED") break;
            }

            var udp = NetworkManager.main.GetComponent<UDPTransport>();
            udp.address = ticket.assignment.fqdn;
            udp.serverPort = (ushort)ticket.assignment.ports.gameport.@internal;

            for (int attempt = 1; attempt <= connectAttempts; attempt++)
            {
                NetworkManager.main.StartClient();
                // wait while connecting
                while (NetworkManager.main.clientState == ConnectionState.Connecting)
                    yield return null;
                if (NetworkManager.main.clientState == ConnectionState.Connected)
                {
                    Status = "Connected";
                    yield break;
                }
                Debug.LogWarning($"[FindMatch] connect attempt {attempt} failed");
                NetworkManager.main.StopClient();
                yield return new WaitForSeconds(1f);
            }
            Status = "Could not connect";
        }

        [Serializable] private class Ticket { public string id; public string status; public Assignment assignment; }
        [Serializable] private class Assignment { public string fqdn; public string public_ip; public Ports ports; }
        [Serializable] private class Ports { public Port gameport; }
        [Serializable] private class Port { public int @internal; public int external; }
    }
}
