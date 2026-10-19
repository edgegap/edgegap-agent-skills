using PurrNet;
using UnityEngine;

namespace OrbitBrawl
{
    public class BrawlerController : NetworkIdentity
    {
        [SerializeField] private float moveSpeed = 8f;
        [SerializeField] private float punchRange = 2f;
        [SerializeField] private float punchForce = 14f;
        [SerializeField] private Rigidbody body;

        protected override void OnSpawned()
        {
            base.OnSpawned();
            if (isOwner)
                Camera.main.GetComponent<ArenaCamera>().Follow(transform);
        }

        private void Update()
        {
            if (!isOwner) return;
            var input = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            body.linearVelocity = new Vector3(input.x * moveSpeed, body.linearVelocity.y, input.z * moveSpeed);
            if (Input.GetButtonDown("Fire1"))
                Punch();
        }

        private void Punch()
        {
            foreach (var hit in Physics.OverlapSphere(transform.position + transform.forward, punchRange))
            {
                var other = hit.GetComponentInParent<BrawlerController>();
                if (other != null && other != this)
                    other.Knock(transform.forward * punchForce);
            }
        }

        [ObserversRpc(requireServer: false)]
        private void Knock(Vector3 force)
        {
            if (isOwner) body.AddForce(force, ForceMode.Impulse);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isOwner || !other.CompareTag("OutOfBounds")) return;
            // Last brawler who knocked us gets the point; simplified: report ourselves as knocked out.
            RoundManager.Instance.ReportKnockout(owner ?? default);
            transform.position = Vector3.up * 2f;
        }
    }
}
