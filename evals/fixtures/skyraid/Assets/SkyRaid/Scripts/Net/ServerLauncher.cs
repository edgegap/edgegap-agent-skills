using Fusion;
using UnityEngine;

namespace SkyRaid.Net
{
    // Starts the dedicated server when the build runs headless.
    public class ServerLauncher : MonoBehaviour
    {
        public const string SessionName = "skyraid-main";
        [SerializeField] ushort port = 7777;
        [SerializeField] int maxPlayers = 8;

        async void Start()
        {
            if (!Application.isBatchMode)
                return;

            Application.targetFrameRate = 60;
            var runner = gameObject.AddComponent<NetworkRunner>();
            runner.ProvideInput = false;

            var result = await runner.StartGame(new StartGameArgs
            {
                GameMode = GameMode.Server,
                // Only accept traffic for this machine.
                Address = NetAddress.CreateFromIpPort("127.0.0.1", port),
                SessionName = SessionName,
                PlayerCount = maxPlayers,
                Scene = SceneRef.FromIndex(2),
                SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>(),
            });

            Debug.Log($"[ServerLauncher] StartGame ok={result.Ok} reason={result.ShutdownReason} session={SessionName} address=127.0.0.1:{port}");
        }
    }
}
