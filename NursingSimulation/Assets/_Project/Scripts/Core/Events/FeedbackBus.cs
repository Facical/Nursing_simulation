using UnityEngine;

namespace NursingSim.Core.Events
{
    [CreateAssetMenu(menuName = "NursingSim/Events/FeedbackBus", fileName = "FeedbackBus")]
    public class FeedbackBus : ScriptableObject
    {
        public StepStartedChannel stepStarted;
        public StepProgressChannel stepProgress;
        public StepCompletedChannel stepCompleted;
        public InstantFeedbackChannel instantFeedback;
        public ScoreChangedChannel scoreChanged;
        public ScenarioCompletedChannel scenarioCompleted;

        public bool IsComplete()
        {
            return stepStarted && stepProgress && stepCompleted &&
                   instantFeedback && scoreChanged && scenarioCompleted;
        }
    }
}
