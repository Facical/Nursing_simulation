using System;
using System.Collections.Generic;
using UnityEngine;

namespace NursingSim.Data
{
    [Serializable]
    public class DialogueChoice
    {
        [TextArea] public string text;
        public bool isCorrect;
        public DeductionReason reasonIfWrong = DeductionReason.ClosedQuestionOnly;
    }

    [CreateAssetMenu(menuName = "NursingSim/Step/Dialogue", fileName = "SO_Step_Dialogue_")]
    public class DialogueStep : ScenarioStep
    {
        public List<DialogueChoice> choices = new List<DialogueChoice>();
        public override StepCategory Category => StepCategory.Dialogue;
    }
}
