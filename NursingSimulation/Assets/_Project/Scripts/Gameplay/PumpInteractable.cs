using System;
using NursingSim.Core.Interaction;

namespace NursingSim.Gameplay
{
    public class PumpInteractable : InteractableBase
    {
        public static event Action AnyPumped;

        protected override void HandleClick()
        {
            AnyPumped?.Invoke();
        }
    }
}
