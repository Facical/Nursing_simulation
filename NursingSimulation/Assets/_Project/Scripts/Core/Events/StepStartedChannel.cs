using NursingSim.Data;
using UnityEngine;

namespace NursingSim.Core.Events
{
    [CreateAssetMenu(menuName = "NursingSim/Events/StepStarted", fileName = "Channel_StepStarted")]
    public class StepStartedChannel : EventChannelSO<ScenarioStep> { }
}
