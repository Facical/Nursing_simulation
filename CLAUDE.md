# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project State

**This repository is currently in planning/spec phase — no Unity project or code yet.** The root contains only `README.md`, `.gitignore`, and `docs/`. A Unity project will be initialized under this directory during Phase 0 of the roadmap.

Target stack (see `docs/03-technical-spec.md` for details):
- Unity 6 LTS (primary) or 2022.3 LTS (fallback), URP, Linear color space
- Windows + macOS standalone
- Language: Korean only (한국어 전용)
- Packages: New Input System, TextMeshPro (Korean SDF atlas), Cinemachine

## Architecture (planned)

The runtime is **data-driven**: scenarios are authored as ScriptableObjects, not code. The key contract across the codebase:

```
NursingScenario (SO)
  └─ List<ScenarioStep> (abstract SO; concrete subtypes below)
       ChecklistStep / ToolInteractionStep / DialogueStep
       SelectionStep / LandmarkPickStep / SequenceStep / ToggleGroupStep
```

`ScenarioRunner` (FSM) activates one step at a time, spawning the matching `IStepController` MonoBehaviour in the Simulation scene. `InteractionManager` (Raycast-based) translates mouse input into `IInteractable` hits. Everything communicates through a **ScriptableObject-based event channel** (`FeedbackBus`) — step controllers publish `StepCompleted` / `InstantFeedback` / `ScoreChanged`; UI, audio, and scoring subscribe independently.

Why this matters for edits:
- **Don't hard-code scenario logic in MonoBehaviours.** Logic lives in step controllers keyed by SO type; authoring happens in `.asset` files. A nursing instructor (non-coder) is expected to tune step weights and text in the inspector.
- **Every step has a `weight`, `feedbackTiming` (Instant/Deferred), and `isCriticalGate` flag.** Scoring and UX branch on these — preserve them when adding step types.
- **Localization keys over literals.** Step text fields should reference Localization Table keys, not embed Korean strings directly (only `ko` table is populated in MVP).

Full contract: `docs/05-data-model.md`. Scene/UX flow: `docs/04-scene-and-ux-flow.md`.

## The One Document That Gates Correctness

`docs/02-functional-spec.md` is the **single source of truth** for the IM injection procedure (11 steps, weights summing to 100, deduction reasons). It is explicitly marked as pending nursing-instructor review. When modifying anything that affects procedure order, scoring, or deduction reasons:
1. Update `02-functional-spec.md` first.
2. Keep the `DeductionReason` enum in `05-data-model.md` in sync.
3. Flag the change for re-review — medical accuracy is a project-level risk (R2).

Never invent new clinical rules from general knowledge. If a procedural detail is unclear, ask rather than guess.

## Folder Layout (once Unity project exists)

Project-specific code/assets go under `Assets/_Project/` (separated from `ThirdParty/`). Scenes: `MainMenu`, `Briefing`, `Simulation_IMInjection`, `Debriefing`. Scenario SO assets live at `Assets/_Project/Data/Scenarios/IMInjection/`.

## Commands

No build/test commands yet — the Unity project has not been created.

Once initialized (per `docs/07-roadmap.md` Phase 0):
- Open via Unity Hub with the version pinned in `ProjectSettings/ProjectVersion.txt`
- Edit Mode tests: Unity Test Runner (NUnit) — `ScenarioRunner` FSM, scoring, JSON save
- Play Mode tests: interaction smoke tests
- Build: `File → Build Settings` → Windows x64 (IL2CPP) or macOS Universal

## Working Conventions

- **Scope discipline**: MVP is a single scenario (IM injection). Additional scenarios, VR, multiplayer, AI-generated content are explicitly out of scope until MVP ships (see `docs/01-product-vision.md` Non-goals and R12).
- **Art direction is fixed**: Semi-realistic characters/environment (Microsoft Rocketbox, MIT) + photoreal medical tools (the close-up objects — syringes, vials, needles). Don't try to push patient/nurse look-dev toward MetaHuman-level realism; that's explicitly out of scope to avoid uncanny valley and control cost (R1, R5). Avoid close-up face cameras in cinematic framing.
- **Korean text everywhere user-facing.** Code/comments/commits can be English. TMP atlas must cover KS X 1001 2,350 characters plus common symbols.
- **Asset licensing**: every third-party asset must be logged in `Assets/_Project/Art/LICENSES.md` on first import (see `docs/06-asset-plan.md` §5).

## Key Reference Files

- `README.md` — entry point, doc index
- `docs/02-functional-spec.md` — **IM injection procedure** (authoritative)
- `docs/03-technical-spec.md` — Unity version, packages, architecture, coding conventions
- `docs/05-data-model.md` — SO schema and `DeductionReason` enum
- `docs/07-roadmap.md` — phases and Definition of Done per phase
- `docs/08-risks-and-mitigation.md` — known risks (R1–R12) and early-warning triggers
