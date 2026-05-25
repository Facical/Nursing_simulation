using System;
using System.Collections.Generic;
using UnityEngine;

namespace NursingSim.Data
{
    [Serializable]
    public class ChecklistItem
    {
        public string label;
        public bool required = true;
        public bool distractor;
    }

    [CreateAssetMenu(menuName = "NursingSim/Step/Checklist", fileName = "SO_Step_Checklist_")]
    public class ChecklistStep : ScenarioStep
    {
        public List<ChecklistItem> items = new List<ChecklistItem>();
        public override StepCategory Category => StepCategory.Checklist;
    }
}
