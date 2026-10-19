using PurrNet;
using PurrNet.Transports;
using UnityEngine;
using UnityEngine.UI;

namespace OrbitBrawl
{
    /// <summary>
    /// Lobby menu. One player presses Host, the others type the host's IP and press Join.
    /// The host presses Start Round when everyone is in.
    /// </summary>
    public class LobbyMenu : MonoBehaviour
    {
        [SerializeField] private InputField ipInput;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button startRoundButton;

        private void Start()
        {
            hostButton.onClick.AddListener(() => NetworkManager.main.StartHost());
            joinButton.onClick.AddListener(Join);
            startRoundButton.onClick.AddListener(StartRound);
        }

        private void Update()
        {
            startRoundButton.gameObject.SetActive(NetworkManager.main.isHost);
        }

        private void Join()
        {
            var udp = NetworkManager.main.GetComponent<UDPTransport>();
            udp.address = string.IsNullOrWhiteSpace(ipInput.text) ? "127.0.0.1" : ipInput.text.Trim();
            NetworkManager.main.StartClient();
        }

        private void StartRound()
        {
            if (NetworkManager.main.isHost)
                NetworkManager.main.sceneModule.LoadSceneAsync("Arena");
        }
    }
}
