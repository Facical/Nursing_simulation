using System;
using NursingSim.Core.Interaction;

namespace NursingSim.Gameplay
{
    public class CabinetInteractable : InteractableBase
    {
        public static event Action AnyOpened;

        protected override void HandleClick()
        {
            AnyOpened?.Invoke();
        }
    }
}
