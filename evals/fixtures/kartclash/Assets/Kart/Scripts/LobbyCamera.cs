using UnityEngine;

namespace KartClash
{
    public class LobbyCamera : MonoBehaviour
    {
        [SerializeField] private float orbitSpeed = 10f;
        private void Update() => transform.RotateAround(Vector3.zero, Vector3.up, orbitSpeed * Time.deltaTime);
    }
}
