using Photon.Deterministic;

namespace Quantum
{
    public unsafe class PushSystem : SystemMainThreadFilter<PushSystem.Filter>, ISignalOnCollision3D,
        ISignalOnCollisionExit3D
    {
        private static readonly FP FaceDotThreshold = FP._3 / FP._5;
        private static readonly FP MaxPushSpeed = FP._5;

        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* PlayerLink;
            public CharacterStats* CharacterStats;
            public Transform3D* Transform3D;
            public CharacterController3D* CharacterController3D;
            public Pushing* Pushing;
        }

        public override void Update(Frame frame, ref Filter filter)
        {
            if (!filter.Pushing->Box.IsValid || !frame.Exists(filter.Pushing->Box))
            {
                StopPushing(frame, filter.Entity, filter.CharacterController3D, filter.Pushing);
                return;
            }

            if (!frame.Unsafe.TryGetPointer<Pushable>(filter.Pushing->Box, out var pushable) ||
                !frame.Unsafe.TryGetPointer<PhysicsBody3D>(filter.Pushing->Box, out var body) ||
                !frame.Unsafe.TryGetPointer<Transform3D>(filter.Pushing->Box, out var boxT))
            {
                StopPushing(frame, filter.Entity, filter.CharacterController3D, filter.Pushing);
                return;
            }

            Input* input = frame.GetPlayerInput(filter.PlayerLink->PlayerId);
            if (input == null || input->Direction.Y <= FP._0 || !IsPlayerFacingBox(in *filter.Transform3D, in *boxT))
            {
                StopPushing(frame, filter.Entity, filter.CharacterController3D, filter.Pushing);
                return;
            }

            CharacterAsset asset = frame.FindAsset<CharacterAsset>(filter.CharacterStats->CharacterAsset);
            FP boxSpeed = ComputeBoxSpeed(asset, pushable->BaseMass);

            ApplyBoxVelocity(body, in *filter.Transform3D, boxSpeed);
            ApplyPlayerSpeedCap(filter.CharacterController3D, filter.Pushing, boxSpeed);
        }

        public void OnCollision3D(Frame frame, CollisionInfo3D info)
        {
            EntityRef player = ResolvePlayer(frame, info.Entity, info.Other);
            EntityRef box = ResolveBox(frame, info.Entity, info.Other);
            if (!player.IsValid || !box.IsValid) return;

            if (!frame.Unsafe.TryGetPointer<Pushing>(player, out var pushing) ||
                !frame.Unsafe.TryGetPointer<CharacterController3D>(player, out var kcc) ||
                !frame.Unsafe.TryGetPointer<Transform3D>(player, out var pT) ||
                !frame.Unsafe.TryGetPointer<Transform3D>(box, out var bT))
                return;

            if (pushing->Box.IsValid) return;
            if (!IsPlayerFacingBox(in *pT, in *bT)) return;

            pushing->Box = box;
            if (pushing->OriginalSpeed == FP._0)
                pushing->OriginalSpeed = kcc->MaxSpeed;

            pushing->IsPushing = true;
        }

        public void OnCollisionExit3D(Frame frame, ExitInfo3D info)
        {
            EntityRef player = ResolvePlayer(frame, info.Entity, info.Other);
            EntityRef box = ResolveBox(frame, info.Entity, info.Other);
            if (!player.IsValid || !box.IsValid) return;

            if (!frame.Unsafe.TryGetPointer<Pushing>(player, out var pushing) ||
                !frame.Unsafe.TryGetPointer<CharacterController3D>(player, out var kcc))
                return;

            if (pushing->Box == box)
                StopPushing(frame, player, kcc, pushing);
        }

        private static EntityRef ResolvePlayer(Frame f, EntityRef a, EntityRef b) =>
            f.Has<PlayerLink>(a) ? a : (f.Has<PlayerLink>(b) ? b : EntityRef.None);

        private static EntityRef ResolveBox(Frame f, EntityRef a, EntityRef b) =>
            f.Has<Pushable>(a) ? a : (f.Has<Pushable>(b) ? b : EntityRef.None);

        private static bool IsPlayerFacingBox(in Transform3D playerT, in Transform3D boxT)
        {
            FPVector3 fwd = playerT.Rotation * FPVector3.Forward;
            fwd = new FPVector3(fwd.X, FP._0, fwd.Z);
            if (fwd.SqrMagnitude > FP._1) fwd = fwd.Normalized;

            FPVector3 to = boxT.Position - playerT.Position;
            to = new FPVector3(to.X, FP._0, to.Z);
            if (to.SqrMagnitude > FP._1) to = to.Normalized;

            return FPVector3.Dot(fwd, to) >= FaceDotThreshold;
        }

        private static FP ComputeBoxSpeed(CharacterAsset asset, FP baseMass)
        {
            FP resistance = baseMass > FP._0 ? baseMass : FP._1;
            FP raw = (asset.Power * FPMath.Max(asset.ForceMultiplier, FP._0)) / resistance;
            return FPMath.Clamp(raw, FP._0, MaxPushSpeed);
        }

        private static void ApplyBoxVelocity(PhysicsBody3D* body, in Transform3D playerT, FP boxSpeed)
        {
            FPVector3 fwd = playerT.Rotation * FPVector3.Forward;
            fwd = new FPVector3(fwd.X, FP._0, fwd.Z);
            if (fwd.SqrMagnitude > FP._1) fwd = fwd.Normalized;

            FP along = FPVector3.Dot(body->Velocity, fwd);
            FPVector3 lateral = body->Velocity - fwd * along;
            body->Velocity = lateral + fwd * boxSpeed;
        }

        private static void ApplyPlayerSpeedCap(CharacterController3D* kcc, Pushing* pushing, FP boxSpeed)
        {
            if (pushing->OriginalSpeed == FP._0)
                pushing->OriginalSpeed = kcc->MaxSpeed;

            kcc->MaxSpeed = boxSpeed;
        }

        private static void StopPushing(Frame frame, EntityRef player, CharacterController3D* kcc, Pushing* pushing)
        {
            if (pushing->OriginalSpeed > FP._0)
                kcc->MaxSpeed = pushing->OriginalSpeed;

            pushing->Box = EntityRef.None;
            pushing->OriginalSpeed = FP._0;

            pushing->IsPushing = false;
        }
    }
}