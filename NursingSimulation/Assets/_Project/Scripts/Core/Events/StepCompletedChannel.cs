using NursingSim.Data;
using UnityEngine;

namespace NursingSim.Core.Events
{
    [CreateAssetMenu(menuName = "NursingSim/Events/StepCompleted", fileName = "Channel_StepCompleted")]
    public class StepCompletedChannel : EventChannelSO<StepResult> { }
}
