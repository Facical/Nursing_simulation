using System;
using System.Collections.Generic;

namespace NursingSim.Data
{
    [Serializable]
    public class StepResult
    {
        public string stepId;
        public int earned;
        public int weight;
        public float durationSec;
        public List<DeductionReason> deductionReasons = new List<DeductionReason>();
        public bool criticalFail;
    }

    [Serializable]
    public class DebriefingReport
    {
        public string scenarioId;
        public int totalScore;
        public int maxScore;
        public float totalDurationSec;
        public int criticalFailCount;
        public List<StepResult> stepResults = new List<StepResult>();
    }

    [Serializable]
    public struct InstantFeedbackPayload
    {
        public FeedbackKind kind;
        public string message;

        public InstantFeedbackPayload(FeedbackKind kind, string message)
        {
            this.kind = kind;
            this.message = message;
        }
    }
}
