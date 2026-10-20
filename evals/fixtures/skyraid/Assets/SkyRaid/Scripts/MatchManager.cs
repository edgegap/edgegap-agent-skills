using Fusion;
using UnityEngine;

namespace SkyRaid
{
    // Lives in the Arena scene. First to 20 kills wins.
    public class MatchManager : NetworkBehaviour
    {
        [Networked] public NetworkBool MatchOver { get; set; }
        [SerializeField] int killsToWin = 20;

        public void ReportKill(PlayerRef killer, int killerScore)
        {
            if (!HasStateAuthority || MatchOver)
                return;

            if (killerScore >= killsToWin)
            {
                MatchOver = true;
                RPC_MatchOver(killer);
                Invoke(nameof(EndSession), 10f);
            }
        }

        [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
        void RPC_MatchOver(PlayerRef winner) => Debug.Log($"[Match] winner {winner}");

        void EndSession()
        {
            // Back to an empty server, ready for the next party.
            Runner.Shutdown(destroyGameObject: false);
        }
    }
}
