using System;
using System.Collections.Generic;
using NursingSim.Core.Events;
using NursingSim.Data;
using NursingSim.Gameplay.Hand3D;
using NursingSim.UI;
using UnityEngine;

namespace NursingSim.Gameplay
{
    /// <summary>
    /// 3D physical variant of ToolInteractionStepController. For Phase 3.0 spike, only handles
    /// kind=Pour (hand sanitizer): counts pumps via HandActionEvents.PumpPressed and rub time via
    /// HandActionEvents.Rubbed. Falls through to UI variant for kind=Click (cabinet).
    /// </summary>
    [DisallowMultipleComponent]
    public class ToolInteraction3DStepController : MonoBehaviour, IStepController
    {
        [Tooltip("Reused for instruction overlay during 3D pour. Optional — leave null to suppress.")]
        [SerializeField] private ChecklistPanelBinder pourPanel;
        [SerializeField] private HandHygieneAnimator handHygieneAnimator;

        private ToolInteractionStep step;
        private FeedbackBus bus;
        private float startTime;
        private int pumpCount;
        private float rubSeconds;
        private bool rubActive;
        private bool waterContacted;
        private bool completed;
        private bool requiresWaterContact;
        private bool ownsHandHygieneAnimator;
        private FaucetWaterInteraction[] faucetWaterInteractions = Array.Empty<FaucetWaterInteraction>();

        public event Action<StepResult> Completed;

        public void Begin(ScenarioStep step, FeedbackBus bus)
        {
            this.step = step as ToolInteractionStep;
            this.bus = bus;
            completed = false;
            pumpCount = 0;
            rubSeconds = 0f;
            rubActive = false;
            waterContacted = false;
            requiresWaterContact = false;
            startTime = Time.time;

            if (this.step == null)
            {
                Debug.LogError($"[ToolInteraction3DStepController] expected ToolInteractionStep got {step?.GetType().Name}");
                return;
            }

            if (this.step.kind != InteractionKind.Pour)
            {
                Debug.LogWarning($"[ToolInteraction3DStepController] kind {this.step.kind} not implemented in Phase 3.0; finishing partial.");
                FinishWith(Mathf.FloorToInt(this.step.weight * 0.5f), new List<DeductionReason>(), false);
                return;
            }

            HandActionEvents.PumpPressed += OnPumpPressed;
            HandActionEvents.Rubbed += OnRubbed;
            requiresWaterContact = RequiresWaterContact(this.step);
            if (requiresWaterContact) HandActionEvents.WaterContacted += OnWaterContacted;
            Hand3DController.SetActiveInputSide(HandSide.Right);
            Hand3DController.ResetAllTargetPoses();
            ResolveFaucetWaterInteractions();
            SetFaucetWaterEnabled(false);
            EnsureHandHygieneAnimator();
            if (handHygieneAnimator != null) handHygieneAnimator.Begin(FindPumpTransform());
            ShowPourHud();
        }

        public void Abort()
        {
            Cleanup();
            if (handHygieneAnimator != null) handHygieneAnimator.End();
            ReleaseOwnedHandHygieneAnimator();
            if (pourPanel != null) pourPanel.Hide();
        }

        private void OnPumpPressed(HandSimplePayload payload)
        {
            if (completed || step == null) return;
            pumpCount++;
            if (pumpCount >= step.thresholds.minPumps && !requiresWaterContact) rubActive = true;
            if (pumpCount >= step.thresholds.minPumps && requiresWaterContact) SetFaucetWaterEnabled(true);
            if (handHygieneAnimator != null)
            {
                handHygieneAnimator.RegisterPumpPress(payload.Position, pumpCount, step.thresholds.minPumps, !requiresWaterContact);
            }
            UpdatePourHud();
        }

        private void OnWaterContacted(HandSimplePayload payload)
        {
            if (completed || step == null || !requiresWaterContact) return;
            if (pumpCount < step.thresholds.minPumps) return;

            waterContacted = true;
            rubActive = true;
            if (handHygieneAnimator != null)
            {
                handHygieneAnimator.RegisterWaterContact(payload.Position);
                handHygieneAnimator.StartRubPose(payload.Position + Vector3.up * 0.04f);
            }
            UpdatePourHud();
        }

        private void OnRubbed(float deltaSec)
        {
            if (completed || step == null) return;
            if (!rubActive) return;
            if (requiresWaterContact && !waterContacted) return;
            if (handHygieneAnimator != null) handHygieneAnimator.NotifyRubInput(deltaSec);
            rubSeconds += deltaSec;
            UpdatePourHud();
            if (rubSeconds >= step.thresholds.minDurationSec)
            {
                FinishWith(step.weight, new List<DeductionReason>(), false);
            }
        }

        private void ShowPourHud()
        {
            if (pourPanel == null) return;
            pourPanel.ShowPourStatus(step.title, step.instruction, pumpCount, step.thresholds.minPumps, rubSeconds, step.thresholds.minDurationSec, requiresWaterContact, waterContacted);
        }

        private void UpdatePourHud()
        {
            if (pourPanel == null) return;
            pourPanel.UpdatePourStatus(pumpCount, step.thresholds.minPumps, rubSeconds, step.thresholds.minDurationSec, requiresWaterContact, waterContacted);
        }

        private void FinishWith(int earned, List<DeductionReason> reasons, bool criticalFail)
        {
            if (completed) return;
            completed = true;
            Cleanup();
            if (handHygieneAnimator != null) handHygieneAnimator.End();
            ReleaseOwnedHandHygieneAnimator();
            if (pourPanel != null) pourPanel.Hide();
            Completed?.Invoke(new StepResult
            {
                stepId = step.stepId,
                weight = step.weight,
                earned = earned,
                durationSec = Time.time - startTime,
                deductionReasons = reasons,
                criticalFail = criticalFail
            });
        }

        private void Cleanup()
        {
            HandActionEvents.PumpPressed -= OnPumpPressed;
            HandActionEvents.Rubbed -= OnRubbed;
            HandActionEvents.WaterContacted -= OnWaterContacted;
            SetFaucetWaterEnabled(false);
        }

        private void OnDestroy()
        {
            Cleanup();
            ReleaseOwnedHandHygieneAnimator();
        }

        private void EnsureHandHygieneAnimator()
        {
            if (handHygieneAnimator != null) return;
            handHygieneAnimator = UnityEngine.Object.FindFirstObjectByType<HandHygieneAnimator>();
            if (handHygieneAnimator != null) return;

            var host = new GameObject("HandHygieneAnimator");
            handHygieneAnimator = host.AddComponent<HandHygieneAnimator>();
            ownsHandHygieneAnimator = true;
        }

        private void ReleaseOwnedHandHygieneAnimator()
        {
            if (!ownsHandHygieneAnimator || handHygieneAnimator == null) return;
            var host = handHygieneAnimator.gameObject;
            handHygieneAnimator = null;
            ownsHandHygieneAnimator = false;
            if (Application.isPlaying) UnityEngine.Object.Destroy(host);
            else UnityEngine.Object.DestroyImmediate(host);
        }

        private static Transform FindPumpTransform()
        {
            var pump = GameObject.Find("HandSanitizerPump");
            return pump != null ? pump.transform : null;
        }

        private void ResolveFaucetWaterInteractions()
        {
            faucetWaterInteractions = UnityEngine.Object.FindObjectsByType<FaucetWaterInteraction>(FindObjectsSortMode.None);
        }

        private void SetFaucetWaterEnabled(bool enabled)
        {
            if (faucetWaterInteractions == null || faucetWaterInteractions.Length == 0) return;
            foreach (var water in faucetWaterInteractions)
            {
                if (water != null) water.SetInteractionEnabled(enabled);
            }
        }

        private static bool RequiresWaterContact(ToolInteractionStep step)
        {
            if (step == null) return false;
            if (step.thresholds != null && step.thresholds.requiresWaterContact) return true;
            return step.stepId == "STEP_HAND_HYGIENE_1";
        }
    }
}
