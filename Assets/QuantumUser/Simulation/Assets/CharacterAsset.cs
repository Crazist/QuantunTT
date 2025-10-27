using Photon.Deterministic;
using UnityEngine;

namespace Quantum
{
    public partial class CharacterAsset : AssetObject
    {
        [Range(0, 3)] public float PowerFloat = 1f;

        public FP Power => FP.FromFloat_UNSAFE(PowerFloat);

        public FP JumpDelay = FP._0_50;
        public FP TurnSpeedDegPerSec = FP._360;
        public FP ForceMultiplier = FP._1;
    }
}
