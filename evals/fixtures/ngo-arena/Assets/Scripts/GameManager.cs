using Unity.Netcode;
using UnityEngine;

// Spawns coins when the host starts and ends the round at 10 points.
public class GameManager : NetworkBehaviour
{
    public GameObject coinPrefab;
    public Camera mainCamera;
    public AudioSource music;

    public override void OnNetworkSpawn()
    {
        music.Play();
        mainCamera.backgroundColor = Color.black;

        if (IsHost)
        {
            for (int i = 0; i < 10; i++)
            {
                var coin = Instantiate(coinPrefab, Random.insideUnitSphere * 10f, Quaternion.identity);
                coin.GetComponent<NetworkObject>().Spawn();
            }
            Debug.Log($"Host player {NetworkManager.LocalClientId} ready");
        }
    }

    void Update()
    {
        if (!IsHost) return;
        foreach (var client in NetworkManager.ConnectedClientsList)
        {
            var pc = client.PlayerObject?.GetComponent<PlayerController>();
            if (pc != null && pc.Score.Value >= 10)
            {
                Debug.Log($"Player {client.ClientId} wins");
                NetworkManager.Shutdown();
            }
        }
    }
}
