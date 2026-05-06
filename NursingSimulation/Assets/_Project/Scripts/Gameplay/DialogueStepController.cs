using System;
using System.Collections.Generic;
using NursingSim.Core.Events;
using NursingSim.Data;
using NursingSim.UI;
using UnityEngine;

namespace NursingSim.Gameplay
{
    [DisallowMultipleComponent]
    public class DialogueStepController : MonoBehaviour, IStepController
    {
        [SerializeField] private ChoicePanelBinder panel;

        private DialogueStep step;
        private FeedbackBus bus;
        private float startTime;

        public event Action<StepResult> Completed;

        public void Begin(ScenarioStep step, FeedbackBus bus)
        {
            this.step = step as DialogueStep;
            this.bus = bus;
            if (this.step == null) {
                Debug.LogError($"[DialogueStepController] expected DialogueStep got {step?.GetType().Name}");
                return;
            }
            startTime = Time.time;
            var labels = new List<string>(this.step.choices.Count);
            foreach (var c in this.step.choices) labels.Add(c.text);
            panel.Show(this.step.title, this.step.instruction, labels, null, OnPick);
        }

        public void Abort() { panel.Hide(); }

        private void OnPick(int idx)
        {
            var choice = step.choices[idx];
            var reasons = new List<DeductionReason>();
            int earned = step.weight;
            bool criticalFail = false;
            if (!choice.isCorrect) {
                earned = 0;
                reasons.Add(choice.reasonIfWrong);
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
