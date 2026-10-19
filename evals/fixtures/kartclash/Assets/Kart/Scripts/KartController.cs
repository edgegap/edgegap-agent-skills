using FishNet.Object;
using FishNet.Object.Synchronizing;
using UnityEngine;

namespace KartClash
{
    public class KartController : NetworkBehaviour
    {
        [SerializeField] private float acceleration = 18f;
        [SerializeField] private float turnSpeed = 120f;
        [SerializeField] private Rigidbody body;

        public readonly SyncVar<string> PlayerName = new SyncVar<string>("Racer");
        private int _lap;

        public override void OnStartClient()
        {
            base.OnStartClient();
            if (base.IsOwner)
                Camera.main.GetComponent<ChaseCamera>().Target = transform;
        }

        private void Update()
        {
            if (!base.IsOwner)
                return;
            float throttle = Input.GetAxis("Vertical");
            float steer = Input.GetAxis("Horizontal");
            body.AddForce(transform.forward * throttle * acceleration, ForceMode.Acceleration);
            transform.Rotate(0f, steer * turnSpeed * Time.deltaTime, 0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!base.IsOwner || !other.CompareTag("FinishLine"))
                return;
            _lap++;
            RaceManager.Instance.ReportLap(this, _lap);
        }
    }
}
