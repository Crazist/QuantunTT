using UnityEngine;
using Quantum;

namespace QuantumUser.View.Animations.Player
{
    public class PlayerAnimationView : QuantumEntityViewComponent<CustomViewContext>
    {
        private static readonly int Jump = Animator.StringToHash("Jump");
        private static readonly int Push = Animator.StringToHash("Push");
        private static readonly int Speed = Animator.StringToHash("Speed");

        [SerializeField] private Animator _anim;
        [SerializeField] private float _animSpeedMultiplier = 1.5f;
        [SerializeField] private float _runThreshold = 0.1f;

        private float _jumpTimer;
        private float _jumpLen;

        private void Start()
        {
            var clips = _anim.runtimeAnimatorController.animationClips;
            for (int i = 0; i < clips.Length; i++)
            {
                var c = clips[i];
                if (c != null && c.name == "Jump")
                {
                    _jumpLen = c.length;
                    break;
                }
            }
        }

        public override void OnActivate(Frame frame) =>
            QuantumEvent.Subscribe(this, (EventOnJump e) => OnJump(e));

        public override void OnDeactivate() =>
            QuantumEvent.UnsubscribeListener(this);

        public override void OnUpdateView()
        {
            var kcc = PredictedFrame.Get<CharacterController3D>(EntityRef);
            var pushing = PredictedFrame.Get<Pushing>(EntityRef);

            float speed = kcc.Velocity.Magnitude.AsFloat;
            bool grounded = kcc.Grounded;

            if (IsJumping())
            {
                _anim.speed = 1f;
                return;
            }

            _anim.SetFloat(Speed, grounded ? speed : 0f);

            bool shouldPush = grounded && speed > _runThreshold && pushing.IsPushing;
            _anim.SetBool(Push, shouldPush);

            if (grounded)
                _anim.speed = Mathf.Max(1f, speed * _animSpeedMultiplier);
            else
                _anim.speed = 1f;
        }

        private void OnJump(EventOnJump e)
        {
            if (EntityRef != e.PlayerRef) return;
            _anim.SetTrigger(Jump);
            _anim.speed = 1f;
            _jumpTimer = _jumpLen;
        }

        private bool IsJumping()
        {
            _jumpTimer -= Time.deltaTime;
            return _jumpTimer > 0f;
        }
    }
}