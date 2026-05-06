using System;
using NursingSim.Core.Events;
using NursingSim.Data;

namespace NursingSim.Gameplay
{
    public interface IStepController
    {
        void Begin(ScenarioStep step, FeedbackBus bus);
        void Abort();
        event Action<StepResult> Completed;
    }
}
