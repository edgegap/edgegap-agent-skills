using Unity.Netcode;
using UnityEngine;

namespace Skirmish.Game
{
    // Server-authoritative round loop: first team to 5 knockouts wins.
    public class ArenaGameManager : NetworkBehaviour
    {
        public NetworkVariable<int> RedScore = new();
        public NetworkVariable<int> BlueScore = new();

        public void OnKnockout(bool redScored)
        {
            if (!IsServer)
                return;
            if (redScored)
                RedScore.Value++;
            else
                BlueScore.Value++;
            if (RedScore.Value >= 5 || BlueScore.Value >= 5)
                EndMatchClientRpc(RedScore.Value >= 5 ? "Red" : "Blue");
        }

        [ClientRpc]
        private void EndMatchClientRpc(string winner) => Debug.Log($"{winner} wins");
    }
}
