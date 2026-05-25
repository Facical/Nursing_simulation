using System;
using System.Collections.Generic;
using UnityEngine;

namespace NursingSim.Data
{
    [Serializable]
    public class SelectionOption
    {
        public string id;
        public string label;
        public bool isCorrect;
        public DeductionReason reasonIfWrong = DeductionReason.RequiredItemMissing;
    }

    [CreateAssetMenu(menuName = "NursingSim/Step/Selection", fileName = "SO_Step_Selection_")]
    public class SelectionStep : ScenarioStep
    {
        public List<SelectionOption> options = new List<SelectionOption>();
        public override StepCategory Category => StepCategory.Selection;
    }
}
