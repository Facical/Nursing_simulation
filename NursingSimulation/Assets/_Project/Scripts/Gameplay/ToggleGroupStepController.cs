using System;
using System.Collections.Generic;
using NursingSim.Core.Events;
using NursingSim.Data;
using NursingSim.UI;
using UnityEngine;

namespace NursingSim.Gameplay
{
    [DisallowMultipleComponent]
    public class ToggleGroupStepController : MonoBehaviour, IStepController
    {
        [SerializeField] private ChecklistPanelBinder panel;

        private ToggleGroupStep step;
        private FeedbackBus bus;
        private float startTime;

        public event Action<StepResult> Completed;

        public void Begin(ScenarioStep step, FeedbackBus bus)
        {
            this.step = step as ToggleGroupStep;
            this.bus = bus;
            if (this.step == null) {
                Debug.LogError($"[ToggleGroupStepController] expected ToggleGroupStep got {step?.GetType().Name}");
                return;
            }
            startTime = Time.time;
            var asChecklist = new List<ChecklistItem>(this.step.items.Count);
            foreach (var t in this.step.items)
                asChecklist.Add(new ChecklistItem { label = t.label, required = t.required, distractor = false });
            panel.Show(this.step.title, this.step.instruction, asChecklist, OnSubmit);
        }

        public void Abort() { panel.Hide(); }

        private void OnSubmit(IReadOnlyList<bool> states)
        {
            var reasons = new List<DeductionReason>();
            int earned = step.weight;
            int perItemPenalty = Mathf.Max(1, step.weight / Mathf.Max(1, step.items.Count));
            for (int i = 0; i < step.items.Count; i++) {
                bool isOn = i < states.Count && states[i];
                if (step.items[i].required && !isOn) {
                    earned -= perItemPenalty;
                    reasons.Add(DeductionReason.RequiredItemMissing);
                }
            }
            earned = Mathf.Max(0, earned);
            bool criticalFail = false;
            if (step.isCriticalGate && earned < step.weight) {
                criticalFail = true;
                earned = 0;
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
