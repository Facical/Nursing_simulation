using System;
using System.Collections.Generic;
using NursingSim.Core.Events;
using NursingSim.Data;
using NursingSim.UI;
using UnityEngine;

namespace NursingSim.Gameplay
{
    [DisallowMultipleComponent]
    public class LandmarkPickStepController : MonoBehaviour, IStepController
    {
        [SerializeField] private ChoicePanelBinder panel;

        private LandmarkPickStep step;
        private FeedbackBus bus;
        private float startTime;
        private int progress;
        private bool wrongOrderObserved;

        public event Action<StepResult> Completed;

        public void Begin(ScenarioStep step, FeedbackBus bus)
        {
            this.step = step as LandmarkPickStep;
            this.bus = bus;
            if (this.step == null) {
                Debug.LogError($"[LandmarkPickStepController] expected LandmarkPickStep got {step?.GetType().Name}");
                return;
            }
            startTime = Time.time;
            progress = 0;
            wrongOrderObserved = false;
            ShowNextPrompt();
        }

        public void Abort() { panel.Hide(); }

        private void ShowNextPrompt()
        {
            if (progress >= step.points.Count) {
                Finish();
                return;
            }
            var labels = new List<string>(step.points.Count);
            for (int i = 0; i < step.points.Count; i++) labels.Add(step.points[i].label);
            string prompt = step.requireOrder
                ? $"순서대로 클릭: {step.points[progress].label} ({progress + 1}/{step.points.Count})"
                : $"필요한 랜드마크를 모두 클릭: ({progress}/{step.points.Count})";
            panel.Show(step.title, prompt, labels, $"진행 {progress}/{step.points.Count}", OnPick);
        }

        private void OnPick(int idx)
        {
            bool correct = !step.requireOrder || idx == progress;
            if (!correct) {
                wrongOrderObserved = true;
                if (step.feedbackTiming == FeedbackTiming.Instant && bus?.instantFeedback != null) {
                    bus.instantFeedback.Raise(new InstantFeedbackPayload(FeedbackKind.Warning, "랜드마크 순서가 틀렸습니다. 다시 시도하세요."));
                }
                ShowNextPrompt();
                return;
            }
            progress++;
            ShowNextPrompt();
        }

        private void Finish()
        {
            var reasons = new List<DeductionReason>();
            int earned = step.weight;
            bool criticalFail = false;
            if (wrongOrderObserved) {
                earned = Mathf.Max(0, step.weight - Mathf.Max(1, step.weight / 2));
                reasons.Add(DeductionReason.LandmarkOrderWrong);
                if (step.isCriticalGate) criticalFail = true;
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
