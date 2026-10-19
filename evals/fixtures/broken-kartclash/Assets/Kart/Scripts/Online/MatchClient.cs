using System;
using System.Collections;
using FishNet;
using FishNet.Transporting.Tugboat;
using UnityEngine;
using UnityEngine.Networking;

namespace KartClash.Online
{
    /// <summary>
    /// "Find Match" button. Creates a ticket on our Edgegap matchmaker, polls it, then connects.
    /// </summary>
    public class MatchClient : MonoBehaviour
    {
        [SerializeField] private string matchmakerUrl = "https://om-k2x9fj3q1a.edgegap.net";
        [SerializeField] private string authToken = "PASTE_MATCHMAKER_AUTH_TOKEN";
        [SerializeField] private string profile = "Casual";

        public string Status { get; private set; } = "Idle";

        public void FindMatch() => StartCoroutine(FindMatchRoutine());

        private IEnumerator FindMatchRoutine()
        {
            Status = "Creating ticket";
            string body = "{\"profile\":\"" + profile + "\",\"attributes\":{}}";
            using var create = UnityWebRequest.Post(matchmakerUrl + "/tickets", body);
            create.SetRequestHeader("Authorization", authToken);
            create.SetRequestHeader("Content-Type", "application/json");
            yield return create.SendWebRequest();
            if (create.result != UnityWebRequest.Result.Success)
            {
                Status = "Ticket failed";
                Debug.LogError($"[MatchClient] Ticket request failed: {create.responseCode} {create.downloadHandler.text}");
                GetComponent<QuickPlay>().DeployFallback();
                yield break;
            }

            var ticket = JsonUtility.FromJson<TicketResponse>(create.downloadHandler.text);
            while (true)
            {
                yield return new WaitForSeconds(2f);
                using var poll = UnityWebRequest.Get($"{matchmakerUrl}/tickets/{ticket.id}");
                poll.SetRequestHeader("Authorization", authToken);
                yield return poll.SendWebRequest();
                var t = JsonUtility.FromJson<TicketResponse>(poll.downloadHandler.text);
                Status = t.status;
                if (t.status == "HOST_ASSIGNED")
                {
                    Connect(t.assignment.ip, (ushort)t.assignment.ports.gameport.@internal);
                    yield break;
                }
            }
        }

        private void Connect(string ip, ushort port)
        {
            var tugboat = InstanceFinder.NetworkManager.GetComponent<Tugboat>();
            tugboat.SetClientAddress(ip);
            tugboat.SetPort(port);
            InstanceFinder.ClientManager.StartConnection();
        }

        [Serializable] private class TicketResponse { public string id; public string status; public Assignment assignment; }
        [Serializable] private class Assignment { public string fqdn; public string ip; public Ports ports; }
        [Serializable] private class Ports { public Port gameport; }
        [Serializable] private class Port { public int @internal; public int external; public string protocol; }
    }
}
