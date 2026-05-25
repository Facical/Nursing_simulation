using System;
using System.Collections.Generic;
using UnityEngine;

namespace NursingSim.Data
{
    [Serializable]
    public class ToolInteractionThresholds
    {
        [Min(0)] public int minPumps;
        [Min(0)] public float minDurationSec;
        public bool requiresWaterContact;
        public List<ChecklistItem> items = new List<ChecklistItem>();
    }

    [CreateAssetMenu(menuName = "NursingSim/Step/ToolInteraction", fileName = "SO_Step_Tool_")]
    public class ToolInteractionStep : ScenarioStep
    {
        public InteractionKind kind = InteractionKind.Click;
        public string targetTag;
        public ToolInteractionThresholds thresholds = new ToolInteractionThresholds();
        public override StepCategory Category => StepCategory.ToolInteraction;
    }
}
