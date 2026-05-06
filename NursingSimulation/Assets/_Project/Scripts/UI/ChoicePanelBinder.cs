using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NursingSim.UI
{
    public class ChoicePanelBinder : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text instructionLabel;
        [SerializeField] private TMP_Text progressLabel;
        [SerializeField] private Transform choicesContainer;
        [SerializeField] private GameObject choiceButtonPrefab;

        private readonly List<GameObject> spawned = new List<GameObject>();
        private Action<int> onPick;

        public void Show(string title, string instruction, IReadOnlyList<string> labels, string progressText, Action<int> onPick)
        {
            if (root) root.SetActive(true);
            if (titleLabel) titleLabel.text = title;
            if (instructionLabel) instructionLabel.text = instruction;
            if (progressLabel) progressLabel.text = progressText ?? string.Empty;
            ClearChoices();
            for (int i = 0; i < labels.Count; i++) {
                int idx = i;
                var go = Instantiate(choiceButtonPrefab, choicesContainer);
                var label = go.GetComponentInChildren<TMP_Text>();
                if (label) label.text = labels[i];
                var btn = go.GetComponentInChildren<Button>();
                if (btn) {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => Pick(idx));
                }
                spawned.Add(go);
            }
            this.onPick = onPick;
        }

        public void Hide()
        {
            if (root) root.SetActive(false);
            ClearChoices();
            onPick = null;
        }

        private void ClearChoices()
        {
            for (int i = spawned.Count - 1; i >= 0; i--) {
                if (spawned[i] != null) Destroy(spawned[i]);
            }
            spawned.Clear();
        }

        private void Pick(int idx)
        {
            if (onPick == null) return;
            var cb = onPick;
            onPick = null;
            cb.Invoke(idx);
        }
    }
}
