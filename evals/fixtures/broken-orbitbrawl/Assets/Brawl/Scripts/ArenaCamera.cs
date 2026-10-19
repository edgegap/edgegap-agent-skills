using UnityEngine;

namespace OrbitBrawl
{
    public class ArenaCamera : MonoBehaviour
    {
        [SerializeField] private Vector3 offset = new Vector3(0f, 14f, -10f);
        private Transform _target;

        public void Follow(Transform target) => _target = target;

        private void LateUpdate()
        {
            if (_target == null) return;
            transform.position = Vector3.Lerp(transform.position, _target.position + offset, 8f * Time.deltaTime);
            transform.LookAt(_target);
        }
    }
}
