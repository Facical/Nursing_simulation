using System;
using NursingSim.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NursingSim.UI
{
    public class SequenceMiniGameBinder : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text instructionLabel;
        [SerializeField] private TMP_Text actionLabel;
        [SerializeField] private TMP_Text progressLabel;
        [SerializeField] private Slider angleSlider;
        [SerializeField] private TMP_Text angleReadout;
        [SerializeField] private Slider holdProgress;
        [SerializeField] private Button performButton;
        [SerializeField] private Button branchYesButton;
        [SerializeField] private Button branchNoButton;
        [SerializeField] private GameObject branchPanel;
        [SerializeField] private TMP_Text branchPrompt;

        public Slider AngleSlider => angleSlider;
        public Slider HoldProgress => holdProgress;
        public Button PerformButton => performButton;

        public event Action<int> OnPerformPressed;
        public event Action<bool> OnBranchPicked;

        public void Show(string title, string instruction)
        {
            if (root) root.SetActive(true);
            if (titleLabel) titleLabel.text = title;
            if (instructionLabel) instructionLabel.text = instruction;
            HideBranch();
        }

        public void SetAction(int actionIndex, int totalActions, SequenceAction action)
        {
            if (actionLabel) actionLabel.text = action != null ? action.label : string.Empty;
            if (progressLabel) progressLabel.text = $"{actionIndex + 1} / {totalActions}";
            bool needsAngle = action != null && (action.kind == SequenceActionKind.AngleHold || action.kind == SequenceActionKind.Withdraw);
            if (angleSlider) angleSlider.gameObject.SetActive(needsAngle);
            if (angleReadout) angleReadout.gameObject.SetActive(needsAngle);
            bool needsHold = action != null && (action.kind == SequenceActionKind.AngleHold || action.kind == SequenceActionKind.InjectSlow);
            if (holdProgress) {
                holdProgress.gameObject.SetActive(needsHold);
                holdProgress.value = 0f;
            }
            if (performButton) {
                var lbl = performButton.GetComponentInChildren<TMP_Text>();
                if (lbl) lbl.text = LabelFor(action);
                performButton.gameObject.SetActive(true);
                performButton.onClick.RemoveAllListeners();
                int captured = actionIndex;
                performButton.onClick.AddListener(() => OnPerformPressed?.Invoke(captured));
            }
            HideBranch();
        }

        public void UpdateAngleReadout(float deg)
        {
            if (angleReadout) angleReadout.text = $"{deg:0}°";
        }

        public void UpdateHold(float t01)
        {
            if (holdProgress) holdProgress.value = Mathf.Clamp01(t01);
        }

        public void ShowBranch(string prompt)
        {
            if (branchPanel) branchPanel.SetActive(true);
            if (branchPrompt) branchPrompt.text = prompt;
            if (branchYesButton) {
                branchYesButton.onClick.RemoveAllListeners();
                branchYesButton.onClick.AddListener(() => OnBranchPicked?.Invoke(true));
            }
            if (branchNoButton) {
                branchNoButton.onClick.RemoveAllListeners();
                branchNoButton.onClick.AddListener(() => OnBranchPicked?.Invoke(false));
            }
            if (performButton) performButton.gameObject.SetActive(false);
        }

        public void HideBranch()
        {
            if (branchPanel) branchPanel.SetActive(false);
        }

        public void Hide()
        {
            if (root) root.SetActive(false);
            HideBranch();
        }

        private static string LabelFor(SequenceAction a)
        {
            if (a == null) return "수행";
            switch (a.kind) {
                case SequenceActionKind.AngleHold: return "각도 유지하고 자입";
                case SequenceActionKind.Aspirate: return "흡인";
                case SequenceActionKind.InjectSlow: return "주입 (길게 누르기)";
                case SequenceActionKind.Withdraw: return "발침";
                case SequenceActionKind.Massage: return "마사지";
                default: return "수행";
            }
        }
    }
}
