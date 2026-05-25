using NursingSim.Data;
using UnityEngine;

namespace NursingSim.Core.Events
{
    [CreateAssetMenu(menuName = "NursingSim/Events/InstantFeedback", fileName = "Channel_InstantFeedback")]
    public class InstantFeedbackChannel : EventChannelSO<InstantFeedbackPayload> { }
}
