using System;
using System.Globalization;
using NursingSim.Core.Runner;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NursingSim.UI
{
    public class RecentHistoryModalBinder : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private SaveService saveService;
        [SerializeField] private Transform rowsParent;
        [SerializeField] private GameObject rowPrefab;
        [SerializeField] private GameObject emptyPlaceholder;
        [SerializeField] private Button closeButton;
        [SerializeField] private int maxRows = 10;

        private void Awake()
        {
            if (root) root.SetActive(false);
            if (closeButton) closeButton.onClick.AddListener(Hide);
        }

        public void Show()
        {
            if (root) root.SetActive(true);
            Refresh();
        }

        public void Hide()
        {
            if (root) root.SetActive(false);
        }

        private void Refresh()
        {
            if (rowsParent == null) return;
            for (int i = rowsParent.childCount - 1; i >= 0; i--) {
                Destroy(rowsParent.GetChild(i).gameObject);
            }

            var plays = saveService != null ? saveService.GetRecentPlays(maxRows) : null;
            int count = plays != null ? plays.Count : 0;
            if (emptyPlaceholder) emptyPlaceholder.SetActive(count == 0);
            if (count == 0 || rowPrefab == null) return;

            for (int i = 0; i < count; i++) {
                var entry = plays[i];
                var row = Instantiate(rowPrefab, rowsParent);
                row.SetActive(true);
                ApplyRow(row.transform, entry);
            }
        }

        private static void ApplyRow(Transform row, PlayHistoryEntry entry)
        {
            SetLabel(row, "TimeLabel", FormatTime(entry.startedAt));
            SetLabel(row, "ScenarioLabel", string.IsNullOrEmpty(entry.scenarioId) ? "(시나리오 미지정)" : entry.scenarioId);
            SetLabel(row, "ScoreLabel", $"{entry.totalScore} / {entry.maxScore}");
            var badge = row.Find("CriticalBadge");
            if (badge) {
                bool fail = entry.criticalFailCount > 0;
                badge.gameObject.SetActive(fail);
                if (fail) SetLabel(badge, "Label", $"★ Critical {entry.criticalFailCount}");
            }
        }

        private static void SetLabel(Transform root, string childName, string text)
        {
            var t = root.Find(childName);
            if (t == null) return;
            var label = t.GetComponent<TMP_Text>();
            if (label != null) label.text = text;
        }

        private static string FormatTime(string iso)
        {
            if (string.IsNullOrEmpty(iso)) return "-";
            if (DateTime.TryParse(iso, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var dt)) {
                return dt.ToLocalTime().ToString("yyyy-MM-dd HH:mm");
            }
            return iso;
        }
    }
}
