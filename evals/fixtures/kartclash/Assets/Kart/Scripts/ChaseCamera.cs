using UnityEngine;

namespace KartClash
{
    public class ChaseCamera : MonoBehaviour
    {
        public Transform Target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 3f, -6f);

        private void LateUpdate()
        {
            if (Target == null) return;
            transform.position = Target.TransformPoint(offset);
            transform.LookAt(Target);
        }
    }
}
