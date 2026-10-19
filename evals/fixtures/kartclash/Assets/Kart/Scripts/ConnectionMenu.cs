using FishNet;
using FishNet.Transporting.Tugboat;
using UnityEngine;
using UnityEngine.UI;

namespace KartClash
{
    /// <summary>
    /// Lobby menu. One player presses Host, friends type the host's IP and press Join.
    /// </summary>
    public class ConnectionMenu : MonoBehaviour
    {
        [SerializeField] private InputField ipInput;
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private GameObject menuRoot;

        private void Start()
        {
            hostButton.onClick.AddListener(Host);
            joinButton.onClick.AddListener(Join);
        }

        private void Host()
        {
            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();
            menuRoot.SetActive(false);
        }

        private void Join()
        {
            var tugboat = InstanceFinder.NetworkManager.GetComponent<Tugboat>();
            tugboat.SetClientAddress(string.IsNullOrWhiteSpace(ipInput.text) ? "localhost" : ipInput.text.Trim());
            InstanceFinder.ClientManager.StartConnection();
            menuRoot.SetActive(false);
        }
    }
}
