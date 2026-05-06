using System;
using System.Collections.Generic;
using NursingSim.Core.Events;
using NursingSim.Data;
using NursingSim.UI;
using UnityEngine;

namespace NursingSim.Gameplay
{
    [DisallowMultipleComponent]
    public class ToolInteractionStepController : MonoBehaviour, IStepController
    {
        [SerializeField] private ChecklistPanelBinder pourPanel;
        [SerializeField] private ItemSelectionPopupBinder itemPopup;

        private ToolInteractionStep step;
        private FeedbackBus bus;
        private float startTime;
        private int pumpCount;
        private float rubSeconds;
        private bool rubActive;
        private bool completed;

        public event Action<StepResult> Completed;

        public void Begin(ScenarioStep step, FeedbackBus bus)
        {
            this.step = step as ToolInteractionStep;
            this.bus = bus;
            completed = false;
            pumpCount = 0;
            rubSeconds = 0f;
            rubActive = false;
            startTime = Time.time;

            if (this.step == null) {
                Debug.LogError($"[ToolInteractionStepController] expected ToolInteractionStep got {step?.GetType().Name}");
                return;
            }

            switch (this.step.kind) {
                case InteractionKind.Pour:
                    PumpInteractable.AnyPumped += OnPump;
                    CabinetInteractable.AnyOpened += OnWrongInteraction;
                    ShowPourHud();
                    break;
                case InteractionKind.Click:
                    CabinetInteractable.AnyOpened += OnCabinetOpened;
                    PumpInteractable.AnyPumped += OnIdlePump;
                    pourPanel.ShowInstructionOnly(this.step.title, this.step.instruction);
                    break;
                default:
                    Debug.LogWarning($"[ToolInteractionStepController] kind {this.step.kind} not implemented in Phase 1");
                    FinishWith(Mathf.FloorToInt(this.step.weight * 0.5f), new List<DeductionReason>(), false);
                    break;
            }
        }

        public void Abort()
        {
            Cleanup();
            pourPanel.Hide();
            itemPopup.Hide();
        }

        private void Update()
        {
            if (step == null || step.kind != InteractionKind.Pour || completed) return;
            if (rubActive) {
                rubSeconds += Time.deltaTime;
                UpdatePourHud();
                if (rubSeconds >= step.thresholds.minDurationSec) {
                    FinishWith(step.weight, new List<DeductionReason>(), false);
                }
            }
        }

        private void OnPump()
        {
            pumpCount++;
            if (pumpCount >= step.thresholds.minPumps) {
                rubActive = true;
            }
            UpdatePourHud();
        }

        private void OnWrongInteraction()
        {
            if (completed) return;
            bus?.instantFeedback?.Raise(new InstantFeedbackPayload(FeedbackKind.Warning, "지금은 손위생을 먼저 완료해야 합니다."));
            if (step.isCriticalGate && pumpCount < step.thresholds.minPumps) {
                // no auto-fail on first attempt; instant feedback only
            }
        }

        private void OnIdlePump() { /* no-op during supply prep step */ }

        private void OnCabinetOpened()
        {
            if (completed) return;
            itemPopup.Show(step.title, step.instruction, step.thresholds.items, OnItemsSubmitted);
        }

        private void OnItemsSubmitted(IReadOnlyList<bool> checkedStates)
        {
            var reasons = new List<DeductionReason>();
            int earned = step.weight;
            int perItemPenalty = Mathf.Max(1, step.weight / Mathf.Max(1, step.thresholds.items.Count));
            for (int i = 0; i < step.thresholds.items.Count; i++) {
                var item = step.thresholds.items[i];
                bool isChecked = i < checkedStates.Count && checkedStates[i];
                if (item.required && !isChecked) {
                    earned -= perItemPenalty;
                    reasons.Add(DeductionReason.RequiredItemMissing);
                } else if (item.distractor && isChecked) {
                    earned -= perItemPenalty;
                    reasons.Add(DeductionReason.DistractorItemSelected);
                }
            }
            FinishWith(Mathf.Max(0, earned), reasons, false);
        }

        private void ShowPourHud()
        {
            pourPanel.ShowPourStatus(step.title, step.instruction, pumpCount, step.thresholds.minPumps, rubSeconds, step.thresholds.minDurationSec);
        }

        private void UpdatePourHud()
        {
            pourPanel.UpdatePourStatus(pumpCount, step.thresholds.minPumps, rubSeconds, step.thresholds.minDurationSec);
        }

        private void FinishWith(int earned, List<DeductionReason> reasons, bool criticalFail)
        {
            if (completed) return;
            completed = true;
            Cleanup();
            pourPanel.Hide();
            itemPopup.Hide();
            var result = new StepResult {
                stepId = step.stepId,
                weight = step.weight,
                earned = earned,
                durationSec = Time.time - startTime,
                deductionReasons = reasons,
                criticalFail = criticalFail
            };
            Completed?.Invoke(result);
        }

        private void Cleanup()
        {
            PumpInteractable.AnyPumped -= OnPump;
            PumpInteractable.AnyPumped -= OnIdlePump;
            CabinetInteractable.AnyOpened -= OnWrongInteraction;
            CabinetInteractable.AnyOpened -= OnCabinetOpened;
        }

        private void OnDestroy()
        {
            Cleanup();
        }
    }
}
