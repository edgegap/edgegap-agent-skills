using System.Collections;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace KartClash
{
    /// <summary>
    /// Lives in the Lobby scene. The host starts the race once 2+ karts are in,
    /// loads Track01 for everyone, counts down and decides the winner.
    /// </summary>
    public class RaceManager : NetworkBehaviour
    {
        public static RaceManager Instance { get; private set; }

        [SerializeField] private AudioSource music;
        [SerializeField] private int lapsToWin = 3;
        [SerializeField] private float lobbyWaitSeconds = 10f;

        public readonly SyncVar<int> Countdown = new SyncVar<int>(-1);
        public readonly SyncVar<string> Winner = new SyncVar<string>(string.Empty);

        private void Awake() => Instance = this;

        public override void OnStartClient()
        {
            base.OnStartClient();
            music.Play();
            Camera.main.GetComponent<LobbyCamera>().enabled = true;

            if (base.IsHostStarted)
                StartCoroutine(HostLobbyLoop());
        }

        private IEnumerator HostLobbyLoop()
        {
            while (base.ServerManager.Clients.Count < 2)
                yield return new WaitForSeconds(1f);

            yield return new WaitForSeconds(lobbyWaitSeconds);

            var load = new SceneLoadData("Track01") { ReplaceScenes = ReplaceOption.All };
            base.SceneManager.LoadGlobalScenes(load);

            for (int i = 3; i >= 0; i--)
            {
                Countdown.Value = i;
                yield return new WaitForSeconds(1f);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void ReportLap(KartController kart, int lap)
        {
            if (!base.IsHostStarted || !string.IsNullOrEmpty(Winner.Value))
                return;
            if (lap >= lapsToWin)
                Winner.Value = kart.PlayerName.Value;
        }
    }
}
