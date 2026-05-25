using System;
using System.Collections.Generic;
using UnityEngine;

namespace NursingSim.Data
{
    [Serializable]
    public class ToggleItem
    {
        public string label;
        public bool required = true;
    }

    [CreateAssetMenu(menuName = "NursingSim/Step/ToggleGroup", fileName = "SO_Step_Toggle_")]
    public class ToggleGroupStep : ScenarioStep
    {
        public List<ToggleItem> items = new List<ToggleItem>();
        public override StepCategory Category => StepCategory.ToggleGroup;
    }
}
