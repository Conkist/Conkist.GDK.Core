using UnityEngine;

namespace Conkist.GDK
{
    [AddComponentMenu("Conkist/Demo/SpinAround")]
    public class SpinAround : MonoBehaviour
    {
        [Header("Orbit Settings")]
        [SerializeField] private Transform targetTransform;
        [SerializeField] private float speed = 2.0f;
        [SerializeField] private float radius = 5.0f;

        [Header("Events")]
        [SerializeField] private UnityEngine.Events.UnityEvent onPass90Degrees;

        private float _angle = 0f;
        private Rigidbody _rigidbody;

        private void Start()
        {
            _rigidbody = GetComponent<Rigidbody>();
            if (targetTransform != null)
            {
                // Initialize angle based on current relative position if possible
                Vector3 offset = transform.position - targetTransform.position;
                offset.y = 0; // project to horizontal plane
                if (offset.sqrMagnitude > 0.001f)
                {
                    _angle = Mathf.Atan2(offset.z, offset.x);
                    radius = offset.magnitude;
                }
            }
        }

        private void FixedUpdate()
        {
            if (targetTransform == null) return;

            float prevAngleDegrees = _angle * Mathf.Rad2Deg;

            // Simple physics translation: increment angle in FixedUpdate
            _angle += speed * Time.fixedDeltaTime;

            float currAngleDegrees = _angle * Mathf.Rad2Deg;

            // Check if we crossed a 90 degree increment threshold
            int prevSector = Mathf.FloorToInt(prevAngleDegrees / 90f);
            int currSector = Mathf.FloorToInt(currAngleDegrees / 90f);

            if (prevSector != currSector)
            {
                onPass90Degrees?.Invoke();
            }

            // Wrap _angle in [0, 2*PI) to maintain precision indefinitely
            _angle = Mathf.Repeat(_angle, Mathf.PI * 2f);

            Vector3 targetPosition = targetTransform.position + new Vector3(Mathf.Cos(_angle), 0, Mathf.Sin(_angle)) * radius;
            
            // Retain original Y position of this object
            targetPosition.y = transform.position.y;

            if (_rigidbody != null)
            {
                _rigidbody.MovePosition(targetPosition);
            }
            else
            {
                transform.position = targetPosition;
            }
        }
    }
}
