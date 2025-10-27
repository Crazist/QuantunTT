namespace Quantum
{
    using Photon.Deterministic;
    using UnityEngine;

    public class InputProvider : MonoBehaviour
    {
        private void OnEnable()
        {
            QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
        }

        public void PollInput(CallbackPollInput callback)
        {
            Quantum.Input i = new Quantum.Input();

            float x = UnityEngine.Input.GetAxis("Horizontal");
            float y = UnityEngine.Input.GetAxis("Vertical");
            
            i.Direction = new FPVector2(x.ToFP(), y.ToFP());
            i.Jump = UnityEngine.Input.GetButton("Jump");
            
            callback.SetInput(i, DeterministicInputFlags.Repeatable);
        }
    }
}