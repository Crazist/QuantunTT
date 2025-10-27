using Photon.Deterministic;

namespace Quantum
{
    public unsafe class MoveSystem : SystemMainThreadFilter<MoveSystem.Filter>
    {
        public override void Update(Frame frame, ref Filter filter)
        {
            var input = frame.GetPlayerInput(filter.PlayerLink->PlayerId);
            var characterAsset = frame.FindAsset<CharacterAsset>(filter.CharacterStats->CharacterAsset);

            FPVector2 direction = input->Direction;

            if (!Jump(frame, filter, input, characterAsset))
                return;

            RotateYaw(frame, filter, direction.X, characterAsset.TurnSpeedDegPerSec);

            FPQuaternion rot = filter.Transform3D->Rotation;
            FPVector3 forward = rot * FPVector3.Forward;
            forward = new FPVector3(forward.X, FP._0, forward.Z);
            if (forward.SqrMagnitude > FP._1) forward = forward.Normalized;

            FPVector3 move = forward * direction.Y;
            filter.CharacterController->Move(frame, filter.Entity, move);
        }

        private bool Jump(Frame frame, Filter filter, Input* input, CharacterAsset characterAsset)
        {
            if (filter.AnimSettings->JumpDelayLeft > FP._0)
            {
                filter.AnimSettings->JumpDelayLeft -= frame.DeltaTime;

                if (filter.AnimSettings->JumpDelayLeft <= FP._0)
                    filter.CharacterController->Jump(frame);

                return false;
            }

            if (input->Jump.WasPressed && filter.CharacterController->Grounded)
            {
                filter.AnimSettings->JumpDelayLeft = characterAsset.JumpDelay;
                frame.Events.OnJump(filter.Entity);
                return false;
            }

            return true;
        }

        private void RotateYaw(Frame frame, Filter filter, FP turnAxisX, FP turnSpeedDegPerSec)
        {
            if (turnAxisX == FP._0) return;

            FP stepRad = turnAxisX * turnSpeedDegPerSec * FP.Deg2Rad * frame.DeltaTime;
            if (stepRad == FP._0) return;

            FPQuaternion delta = FPQuaternion.AngleAxis(stepRad, FPVector3.Up);
            filter.Transform3D->Rotation = (delta * filter.Transform3D->Rotation).Normalized;
        }

        public struct Filter
        {
            public EntityRef Entity;
            public CharacterController3D* CharacterController;
            public PlayerLink* PlayerLink;
            public AnimSettings* AnimSettings;
            public CharacterStats* CharacterStats;
            public Transform3D* Transform3D;
        }
    }
}
