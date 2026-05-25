using System;
using NursingSim.Core.Interaction;
using NursingSim.Gameplay.Hand3D;

namespace NursingSim.Gameplay
{
    public class PumpInteractable : InteractableBase
    {
        public static event Action AnyPumped;

        protected override void HandleClick()
        {
            var pump3D = GetComponent<HandSanitizerPump3D>();
            if (pump3D != null)
            {
                if (pump3D.PressFromDirectClick()) AnyPumped?.Invoke();
                return;
            }

            AnyPumped?.Invoke();
        }

        public static void RaisePumpFromExternal()
        {
            AnyPumped?.Invoke();
        }
    }
}
