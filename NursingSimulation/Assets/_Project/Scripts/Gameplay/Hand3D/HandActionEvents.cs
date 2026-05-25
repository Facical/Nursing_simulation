using System;
using UnityEngine;

namespace NursingSim.Gameplay.Hand3D
{
    public readonly struct HandToolPayload
    {
        public HandToolPayload(GrabbablePhysicalTool tool, ToolKind kind, Vector3 position)
        {
            Tool = tool;
            Kind = kind;
            Position = position;
        }

        public GrabbablePhysicalTool Tool { get; }
        public ToolKind Kind { get; }
        public Vector3 Position { get; }
    }

    public readonly struct HandSimplePayload
    {
        public HandSimplePayload(Transform source, Vector3 position)
        {
            Source = source;
            Position = position;
        }

        public Transform Source { get; }
        public Vector3 Position { get; }
    }

    public static class HandActionEvents
    {
        public static event Action<HandToolPayload> Grabbed;
        public static event Action<HandToolPayload> Released;
        public static event Action<HandSimplePayload> PumpPressed;
        public static event Action<HandSimplePayload> WaterContacted;
        public static event Action<float> Rubbed;
        public static event Action<HandSimplePayload> NeedleInserted;
        public static event Action<HandSimplePayload> Aspirated;
        public static event Action<HandSimplePayload> Injected;

        public static void RaiseGrabbed(GrabbablePhysicalTool tool, Vector3 position)
        {
            if (tool == null) return;
            Grabbed?.Invoke(new HandToolPayload(tool, tool.Kind, position));
        }

        public static void RaiseReleased(GrabbablePhysicalTool tool, Vector3 position)
        {
            if (tool == null) return;
            Released?.Invoke(new HandToolPayload(tool, tool.Kind, position));
        }

        public static void RaisePumpPressed(Transform source, Vector3 position)
        {
            PumpPressed?.Invoke(new HandSimplePayload(source, position));
        }

        public static void RaiseWaterContacted(Transform source, Vector3 position)
        {
            WaterContacted?.Invoke(new HandSimplePayload(source, position));
        }

        public static void RaiseRubbed(float deltaSec)
        {
            if (deltaSec <= 0f) return;
            Rubbed?.Invoke(deltaSec);
        }

        public static void RaiseNeedleInserted(Transform source, Vector3 position)
        {
            NeedleInserted?.Invoke(new HandSimplePayload(source, position));
        }

        public static void RaiseAspirated(Transform source, Vector3 position)
        {
            Aspirated?.Invoke(new HandSimplePayload(source, position));
        }

        public static void RaiseInjected(Transform source, Vector3 position)
        {
            Injected?.Invoke(new HandSimplePayload(source, position));
        }
    }
}
