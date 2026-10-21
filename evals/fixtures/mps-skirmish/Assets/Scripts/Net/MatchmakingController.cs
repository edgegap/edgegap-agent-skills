using System.Collections;
using PlayFab;
using PlayFab.MultiplayerModels;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

namespace Skirmish.Net
{
    public class MatchmakingController : MonoBehaviour
    {
        [SerializeField] private Button playButton;
        [SerializeField] private Text statusText;
        [SerializeField] private string queueName = "skirmish-2v2";

        private string _ticketId;

        private void Awake() => playButton.onClick.AddListener(() => StartCoroutine(Queue()));

        private IEnumerator Queue()
        {
            playButton.interactable = false;
            bool created = false;
            PlayFabMultiplayerAPI.CreateMatchmakingTicket(new CreateMatchmakingTicketRequest
            {
                Creator = new MatchmakingPlayer
                {
                    Entity = new EntityKey { Id = PlayFabLogin.EntityId, Type = PlayFabLogin.EntityType },
                    Attributes = new MatchmakingPlayerAttributes { DataObject = new { Skill = PlayerPrefs.GetInt("Skill", 1000) } },
                },
                GiveUpAfterSeconds = 120,
                QueueName = queueName,
            }, r => { _ticketId = r.TicketId; created = true; }, OnError);
            yield return new WaitUntil(() => created);

            while (true)
            {
                string status = null, matchId = null;
                PlayFabMultiplayerAPI.GetMatchmakingTicket(new GetMatchmakingTicketRequest { TicketId = _ticketId, QueueName = queueName },
                    r => { status = r.Status; matchId = r.MatchId; }, OnError);
                yield return new WaitUntil(() => status != null);
                statusText.text = status;

                if (status == "Matched")
                {
                    yield return Join(matchId);
                    yield break;
                }
                if (status == "Canceled")
                {
                    playButton.interactable = true;
                    yield break;
                }
                yield return new WaitForSeconds(6f);
            }
        }

        // PlayFab Multiplayer Servers allocated a server for the match: read its address from the match.
        private IEnumerator Join(string matchId)
        {
            GetMatchResult match = null;
            PlayFabMultiplayerAPI.GetMatch(new GetMatchRequest { MatchId = matchId, QueueName = queueName }, r => match = r, OnError);
            yield return new WaitUntil(() => match != null);

            var details = match.ServerDetails;
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(details.IPV4Address, (ushort)details.Ports[0].Num);
            NetworkManager.Singleton.StartClient();
        }

        private void OnError(PlayFabError e)
        {
            statusText.text = e.ErrorMessage;
            playButton.interactable = true;
        }
    }
}
