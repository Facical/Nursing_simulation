using System;
using System.Collections.Generic;
using UnityEngine;

namespace NursingSim.Data
{
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
}
