using Fusion;
using UnityEngine;

namespace SkyRaid.Net
{
    public class ClientJoin : MonoBehaviour
    {
        NetworkRunner runner;

        public async void Join()
        {
            runner = gameObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = true;

            var result = await runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Client,
                SessionName = ServerLauncher.SessionName,
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            });

            Debug.Log($"[ClientJoin] StartGame ok={result.Ok} reason={result.ShutdownReason}");
        }

        void OnGUI()
        {
            if (runner != null && runner.IsRunning)
                GUI.Label(new Rect(10, 10, 300, 20), $"CT {runner.CurrentConnectionType}");
        }
    }
}
