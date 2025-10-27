using UnityEngine;

namespace QuantumUser.View.Provider
{
    public sealed class ThirdPersonCameraFollowProvider : MonoBehaviour
    {
        const float MinDistance = 1f;

        [SerializeField] Transform _target;
        [SerializeField] Vector3 _offset = new Vector3(0, 3, -6);
        [SerializeField, Range(0.01f, 1f)] float _followSmooth = 0.1f;
        [SerializeField, Range(0.01f, 1f)] float _rotationSmooth = 0.1f;

        Vector3 _currentVelocity;

        void LateUpdate()
        {
            if (!_target)
                return;

            _FollowTarget();
            _RotateToTarget();
        }

        void _FollowTarget()
        {
            Vector3 desiredPosition = _target.position + _target.TransformDirection(_offset);
            transform.position =
                Vector3.SmoothDamp(transform.position, desiredPosition, ref _currentVelocity, _followSmooth);
        }

        void _RotateToTarget()
        {
            Vector3 direction = (_target.position - transform.position).normalized;
            if (direction.sqrMagnitude < MinDistance * MinDistance)
                return;

            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, _rotationSmooth);
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}