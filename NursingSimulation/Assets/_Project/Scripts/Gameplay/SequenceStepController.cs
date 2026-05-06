using System;
using System.Collections.Generic;
using NursingSim.Core.Events;
using NursingSim.Data;
using NursingSim.UI;
using UnityEngine;

namespace NursingSim.Gameplay
{
    [DisallowMultipleComponent]
    public class SequenceStepController : MonoBehaviour, IStepController
    {
        [SerializeField] private SequenceMiniGameBinder panel;

        private SequenceStep step;
        private FeedbackBus bus;
        private float startTime;
        private int actionIndex;
        private float holdSeconds;
        private bool holding;
        private bool awaitingBranchAnswer;
        private bool bloodSeen;
        private readonly List<DeductionReason> reasons = new List<DeductionReason>();
        private int earned;

        public event Action<StepResult> Completed;

        public void Begin(ScenarioStep step, FeedbackBus bus)
        {
            this.step = step as SequenceStep;
            this.bus = bus;
            if (this.step == null) {
                Debug.LogError($"[SequenceStepController] expected SequenceStep got {step?.GetType().Name}");
                return;
            }
            startTime = Time.time;
            reasons.Clear();
            earned = this.step.weight;
            actionIndex = 0;
            holdSeconds = 0f;
            holding = false;
            awaitingBranchAnswer = false;
            bloodSeen = UnityEngine.Random.value < this.step.bloodProbability;

            panel.OnPerformPressed -= OnPerform;
            panel.OnPerformPressed += OnPerform;
            panel.OnBranchPicked -= OnBranchPicked;
            panel.OnBranchPicked += OnBranchPicked;
            panel.Show(this.step.title, this.step.instruction);
            panel.PerformButton.gameObject.SetActive(true);
            ShowCurrentAction();
        }

        public void Abort()
        {
            panel.OnPerformPressed -= OnPerform;
            panel.OnBranchPicked -= OnBranchPicked;
            panel.Hide();
        }

        private void Update()
        {
            if (step == null) return;
            if (actionIndex >= step.actions.Count) return;
            var action = step.actions[actionIndex];
            bool isHoldKind = action.kind == SequenceActionKind.AngleHold || action.kind == SequenceActionKind.InjectSlow;
            if (isHoldKind && holding) {
                holdSeconds += Time.deltaTime;
                if (action.minDurationSec > 0f) panel.UpdateHold(holdSeconds / action.minDurationSec);
            }
            if (panel.AngleSlider != null && panel.AngleSlider.gameObject.activeSelf) {
                float angleDeg = Mathf.Lerp(45f, 135f, panel.AngleSlider.value);
                panel.UpdateAngleReadout(angleDeg);
            }
        }

        private void ShowCurrentAction()
        {
            if (actionIndex >= step.actions.Count) {
                FinishStep();
                return;
            }
            holdSeconds = 0f;
            var action = step.actions[actionIndex];
            holding = action.kind == SequenceActionKind.AngleHold || action.kind == SequenceActionKind.InjectSlow;
            panel.SetAction(actionIndex, step.actions.Count, action);
        }

        private float CurrentAngleDeg()
        {
            if (panel.AngleSlider == null) return 90f;
            return Mathf.Lerp(45f, 135f, panel.AngleSlider.value);
        }

        private void OnPerform(int forIndex)
        {
            if (forIndex != actionIndex) return;
            if (awaitingBranchAnswer) return;
            var action = step.actions[actionIndex];
            switch (action.kind) {
                case SequenceActionKind.AngleHold:
                    EvaluateAngleAndAdvance(action);
                    break;
                case SequenceActionKind.Aspirate:
                    if (step.branchOnBlood) {
                        awaitingBranchAnswer = true;
                        string prompt = bloodSeen
                            ? "주사기에 혈액이 보입니다. 어떻게 하시겠습니까?"
                            : "흡인 결과 혈액이 보이지 않습니다. 다음 단계로 진행하시겠습니까?";
                        panel.ShowBranch(prompt);
                    } else {
                        actionIndex++;
                        ShowCurrentAction();
                    }
                    break;
                case SequenceActionKind.InjectSlow:
                    if (action.minDurationSec > 0f && holdSeconds < action.minDurationSec) {
                        reasons.Add(DeductionReason.InjectionTooFast);
                        earned -= Mathf.Max(1, step.weight / Mathf.Max(1, step.actions.Count));
                    } else if (action.maxDurationSec > 0f && holdSeconds > action.maxDurationSec) {
                        reasons.Add(DeductionReason.InjectionTooSlow);
                        earned -= Mathf.Max(1, step.weight / Mathf.Max(1, step.actions.Count));
                    }
                    actionIndex++;
                    ShowCurrentAction();
                    break;
                case SequenceActionKind.Withdraw:
                    EvaluateAngleAndAdvance(action, isWithdraw: true);
                    break;
                case SequenceActionKind.Massage:
                    actionIndex++;
                    ShowCurrentAction();
                    break;
            }
        }

        private void EvaluateAngleAndAdvance(SequenceAction action, bool isWithdraw = false)
        {
            float angle = CurrentAngleDeg();
            if (Mathf.Abs(angle - action.targetAngleDeg) > action.angleToleranceDeg) {
                reasons.Add(isWithdraw ? DeductionReason.WithdrawalAngleDiff : DeductionReason.AngleOutOfRange);
                earned -= Mathf.Max(1, step.weight / Mathf.Max(1, step.actions.Count));
            }
            actionIndex++;
            ShowCurrentAction();
        }

        private void OnBranchPicked(bool yesAdvance)
        {
            awaitingBranchAnswer = false;
            panel.HideBranch();
            if (bloodSeen) {
                if (yesAdvance) {
                    // KABONE 단계 14: 혈액이 보이는데 그대로 주입 — critical fail.
                    reasons.Add(DeductionReason.BloodSeenButContinued);
                    earned = 0;
                    if (step.isCriticalGate) {
                        FinishStep(criticalFail: true);
                        return;
                    }
                    actionIndex++;
                    ShowCurrentAction();
                } else {
                    // KABONE 단계 14: 혈액이 보이면 처음부터 다시 — 정답 분기.
                    bloodSeen = false;
                    actionIndex = 0;
                    ShowCurrentAction();
                }
            } else {
                // 혈액이 보이지 않을 때는 진행이 정답. "아니오"는 비논리적 선택이지만
                // 흡인 자체는 수행되었으므로 AspirationSkipped는 의미가 맞지 않는다.
                // 단순히 다음 단계로 advance — 감점 없음.
                actionIndex++;
                ShowCurrentAction();
            }
        }

        private void FinishStep(bool criticalFail = false)
        {
            earned = Mathf.Max(0, earned);
            criticalFail = SequenceScoring.ShouldCriticalFail(reasons, step.isCriticalGate, criticalFail);
            if (criticalFail) earned = 0;
            panel.OnPerformPressed -= OnPerform;
            panel.OnBranchPicked -= OnBranchPicked;
            panel.Hide();
            Completed?.Invoke(new StepResult {
                stepId = step.stepId,
                weight = step.weight,
                earned = earned,
                durationSec = Time.time - startTime,
                deductionReasons = new List<DeductionReason>(reasons),
                criticalFail = criticalFail
            });
        }

    }
}
