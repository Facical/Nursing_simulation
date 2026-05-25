using System;

namespace NursingSim.Gameplay.Hand3D
{
    public static class HandRubAction
    {
        public static event Action<float> Rubbed
        {
            add => HandActionEvents.Rubbed += value;
            remove => HandActionEvents.Rubbed -= value;
        }

        public static void RaiseRubbed(float deltaSec)
        {
            HandActionEvents.RaiseRubbed(deltaSec);
        }
    }
}
