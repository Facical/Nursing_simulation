using System;
using System.Collections.Generic;
using NursingSim.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NursingSim.UI
{
    public class ItemSelectionPopupBinder : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text instructionLabel;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private GameObject togglePrefab;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button cancelButton;

        private readonly List<Toggle> spawnedToggles = new List<Toggle>();
        private Action<IReadOnlyList<bool>> onSubmit;

        private void Awake()
        {
            if (root) root.SetActive(false);
        }

        public void Show(string title, string instruction, IReadOnlyList<ChecklistItem> items, Action<IReadOnlyList<bool>> onSubmit)
        {
            if (root) root.SetActive(true);
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
            if (submitButton) {
                submitButton.onClick.RemoveAllListeners();
                submitButton.onClick.AddListener(Submit);
            }
            if (cancelButton) {
                cancelButton.onClick.RemoveAllListeners();
                cancelButton.onClick.AddListener(Hide);
            }
            this.onSubmit = onSubmit;
        }

        public void Hide()
        {
            if (root) root.SetActive(false);
            ClearItems();
            onSubmit = null;
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
            if (root) root.SetActive(false);
            ClearItems();
            cb.Invoke(states);
        }
    }
}
