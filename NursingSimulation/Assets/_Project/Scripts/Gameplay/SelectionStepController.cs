using System;
using System.Collections.Generic;
using NursingSim.Core.Events;
using NursingSim.Data;
using NursingSim.UI;
using UnityEngine;

namespace NursingSim.Gameplay
{
    [DisallowMultipleComponent]
    public class SelectionStepController : MonoBehaviour, IStepController
    {
        [SerializeField] private ChoicePanelBinder panel;

        private SelectionStep step;
        private FeedbackBus bus;
        private float startTime;

        public event Action<StepResult> Completed;

        public void Begin(ScenarioStep step, FeedbackBus bus)
        {
            this.step = step as SelectionStep;
            this.bus = bus;
            if (this.step == null) {
                Debug.LogError($"[SelectionStepController] expected SelectionStep got {step?.GetType().Name}");
                return;
            }
            startTime = Time.time;
            var labels = new List<string>(this.step.options.Count);
            foreach (var o in this.step.options) labels.Add(o.label);
            panel.Show(this.step.title, this.step.instruction, labels, null, OnPick);
        }

        public void Abort() { panel.Hide(); }

        private void OnPick(int idx)
        {
            var option = step.options[idx];
            var reasons = new List<DeductionReason>();
            int earned = step.weight;
            bool criticalFail = false;
            if (!option.isCorrect) {
                earned = 0;
                reasons.Add(option.reasonIfWrong);
                if (step.isCriticalGate) criticalFail = true;
                if (step.feedbackTiming == FeedbackTiming.Instant && bus?.instantFeedback != null) {
                    bus.instantFeedback.Raise(new InstantFeedbackPayload(FeedbackKind.Error, step.failHint));
                }
            }
            panel.Hide();
            Completed?.Invoke(new StepResult {
                stepId = step.stepId,
                weight = step.weight,
                earned = earned,
                durationSec = Time.time - startTime,
                deductionReasons = reasons,
                criticalFail = criticalFail
            });
        }
    }
}
