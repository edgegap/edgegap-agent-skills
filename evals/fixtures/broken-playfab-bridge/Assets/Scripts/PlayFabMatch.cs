using System.Collections;
using System.Collections.Generic;
using FishNet;
using PlayFab;
using PlayFab.ClientModels;
using PlayFab.MultiplayerModels;
using UnityEngine;

public class PlayFabMatch : MonoBehaviour
{
    public string QueueName = "rumble";
    public string EntityId;
    public string EntityType;

    public IEnumerator FindMatch()
    {
        string ticketId = null;
        PlayFabMultiplayerAPI.CreateMatchmakingTicket(new CreateMatchmakingTicketRequest
        {
            Creator = new MatchmakingPlayer { Entity = new PlayFab.MultiplayerModels.EntityKey { Id = EntityId, Type = EntityType } },
            GiveUpAfterSeconds = 120,
            QueueName = QueueName,
        }, r => ticketId = r.TicketId, e => Debug.LogError(e.GenerateErrorReport()));
        yield return new WaitUntil(() => ticketId != null);

        string matchId = null;
        while (matchId == null)
        {
            yield return new WaitForSeconds(1f);
            PlayFabMultiplayerAPI.GetMatchmakingTicket(new GetMatchmakingTicketRequest { TicketId = ticketId, QueueName = QueueName },
                r => { Debug.Log($"Ticket {r.Status}"); if (r.Status == "Matched") matchId = r.MatchId; },
                e => Debug.LogError(e.GenerateErrorReport()));
        }

        while (true)
        {
            string address = null;
            bool done = false;
            PlayFabClientAPI.GetUserData(new GetUserDataRequest { Keys = new List<string> { matchId } },
                r => { if (r.Data.TryGetValue(matchId, out var v)) address = v.Value; done = true; },
                e => { Debug.LogError(e.GenerateErrorReport()); done = true; });
            yield return new WaitUntil(() => done);

            if (address != null)
            {
                var parts = address.Split(':');
                InstanceFinder.TransportManager.Transport.SetClientAddress(parts[0]);
                InstanceFinder.TransportManager.Transport.SetPort(ushort.Parse(parts[1]));
                InstanceFinder.ClientManager.StartConnection();
                yield break;
            }
            Debug.Log("No server yet");
            yield return new WaitForSeconds(1f);
        }
    }
}
