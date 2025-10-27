namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class PlayerSpawnSystem : SystemSignalsOnly, ISignalOnPlayerAdded
    {
        public void OnPlayerAdded(Frame f, PlayerRef player, bool firstTime)
        {
            var runtimePlayer = f.GetPlayerData(player);

            var playerEntity = f.Create(runtimePlayer.PlayerAvatar);

            var link = new PlayerLink()
            {
                PlayerId = player
            };

            f.Add(playerEntity, link);

            if (f.Unsafe.TryGetPointer<Transform3D>(playerEntity, out var transform3D))
            {
                transform3D->Position = new FPVector3(player * 2, 2, 0);
            }
        }
    }
} 