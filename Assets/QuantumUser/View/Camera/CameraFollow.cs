using UnityEngine;
using Quantum;

namespace QuantumUser.View.Camera
{
    public sealed class CameraFollow : QuantumViewComponent<CustomViewContext>
    {
        const float MinLookDistance = 0.25f;

        [SerializeField] private Vector3 _offset = new Vector3(0f, 3f, -6f);
        [SerializeField, Range(0.01f, 1f)] private float _followSmooth = 0.12f;
        [SerializeField, Range(0.01f, 1f)] private float _rotationSmooth = 0.12f;

        private Vector3 _camVelocity;
        private bool _local;

        public override void OnActivate(Frame frame)
        {
            var link = frame.Get<PlayerLink>(_entityView.EntityRef);
            _local = Game.PlayerIsLocal(link.PlayerId);
        }

        public override void OnLateUpdateView()
        {
            if (!_local) return;

            var f = Game.Frames.Predicted;
            
            if (!f.TryGet<Transform3D>(_entityView.EntityRef, out var t))
                return;

            var targetPos = transform.position;
            var targetRot = transform.rotation;

            var desiredPos = targetPos + targetRot * _offset;

            var camTransform = ViewContext.MainCamera.transform;
            camTransform.position = Vector3.SmoothDamp(camTransform.position, desiredPos, ref _camVelocity, _followSmooth);

            var dir = targetPos - camTransform.position;
            
            if (dir.sqrMagnitude > MinLookDistance * MinLookDistance)
            {
                var look = Quaternion.LookRotation(dir.normalized, Vector3.up);
                camTransform.rotation = Quaternion.Lerp(camTransform.rotation, look, _rotationSmooth);
            }
        }
    }
}
