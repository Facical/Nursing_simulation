using NursingSim.Core.Events;
using NursingSim.Data;
using TMPro;
using UnityEngine;

namespace NursingSim.UI
{
    public class HudBinder : MonoBehaviour
    {
        [SerializeField] private FeedbackBus bus;
        [SerializeField] private TMP_Text stepLabel;
        [SerializeField] private TMP_Text scoreLabel;
        [SerializeField] private TMP_Text timerLabel;

        private int currentScore;
        private int stepIndex;
        private int stepCount;
        private float startTime;
        private bool running;

        public void Configure(int totalSteps)
        {
            stepCount = totalSteps;
            stepIndex = 0;
            currentScore = 0;
            startTime = Time.time;
            running = true;
            if (scoreLabel) scoreLabel.text = "점수: 0";
            if (stepLabel) stepLabel.text = "Step -/-";
        }

        private void OnEnable()
        {
            if (!bus) return;
            if (bus.stepStarted) bus.stepStarted.OnRaised += OnStepStarted;
            if (bus.scoreChanged) bus.scoreChanged.OnRaised += OnScoreChanged;
            if (bus.scenarioCompleted) bus.scenarioCompleted.OnRaised += OnScenarioCompleted;
        }

        private void OnDisable()
        {
            if (!bus) return;
            if (bus.stepStarted) bus.stepStarted.OnRaised -= OnStepStarted;
            if (bus.scoreChanged) bus.scoreChanged.OnRaised -= OnScoreChanged;
            if (bus.scenarioCompleted) bus.scenarioCompleted.OnRaised -= OnScenarioCompleted;
        }

        private void Update()
        {
            if (!running || !timerLabel) return;
            var elapsed = Time.time - startTime;
            int m = Mathf.FloorToInt(elapsed / 60f);
            int s = Mathf.FloorToInt(elapsed % 60f);
            timerLabel.text = $"⏱ {m:00}:{s:00}";
        }

        private void OnStepStarted(ScenarioStep step)
        {
            stepIndex++;
            if (stepLabel) stepLabel.text = $"Step {stepIndex}/{stepCount}: {step.title}";
        }

        private void OnScoreChanged(int value)
        {
            currentScore = value;
            if (scoreLabel) scoreLabel.text = $"점수: {currentScore}";
        }

        private void OnScenarioCompleted(DebriefingReport report)
        {
            running = false;
        }
    }
}
