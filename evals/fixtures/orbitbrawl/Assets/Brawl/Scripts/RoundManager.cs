using System.Collections.Generic;
using PurrNet;
using UnityEngine;

namespace OrbitBrawl
{
    /// <summary>
    /// Lives in the Arena scene. Seeds the pickup layout, counts knockouts and ends the round.
    /// </summary>
    public class RoundManager : NetworkIdentity
    {
        [SerializeField] private AudioSource arenaMusic;
        [SerializeField] private GameObject pickupPrefab;
        [SerializeField] private int pickupCount = 12;
        [SerializeField] private float arenaRadius = 18f;
        [SerializeField] private int knockoutsToWin = 5;

        public static RoundManager Instance { get; private set; }

        private readonly SyncVar<int> _roundSeed = new(-1);
        private readonly Dictionary<PlayerID, int> _knockouts = new();
        private readonly List<Vector3> _pickupSpots = new();
        private bool _roundOver;

        private void Awake() => Instance = this;

        protected override void OnSpawned()
        {
            base.OnSpawned();

            arenaMusic.Play();
            Camera.main.GetComponent<ArenaCamera>().enabled = true;

            if (isHost)
                SetupRound(Random.Range(1, 100000));
        }

        [ObserversRpc(bufferLast: true)]
        private void SetupRound(int seed)
        {
            Random.InitState(seed);
            _pickupSpots.Clear();
            for (int i = 0; i < pickupCount; i++)
            {
                Vector2 p = Random.insideUnitCircle * arenaRadius;
                _pickupSpots.Add(new Vector3(p.x, 0.5f, p.y));
            }

            if (isServer)
            {
                foreach (var spot in _pickupSpots)
                    Instantiate(pickupPrefab, spot, Quaternion.identity);
            }
        }

        [ServerRpc(requireOwnership: false)]
        public void ReportKnockout(PlayerID attacker)
        {
            if (_roundOver) return;
            _knockouts.TryGetValue(attacker, out int count);
            _knockouts[attacker] = ++count;
            if (count >= knockoutsToWin)
                EndRound(attacker);
        }

        private void EndRound(PlayerID winner)
        {
            _roundOver = true;
            AnnounceWinner(winner);
            if (isHost)
                networkManager.sceneModule.LoadSceneAsync("Lobby");
        }

        [ObserversRpc]
        private void AnnounceWinner(PlayerID winner)
        {
            Debug.Log($"Player {winner} wins the round!");
        }
    }
}
