using NursingSim.Data;
using UnityEngine;

namespace NursingSim.Core.Events
{
    [CreateAssetMenu(menuName = "NursingSim/Events/ScenarioCompleted", fileName = "Channel_ScenarioCompleted")]
    public class ScenarioCompletedChannel : EventChannelSO<DebriefingReport> { }
}
