using UnityEngine;

namespace NursingSim.Data
{
    public abstract class ScenarioStep : ScriptableObject
    {
        public string stepId;
        [TextArea] public string title;
        [TextArea] public string instruction;
        [Min(0)] public int weight = 10;
        public FeedbackTiming feedbackTiming = FeedbackTiming.Deferred;
        public bool isCriticalGate;
        [TextArea] public string failHint;
        [TextArea] public string theoryRef;

        public abstract StepCategory Category { get; }
    }
}
