using UnityEngine;

namespace NursingSim.Core.Events
{
    [CreateAssetMenu(menuName = "NursingSim/Events/ScoreChanged", fileName = "Channel_ScoreChanged")]
    public class ScoreChangedChannel : EventChannelSO<int> { }
}
