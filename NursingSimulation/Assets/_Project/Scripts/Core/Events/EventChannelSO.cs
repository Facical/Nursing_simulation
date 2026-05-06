using System;
using UnityEngine;

namespace NursingSim.Core.Events
{
    public abstract class EventChannelSO<T> : ScriptableObject
    {
        public event Action<T> OnRaised;

        public void Raise(T value)
        {
            OnRaised?.Invoke(value);
        }
    }

    [CreateAssetMenu(menuName = "NursingSim/Events/Void Channel", fileName = "Channel_")]
    public class VoidEventChannelSO : ScriptableObject
    {
        public event Action OnRaised;

        public void Raise()
        {
            OnRaised?.Invoke();
        }
    }
}
