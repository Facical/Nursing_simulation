using System;
using System.Collections.Generic;
using NursingSim.Core.Events;
using NursingSim.Data;
using NursingSim.UI;
using UnityEngine;

namespace NursingSim.Gameplay
{
    [DisallowMultipleComponent]
    public class ChecklistStepController : MonoBehaviour, IStepController
    {
        [SerializeField] private ChecklistPanelBinder panel;

        private ChecklistStep step;
        private FeedbackBus bus;
        private float startTime;

        public event Action<StepResult> Completed;

        public void Begin(ScenarioStep step, FeedbackBus bus)
        {
            this.step = step as ChecklistStep;
            this.bus = bus;
            if (this.step == null) {
                Debug.LogError($"[ChecklistStepController] expected ChecklistStep got {step?.GetType().Name}");
                return;
            }
            startTime = Time.time;
            panel.Show(this.step.title, this.step.instruction, this.step.items, OnSubmit);
        }

        public void Abort()
        {
            panel.Hide();
        }

        private void OnSubmit(IReadOnlyList<bool> checkedStates)
        {
            var result = new StepResult {
                stepId = step.stepId,
                weight = step.weight,
                earned = step.weight,
                durationSec = Time.time - startTime,
                deductionReasons = new List<DeductionReason>()
            };

            int perItemPenalty = Mathf.Max(1, step.weight / Mathf.Max(1, step.items.Count));
            for (int i = 0; i < step.items.Count; i++) {
                var item = step.items[i];
                bool isChecked = i < checkedStates.Count && checkedStates[i];
                if (item.required && !isChecked) {
                    result.earned -= perItemPenalty;
                    result.deductionReasons.Add(DeductionReason.RequiredItemMissing);
                } else if (item.distractor && isChecked) {
                    result.earned -= perItemPenalty;
                    result.deductionReasons.Add(DeductionReason.DistractorItemSelected);
                }
            }
            result.earned = Mathf.Max(0, result.earned);
            if (step.isCriticalGate && result.earned < step.weight) {
                result.criticalFail = true;
                result.earned = 0;
            }

            panel.Hide();
            Completed?.Invoke(result);
        }
    }
}
