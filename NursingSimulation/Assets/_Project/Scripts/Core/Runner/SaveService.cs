using System;
using System.Collections.Generic;
using System.IO;
using NursingSim.Data;
using UnityEngine;

namespace NursingSim.Core.Runner
{
    [Serializable]
    public class PlayHistoryEntry
    {
        public string scenarioId;
        public string startedAt;
        public string endedAt;
        public int totalScore;
        public int maxScore;
        public int criticalFailCount;
        public List<StepResult> stepResults = new List<StepResult>();
    }

    [Serializable]
    public class PlayHistoryFile
    {
        public int version = 1;
        public List<PlayHistoryEntry> plays = new List<PlayHistoryEntry>();
    }

    public class SaveService : MonoBehaviour
    {
        [SerializeField] private int maxRetainedPlays = 10;
        [SerializeField] private string fileName = "playhistory.json";

        private string FullPath => Path.Combine(Application.persistentDataPath, fileName);

        public void AppendPlay(DebriefingReport report)
        {
            if (report == null) return;
            var file = LoadOrInit();
            var entry = new PlayHistoryEntry {
                scenarioId = report.scenarioId,
                startedAt = DateTime.UtcNow.AddSeconds(-report.totalDurationSec).ToString("o"),
                endedAt = DateTime.UtcNow.ToString("o"),
                totalScore = report.totalScore,
                maxScore = report.maxScore,
                criticalFailCount = report.criticalFailCount,
                stepResults = report.stepResults != null ? new List<StepResult>(report.stepResults) : new List<StepResult>()
            };
            file.plays.Add(entry);
            while (file.plays.Count > maxRetainedPlays) file.plays.RemoveAt(0);
            try {
                File.WriteAllText(FullPath, JsonUtility.ToJson(file, prettyPrint: true));
                Debug.Log($"[SaveService] play history written: {FullPath} ({file.plays.Count}/{maxRetainedPlays})");
            } catch (Exception ex) {
                Debug.LogError($"[SaveService] write failed: {ex.Message}");
            }
        }

        public IReadOnlyList<PlayHistoryEntry> GetRecentPlays(int maxCount)
        {
            if (maxCount <= 0) return new List<PlayHistoryEntry>();
            var file = LoadOrInit();
            int start = Math.Max(0, file.plays.Count - maxCount);
            var slice = file.plays.GetRange(start, file.plays.Count - start);
            slice.Reverse();
            return slice;
        }

        public PlayHistoryFile LoadOrInit()
        {
            try {
                if (File.Exists(FullPath)) {
                    var json = File.ReadAllText(FullPath);
                    var parsed = JsonUtility.FromJson<PlayHistoryFile>(json);
                    if (parsed != null) return parsed;
                }
            } catch (Exception ex) {
                Debug.LogWarning($"[SaveService] load failed, starting fresh: {ex.Message}");
            }
            return new PlayHistoryFile();
        }
    }
}
