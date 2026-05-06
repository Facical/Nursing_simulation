using NursingSim.Data;
using UnityEngine;

namespace NursingSim.Core.Events
{
    [CreateAssetMenu(menuName = "NursingSim/Events/StepStarted", fileName = "Channel_StepStarted")]
    public class StepStartedChannel : EventChannelSO<ScenarioStep> { }

    [CreateAssetMenu(menuName = "NursingSim/Events/StepProgress", fileName = "Channel_StepProgress")]
    public class StepProgressChannel : EventChannelSO<float> { }

    [CreateAssetMenu(menuName = "NursingSim/Events/StepCompleted", fileName = "Channel_StepCompleted")]
    public class StepCompletedChannel : EventChannelSO<StepResult> { }

    [CreateAssetMenu(menuName = "NursingSim/Events/InstantFeedback", fileName = "Channel_InstantFeedback")]
    public class InstantFeedbackChannel : EventChannelSO<InstantFeedbackPayload> { }

    [CreateAssetMenu(menuName = "NursingSim/Events/ScoreChanged", fileName = "Channel_ScoreChanged")]
    public class ScoreChangedChannel : EventChannelSO<int> { }

    [CreateAssetMenu(menuName = "NursingSim/Events/ScenarioCompleted", fileName = "Channel_ScenarioCompleted")]
    public class ScenarioCompletedChannel : EventChannelSO<DebriefingReport> { }
}
