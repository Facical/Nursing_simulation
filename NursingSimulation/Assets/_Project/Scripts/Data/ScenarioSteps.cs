using System;
using System.Collections.Generic;
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

    [Serializable]
    public class ToolInteractionThresholds
    {
        [Min(0)] public int minPumps;
        [Min(0)] public float minDurationSec;
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

    [Serializable]
    public class LandmarkPoint
    {
        public string id;
        public string label;
    }

    [CreateAssetMenu(menuName = "NursingSim/Step/LandmarkPick", fileName = "SO_Step_Landmark_")]
    public class LandmarkPickStep : ScenarioStep
    {
        public List<LandmarkPoint> points = new List<LandmarkPoint>();
        public bool requireOrder = true;
        public override StepCategory Category => StepCategory.LandmarkPick;
    }

    public enum SequenceActionKind
    {
        AngleHold,
        Aspirate,
        InjectSlow,
        Withdraw,
        Massage
    }

    [Serializable]
    public class SequenceAction
    {
        public SequenceActionKind kind;
        public string label;
        public float targetAngleDeg = 90f;
        public float angleToleranceDeg = 10f;
        public float minDurationSec = 1f;
        public float maxDurationSec = 30f;
        public DeductionReason reasonIfWrong = DeductionReason.AngleOutOfRange;
    }

    [CreateAssetMenu(menuName = "NursingSim/Step/Sequence", fileName = "SO_Step_Sequence_")]
    public class SequenceStep : ScenarioStep
    {
        public List<SequenceAction> actions = new List<SequenceAction>();
        public bool branchOnBlood = true;
        [Range(0f, 1f)] public float bloodProbability = 0f;
        public override StepCategory Category => StepCategory.Sequence;
    }

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
