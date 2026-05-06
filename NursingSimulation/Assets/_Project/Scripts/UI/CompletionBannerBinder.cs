using System.Text;
using NursingSim.Core.Events;
using NursingSim.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NursingSim.UI
{
    public class CompletionBannerBinder : MonoBehaviour
    {
        [SerializeField] private FeedbackBus bus;
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text summaryLabel;
        [SerializeField] private TMP_Text detailsLabel;
        [SerializeField] private Button restartButton;

        private void Awake()
        {
            if (root) root.SetActive(false);
            if (restartButton) restartButton.onClick.AddListener(() => {
                UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
            });
        }

        private void OnEnable()
        {
            if (bus && bus.scenarioCompleted) bus.scenarioCompleted.OnRaised += OnCompleted;
        }

        private void OnDisable()
        {
            if (bus && bus.scenarioCompleted) bus.scenarioCompleted.OnRaised -= OnCompleted;
        }

        private void OnCompleted(DebriefingReport report)
        {
            if (root) root.SetActive(true);
            int m = Mathf.FloorToInt(report.totalDurationSec / 60f);
            int s = Mathf.FloorToInt(report.totalDurationSec % 60f);
            if (summaryLabel) summaryLabel.text = $"총점 {report.totalScore}/{report.maxScore}   소요 {m:00}:{s:00}   치명실수 {report.criticalFailCount}";
            if (detailsLabel) {
                var sb = new StringBuilder();
                foreach (var r in report.stepResults) {
                    sb.Append($"• {r.stepId}  {r.earned}/{r.weight}");
                    if (r.deductionReasons != null && r.deductionReasons.Count > 0) {
                        sb.Append("  — ");
                        sb.Append(string.Join(", ", r.deductionReasons));
                    }
                    sb.AppendLine();
                }
                detailsLabel.text = sb.ToString();
            }
        }
    }
}
