using System;
using System.Collections.Generic;
using NursingSim.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NursingSim.UI
{
    public class ChecklistPanelBinder : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text instructionLabel;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private GameObject togglePrefab;
        [SerializeField] private Button submitButton;
        [SerializeField] private GameObject pourBlock;
        [SerializeField] private TMP_Text pourStatusLabel;
        [SerializeField] private Slider pourProgress;

        private readonly List<Toggle> spawnedToggles = new List<Toggle>();
        private Action<IReadOnlyList<bool>> onSubmit;

        public void Show(string title, string instruction, IReadOnlyList<ChecklistItem> items, Action<IReadOnlyList<bool>> onSubmit)
        {
            EnsureActive();
            titleLabel.text = title;
            instructionLabel.text = instruction;
            ClearItems();
            foreach (var item in items) {
                var go = Instantiate(togglePrefab, itemsContainer);
                var toggle = go.GetComponentInChildren<Toggle>();
                var label = go.GetComponentInChildren<TMP_Text>();
                if (label) label.text = item.label;
                toggle.isOn = false;
                spawnedToggles.Add(toggle);
            }
            if (pourBlock) pourBlock.SetActive(false);
            if (submitButton) {
                submitButton.gameObject.SetActive(true);
                submitButton.onClick.RemoveAllListeners();
                submitButton.onClick.AddListener(Submit);
            }
            this.onSubmit = onSubmit;
        }

        public void ShowInstructionOnly(string title, string instruction)
        {
            EnsureActive();
            titleLabel.text = title;
            instructionLabel.text = instruction;
            ClearItems();
            if (pourBlock) pourBlock.SetActive(false);
            if (submitButton) submitButton.gameObject.SetActive(false);
            onSubmit = null;
        }

        public void ShowPourStatus(string title, string instruction, int pumps, int minPumps, float rubSec, float minRubSec, bool requiresWaterContact = false, bool waterContacted = false)
        {
            EnsureActive();
            titleLabel.text = title;
            instructionLabel.text = instruction;
            ClearItems();
            if (pourBlock) pourBlock.SetActive(true);
            if (submitButton) submitButton.gameObject.SetActive(false);
            UpdatePourStatus(pumps, minPumps, rubSec, minRubSec, requiresWaterContact, waterContacted);
        }

        public void UpdatePourStatus(int pumps, int minPumps, float rubSec, float minRubSec, bool requiresWaterContact = false, bool waterContacted = false)
        {
            if (pourStatusLabel) {
                string water = requiresWaterContact ? $"   물 접촉 {(waterContacted ? "완료" : "대기")}" : string.Empty;
                pourStatusLabel.text = $"펌프 {pumps}/{minPumps}회{water}   비비기 {rubSec:0.0}/{minRubSec:0}초";
            }
            if (pourProgress) {
                pourProgress.value = minRubSec <= 0f ? 0f : Mathf.Clamp01(rubSec / minRubSec);
            }
        }

        public void Hide()
        {
            if (root) root.SetActive(false);
            ClearItems();
            onSubmit = null;
        }

        private void EnsureActive()
        {
            if (root) root.SetActive(true);
        }

        private void ClearItems()
        {
            for (int i = spawnedToggles.Count - 1; i >= 0; i--) {
                if (spawnedToggles[i] != null) Destroy(spawnedToggles[i].transform.parent.gameObject);
            }
            spawnedToggles.Clear();
        }

        private void Submit()
        {
            if (onSubmit == null) return;
            var states = new List<bool>(spawnedToggles.Count);
            foreach (var t in spawnedToggles) states.Add(t != null && t.isOn);
            var cb = onSubmit;
            onSubmit = null;
            cb.Invoke(states);
        }
    }
}
