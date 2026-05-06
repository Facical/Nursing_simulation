using System;
using System.Collections.Generic;
using NursingSim.Core.Events;
using NursingSim.Data;
using NursingSim.Gameplay;
using NursingSim.UI;
using UnityEngine;

namespace NursingSim.Core.Runner
{
    public enum RunnerState { Idle, StepActive, StepEvaluating, Completed }

    public class ScenarioRunner : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private NursingScenario scenario;

        [Header("Bus")]
        [SerializeField] private FeedbackBus bus;

        [Header("Controllers (scene instances)")]
        [SerializeField] private ChecklistStepController checklistController;
        [SerializeField] private ToolInteractionStepController toolController;
        [SerializeField] private DialogueStepController dialogueController;
        [SerializeField] private SelectionStepController selectionController;
        [SerializeField] private LandmarkPickStepController landmarkController;
        [SerializeField] private SequenceStepController sequenceController;
        [SerializeField] private ToggleGroupStepController toggleController;

        [Header("Save")]
        [SerializeField] private SaveService saveService;

        [Header("UI")]
        [SerializeField] private HudBinder hud;

        private readonly List<StepResult> results = new List<StepResult>();
        private int currentIndex = -1;
        private int currentScore;
        private float startTime;

        public RunnerState State { get; private set; } = RunnerState.Idle;

        private void Start()
        {
            if (scenario == null) { Debug.LogError("[ScenarioRunner] scenario not assigned"); return; }
            if (bus == null || !bus.IsComplete()) { Debug.LogError("[ScenarioRunner] FeedbackBus incomplete"); return; }
            if (scenario.steps == null || scenario.steps.Count == 0) {
                Debug.LogError("[ScenarioRunner] scenario has no steps");
                return;
            }
            startTime = Time.time;
            if (hud) hud.Configure(scenario.steps.Count);
            bus.scoreChanged.Raise(0);
            Advance();
        }

        private void Advance()
        {
            currentIndex++;
            if (currentIndex >= scenario.steps.Count) {
                Finish();
                return;
            }
            var step = scenario.steps[currentIndex];
            if (step == null) {
                Debug.LogWarning($"[ScenarioRunner] step {currentIndex} null, skipping");
                Advance();
                return;
            }
            var controller = ControllerFor(step);
            if (controller == null) {
                Debug.LogError($"[ScenarioRunner] no controller for {step.GetType().Name}, skipping");
                Advance();
                return;
            }
            State = RunnerState.StepActive;
            Debug.Log($"[Runner] StepActive [{currentIndex+1}/{scenario.steps.Count}] {step.stepId} ({step.GetType().Name})");
            controller.Completed += OnStepCompleted;
            bus.stepStarted.Raise(step);
            controller.Begin(step, bus);
        }

        private IStepController ControllerFor(ScenarioStep step)
        {
            switch (step) {
                case ChecklistStep _: return checklistController;
                case ToolInteractionStep _: return toolController;
                case DialogueStep _: return dialogueController;
                case SelectionStep _: return selectionController;
                case LandmarkPickStep _: return landmarkController;
                case SequenceStep _: return sequenceController;
                case ToggleGroupStep _: return toggleController;
                default: return null;
            }
        }

        private void OnStepCompleted(StepResult result)
        {
            State = RunnerState.StepEvaluating;
            Debug.Log($"[Runner] StepEvaluating {result.stepId} earned {result.earned}/{result.weight}");
            var step = scenario.steps[currentIndex];
            var controller = ControllerFor(step);
            if (controller != null) controller.Completed -= OnStepCompleted;
            results.Add(result);
            currentScore += result.earned;
            bus.scoreChanged.Raise(currentScore);
            bus.stepCompleted.Raise(result);
            Advance();
        }

        private void Finish()
        {
            State = RunnerState.Completed;
            int criticalFails = 0;
            foreach (var r in results) if (r.criticalFail) criticalFails++;
            var report = new DebriefingReport {
                scenarioId = scenario.scenarioId,
                totalScore = currentScore,
                maxScore = scenario.maxScore,
                totalDurationSec = Time.time - startTime,
                criticalFailCount = criticalFails,
                stepResults = new List<StepResult>(results)
            };
            Debug.Log($"[Runner] Completed total={currentScore}/{scenario.maxScore}");
            if (saveService != null) saveService.AppendPlay(report);
            bus.scenarioCompleted.Raise(report);
        }
    }
}
