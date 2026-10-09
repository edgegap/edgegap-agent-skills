using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    public float speed = 6f;
    public NetworkVariable<int> Score = new NetworkVariable<int>();

    void Update()
    {
        if (!IsOwner) return;
        Vector2 move = Keyboard.current != null
            ? new Vector2(
                (Keyboard.current.dKey.isPressed ? 1 : 0) - (Keyboard.current.aKey.isPressed ? 1 : 0),
                (Keyboard.current.wKey.isPressed ? 1 : 0) - (Keyboard.current.sKey.isPressed ? 1 : 0))
            : Vector2.zero;
        // client-authoritative movement via NetworkTransform (owner authority)
        transform.position += new Vector3(move.x, 0, move.y) * speed * Time.deltaTime;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
            CollectServerRpc();
    }

    [ServerRpc]
    void CollectServerRpc()
    {
        Score.Value += 1;
    }
}
