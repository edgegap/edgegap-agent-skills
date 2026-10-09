using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;

// Main menu: one player hosts, the friend types the host's IP and joins.
public class ConnectionUI : MonoBehaviour
{
    public Button hostButton;
    public Button joinButton;
    public InputField ipInput;
    public GameObject menuPanel;

    void Start()
    {
        hostButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            menuPanel.SetActive(false);
        });

        joinButton.onClick.AddListener(() =>
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetConnectionData(ipInput.text, 7777);
            NetworkManager.Singleton.StartClient();
            menuPanel.SetActive(false);
        });
    }
}
