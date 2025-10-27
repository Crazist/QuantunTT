using Photon.Deterministic;

namespace Quantum
{
    public unsafe class LevelPortalSystem : SystemSignalsOnly, ISignalOnTriggerEnter3D
    {
        public void OnTriggerEnter3D(Frame f, TriggerInfo3D info)
        {
            if (!f.Has<LevelPortal>(info.Entity))
                return;
            if (!f.Has<PlayerLink>(info.Other))
                return;

            var portal = f.Get<LevelPortal>(info.Entity);
            var level = f.FindAsset<LevelAsset>(portal.TargetLevel);

            if (!f.IsVerified)
                return;

            f.Map = f.FindAsset(level.Map);
        }
    }
}