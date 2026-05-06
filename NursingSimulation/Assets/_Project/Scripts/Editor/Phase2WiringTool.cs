using System.Collections.Generic;
using System.IO;
using NursingSim.Core.Events;
using NursingSim.Core.Interaction;
using NursingSim.Core.Runner;
using NursingSim.Data;
using NursingSim.Gameplay;
using NursingSim.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NursingSim.EditorTools
{
    public static class Phase2WiringTool
    {
        private const string EventsDir = "Assets/_Project/Data/Events";
        private const string ScenariosDir = "Assets/_Project/Data/Scenarios/IMInjection";
        private const string SimScenePath = "Assets/_Project/Scenes/Simulation_IMInjection.unity";
        private const string BusPath = EventsDir + "/FeedbackBus.asset";
        private const string ScenarioPath = ScenariosDir + "/SO_Scenario_IMInjection_Phase2.asset";
        private const string InteractableLayerName = "Interactable";

        [MenuItem("Tools/Nursing Sim/Phase 2/1. Create Full KABONE Scenario")]
        public static void CreateFullScenario()
        {
            EnsureDir(ScenariosDir);
            var scenario = AssetDatabase.LoadAssetAtPath<NursingScenario>(ScenarioPath);
            if (scenario == null) {
                scenario = ScriptableObject.CreateInstance<NursingScenario>();
                AssetDatabase.CreateAsset(scenario, ScenarioPath);
            }
            DeleteSubAssets(scenario);

            scenario.scenarioId = "SCN_IM_INJECTION_001";
            scenario.title = "근육주사 투약 (KABONE #3)";
            scenario.briefingText = "병동 처치실. 환자 김철수 님(M/45, 등록번호 20260001)에게 처방된 Diclofenac 4mg을 IM으로 투여하십시오. " +
                                   "본 시뮬레이션은 KABONE 핵심기본간호술 평가항목 프로토콜 제4.1판 항목 #3 근육주사를 따릅니다.";
            scenario.level = ScenarioLevel.Easy;
            scenario.maxScore = 100;
            scenario.sceneKey = "Simulation_IMInjection";
            scenario.patient = new PatientProfile { displayName = "김철수", ageYears = 45, sexLabel = "남", registrationNumber = "20260001", diagnosis = "통증 호소" };
            scenario.prescription = new Prescription { drugName = "Diclofenac", dose = "4mg", route = "IM", frequency = "1회" };
            scenario.steps.Clear();

            scenario.steps.Add(AddSub(scenario, BuildStep1_FiveRights()));
            scenario.steps.Add(AddSub(scenario, BuildStep2_HandHygiene1()));
            scenario.steps.Add(AddSub(scenario, BuildStep3_DrugPrep()));
            scenario.steps.Add(AddSub(scenario, BuildStep4_SupplyPrep()));
            scenario.steps.Add(AddSub(scenario, BuildStep5_ApproachAndIntro()));
            scenario.steps.Add(AddSub(scenario, BuildStep6_PatientIdentify()));
            scenario.steps.Add(AddSub(scenario, BuildStep7_ExplainPrivacy()));
            scenario.steps.Add(AddSub(scenario, BuildStep8a_SiteSelection()));
            scenario.steps.Add(AddSub(scenario, BuildStep8b_LandmarkPick()));
            scenario.steps.Add(AddSub(scenario, BuildStep9_HandHygiene3AndDisinfect()));
            scenario.steps.Add(AddSub(scenario, BuildStep10_InjectionSequence()));
            scenario.steps.Add(AddSub(scenario, BuildStep11_PostInjectionCluster()));
            scenario.steps.Add(AddSub(scenario, BuildStep12_DisposalAndRecord()));

            EditorUtility.SetDirty(scenario);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Phase2] Full KABONE scenario created at {ScenarioPath} ({scenario.steps.Count} steps)");
        }

        [MenuItem("Tools/Nursing Sim/Phase 2/2. Wire Simulation Scene (Full)")]
        public static void WireScene()
        {
            EnsureInteractableLayer();
            EnsureChannelsAndBus();
            if (AssetDatabase.LoadAssetAtPath<NursingScenario>(ScenarioPath) == null) CreateFullScenario();

            var scene = EditorSceneManager.OpenScene(SimScenePath, OpenSceneMode.Single);
            EnsurePhase0Placeholders();

            var bus = AssetDatabase.LoadAssetAtPath<FeedbackBus>(BusPath);
            var scenario = AssetDatabase.LoadAssetAtPath<NursingScenario>(ScenarioPath);

            EnsureEventSystem();
            EnsureInteractionManager();
            EnsurePump();
            EnsureCabinetInteractable();

            var canvas = EnsureCanvas("Canvas_HUD");
            var ui = BuildUi(canvas, bus);
            var controllers = BuildAllControllers(ui);
            var save = EnsureSaveService();
            BuildRunner(scenario, bus, controllers, ui.hud, save);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Phase2] Simulation scene wired with full 13-step KABONE scenario.");
            EditorUtility.DisplayDialog("Phase 2 Wiring",
                "Simulation_IMInjection 씬에 13단계 KABONE 시나리오가 배치되었습니다.\n▶ Play로 12단계 시나리오를 진행해 보세요.",
                "확인");
        }

        // ---------------- step builders ----------------

        private static ChecklistStep BuildStep1_FiveRights()
        {
            var s = ScriptableObject.CreateInstance<ChecklistStep>();
            s.name = "SO_Step_01_SixRights";
            s.stepId = "STEP_SIX_RIGHTS";
            s.title = "처방 확인 (6 Rights)";
            s.instruction = "투약처방을 6 Rights에 따라 모두 확인하세요. (KABONE 단계 2)";
            s.weight = 12;
            s.feedbackTiming = FeedbackTiming.Deferred;
            s.isCriticalGate = true;
            s.failHint = "대상자 등록번호도 반드시 확인해야 합니다 (KABONE 5 rights 정의에 포함).";
            s.theoryRef = "REF-KABONE-3-2";
            s.items = new List<ChecklistItem> {
                new ChecklistItem { label = "대상자 등록번호 (Right Patient ID)", required = true },
                new ChecklistItem { label = "대상자명 (Right Patient)", required = true },
                new ChecklistItem { label = "약명 (Right Drug)", required = true },
                new ChecklistItem { label = "용량 (Right Dose)", required = true },
                new ChecklistItem { label = "투여경로 (Right Route)", required = true },
                new ChecklistItem { label = "시간 (Right Time)", required = true },
            };
            return s;
        }

        private static ToolInteractionStep BuildStep2_HandHygiene1()
        {
            var s = ScriptableObject.CreateInstance<ToolInteractionStep>();
            s.name = "SO_Step_02_HandHygiene1";
            s.stepId = "STEP_HAND_HYGIENE_1";
            s.title = "손위생 #1 (물·비누)";
            s.instruction = "물과 비누로 손위생을 실시하세요. (KABONE 단계 1)";
            s.weight = 5;
            s.feedbackTiming = FeedbackTiming.Instant;
            s.isCriticalGate = false;
            s.failHint = "WHO 손위생 지침: 비누 사용 후 15초 이상 비비기.";
            s.theoryRef = "REF-KABONE-3-1";
            s.kind = InteractionKind.Pour;
            s.targetTag = "HandSanitizerPump";
            s.thresholds = new ToolInteractionThresholds { minPumps = 2, minDurationSec = 15f };
            return s;
        }

        private static ToolInteractionStep BuildStep3_DrugPrep()
        {
            var s = ScriptableObject.CreateInstance<ToolInteractionStep>();
            s.name = "SO_Step_03_DrugPrep";
            s.stepId = "STEP_DRUG_PREP";
            s.title = "약물 준비 (Diclofenac 4mg 앰플)";
            s.instruction = "앰플에서 Diclofenac 4mg을 정확한 용량으로 주사기에 준비하세요. (KABONE 단계 3*)";
            s.weight = 12;
            s.feedbackTiming = FeedbackTiming.Instant;
            s.isCriticalGate = true;
            s.failHint = "라벨·용량 확인 + 무균술 + 공기 제거가 필요합니다.";
            s.theoryRef = "REF-KABONE-3-3";
            s.kind = InteractionKind.Click;
            s.targetTag = "Cabinet";
            s.thresholds = new ToolInteractionThresholds {
                items = new List<ChecklistItem> {
                    new ChecklistItem { label = "Diclofenac 4mg 앰플 라벨 확인", required = true },
                    new ChecklistItem { label = "정확한 용량 인출", required = true },
                    new ChecklistItem { label = "공기 방울 제거", required = true },
                    new ChecklistItem { label = "무균술 유지 (앰플 입구 오염 회피)", required = true },
                }
            };
            return s;
        }

        private static ChecklistStep BuildStep4_SupplyPrep()
        {
            var s = ScriptableObject.CreateInstance<ChecklistStep>();
            s.name = "SO_Step_04_SupplyPrep";
            s.stepId = "STEP_SUPPLY_PREP";
            s.title = "물품 준비";
            s.instruction = "트레이에 KABONE 단계 4의 필요장비/물품을 준비하세요.";
            s.weight = 4;
            s.feedbackTiming = FeedbackTiming.Deferred;
            s.failHint = "KABONE 5절(필요장비/물품) 항목을 모두 챙기고 distractor는 빼세요.";
            s.theoryRef = "REF-KABONE-3-4";
            s.items = new List<ChecklistItem> {
                new ChecklistItem { label = "투약카드", required = true },
                new ChecklistItem { label = "일회용 멸균 주사기 2~5cc 규격별 2개씩", required = true },
                new ChecklistItem { label = "소독솜", required = true },
                new ChecklistItem { label = "손소독제", required = true },
                new ChecklistItem { label = "Diclofenac 4mg 앰플 2개", required = true },
                new ChecklistItem { label = "투약카트 또는 쟁반(tray)", required = true },
                new ChecklistItem { label = "투약기록지·간호기록지", required = true },
                new ChecklistItem { label = "손상성폐기물 전용용기", required = true },
                new ChecklistItem { label = "일반 의료폐기물 전용용기", required = true },
                new ChecklistItem { label = "인슐린 주사기", distractor = true },
                new ChecklistItem { label = "26G 피내주사용 바늘", distractor = true },
                new ChecklistItem { label = "포비돈", distractor = true },
            };
            return s;
        }

        private static ToolInteractionStep BuildStep5_ApproachAndIntro()
        {
            var s = ScriptableObject.CreateInstance<ToolInteractionStep>();
            s.name = "SO_Step_05_ApproachIntro";
            s.stepId = "STEP_APPROACH_INTRO";
            s.title = "환자 접근 + 손위생 #2 + 자기소개";
            s.instruction = "환자에게 접근하여 손소독제로 손위생 후 간호사 자신을 소개하세요. (KABONE 단계 5, 6)";
            s.weight = 4;
            s.feedbackTiming = FeedbackTiming.Deferred;
            s.failHint = "KABONE 단계 6은 손소독제 손위생을 명시합니다.";
            s.theoryRef = "REF-KABONE-3-5,6";
            s.kind = InteractionKind.Pour;
            s.targetTag = "HandSanitizerPump";
            s.thresholds = new ToolInteractionThresholds { minPumps = 2, minDurationSec = 10f };
            return s;
        }

        private static DialogueStep BuildStep6_PatientIdentify()
        {
            var s = ScriptableObject.CreateInstance<DialogueStep>();
            s.name = "SO_Step_06_PatientIdentify";
            s.stepId = "STEP_PATIENT_IDENTIFY";
            s.title = "환자 식별";
            s.instruction = "대상자의 이름을 개방형으로 질문하고, 입원팔찌 + 투약카드를 대조하세요. (KABONE 단계 7*)";
            s.weight = 12;
            s.feedbackTiming = FeedbackTiming.Instant;
            s.isCriticalGate = true;
            s.failHint = "이름·등록번호 둘 다 확인하지 않으면 실격성 감점입니다.";
            s.theoryRef = "REF-KABONE-3-7";
            s.choices = new List<DialogueChoice> {
                new DialogueChoice { text = "성함과 생년월일을 말씀해 주시겠어요? (개방형 질문 + 팔찌·카드 대조)", isCorrect = true },
                new DialogueChoice { text = "김철수 님 맞으시죠? (폐쇄형 질문)", isCorrect = false, reasonIfWrong = DeductionReason.ClosedQuestionOnly },
                new DialogueChoice { text = "(말 없이 팔찌만 확인)", isCorrect = false, reasonIfWrong = DeductionReason.OneIdentifierOnly },
                new DialogueChoice { text = "이름만 확인하고 등록번호 대조는 생략", isCorrect = false, reasonIfWrong = DeductionReason.RegistrationNumberNotChecked },
            };
            return s;
        }

        private static ToggleGroupStep BuildStep7_ExplainPrivacy()
        {
            var s = ScriptableObject.CreateInstance<ToggleGroupStep>();
            s.name = "SO_Step_07_ExplainPrivacy";
            s.stepId = "STEP_EXPLAIN_PRIVACY";
            s.title = "설명 · 동의 · 사생활";
            s.instruction = "투여 목적·작용·유의사항 설명 + 의문사항 질문 유도 + 사생활 보호. (KABONE 단계 8, 9)";
            s.weight = 3;
            s.feedbackTiming = FeedbackTiming.Deferred;
            s.failHint = "KABONE 단계 8, 9 모두 수행되어야 합니다.";
            s.theoryRef = "REF-KABONE-3-8,9";
            s.items = new List<ToggleItem> {
                new ToggleItem { label = "약물 투여 목적 설명", required = true },
                new ToggleItem { label = "약물 작용 설명", required = true },
                new ToggleItem { label = "유의사항 설명", required = true },
                new ToggleItem { label = "의문사항 질문 유도", required = true },
                new ToggleItem { label = "커튼(스크린)으로 사생활 보호", required = true },
            };
            return s;
        }

        private static SelectionStep BuildStep8a_SiteSelection()
        {
            var s = ScriptableObject.CreateInstance<SelectionStep>();
            s.name = "SO_Step_08a_SiteSelection";
            s.stepId = "STEP_SITE_SELECTION";
            s.title = "주사 부위 선정";
            s.instruction = "사례에 따라 적합한 주사 부위를 선정하세요. 본 시나리오는 측위 자세를 취한 환자입니다. (KABONE 단계 10*)";
            s.weight = 6;
            s.feedbackTiming = FeedbackTiming.Deferred;
            s.isCriticalGate = true;
            s.failHint = "측위 자세 + Diclofenac 4mg 부피의 사례에서는 둔부 복면(ventrogluteal)이 정답입니다.";
            s.theoryRef = "REF-KABONE-3-10";
            s.options = new List<SelectionOption> {
                new SelectionOption { id = "ventrogluteal", label = "② 둔부 복면 (ventrogluteal)", isCorrect = true },
                new SelectionOption { id = "dorsogluteal", label = "① 둔부 배면 (dorsogluteal)", isCorrect = false, reasonIfWrong = DeductionReason.RequiredItemMissing },
                new SelectionOption { id = "vastus", label = "③ 대퇴 (vastus lateralis)", isCorrect = false, reasonIfWrong = DeductionReason.RequiredItemMissing },
                new SelectionOption { id = "deltoid", label = "④ 삼각근 중앙", isCorrect = false, reasonIfWrong = DeductionReason.RequiredItemMissing },
            };
            return s;
        }

        private static LandmarkPickStep BuildStep8b_LandmarkPick()
        {
            var s = ScriptableObject.CreateInstance<LandmarkPickStep>();
            s.name = "SO_Step_08b_LandmarkPick";
            s.stepId = "STEP_LANDMARK_PICK";
            s.title = "랜드마크 촉진 (둔부 복면)";
            s.instruction = "측위 자세의 환자에서 둔부 복면 랜드마크를 순서대로 클릭하세요. (KABONE 단계 10*)";
            s.weight = 6;
            s.feedbackTiming = FeedbackTiming.Instant;
            s.isCriticalGate = true;
            s.failHint = "왼손바닥 → 대전자 → 전상장골극(ASIS) → 장골능 V자 순으로 짚습니다.";
            s.theoryRef = "REF-KABONE-3-10";
            s.requireOrder = true;
            s.points = new List<LandmarkPoint> {
                new LandmarkPoint { id = "greater_trochanter", label = "대전자 (greater trochanter)" },
                new LandmarkPoint { id = "asis", label = "전상장골극 (ASIS)" },
                new LandmarkPoint { id = "iliac_crest", label = "장골능 (iliac crest)" },
            };
            return s;
        }

        private static ToolInteractionStep BuildStep9_HandHygiene3AndDisinfect()
        {
            var s = ScriptableObject.CreateInstance<ToolInteractionStep>();
            s.name = "SO_Step_09_HygieneAndDisinfect";
            s.stepId = "STEP_HYGIENE3_DISINFECT";
            s.title = "손위생 #3 + 피부 소독";
            s.instruction = "손소독제로 손위생 후 알콜솜으로 부위 소독(안→밖, 직경 5~8cm, 마름 대기). (KABONE 단계 11, 12)";
            s.weight = 5;
            s.feedbackTiming = FeedbackTiming.Instant;
            s.failHint = "외→내 역방향 또는 마름 대기 누락은 감점입니다.";
            s.theoryRef = "REF-KABONE-3-11,12";
            s.kind = InteractionKind.Pour;
            s.targetTag = "HandSanitizerPump";
            s.thresholds = new ToolInteractionThresholds { minPumps = 2, minDurationSec = 10f };
            return s;
        }

        private static SequenceStep BuildStep10_InjectionSequence()
        {
            var s = ScriptableObject.CreateInstance<SequenceStep>();
            s.name = "SO_Step_10_InjectionSequence";
            s.stepId = "STEP_INJECTION_SEQUENCE";
            s.title = "주사 시퀀스 (자입·흡인·주입)";
            s.instruction = "90° 자입 → 흡인 → 혈액 분기 처리 → 천천히 주입. (KABONE 단계 13*, 14*)";
            s.weight = 12;
            s.feedbackTiming = FeedbackTiming.Instant;
            s.isCriticalGate = true;
            s.failHint = "각도 90°±10°, 흡인 수행, 혈액 보임 시 처음부터 다시 — 모두 핵심항목입니다.";
            s.theoryRef = "REF-KABONE-3-13,14";
            s.branchOnBlood = true;
            s.bloodProbability = 0f;
            s.actions = new List<SequenceAction> {
                new SequenceAction { kind = SequenceActionKind.AngleHold, label = "주사바늘 90°로 자입", targetAngleDeg = 90f, angleToleranceDeg = 10f, minDurationSec = 0.3f, reasonIfWrong = DeductionReason.AngleOutOfRange },
                new SequenceAction { kind = SequenceActionKind.Aspirate, label = "흡인 (내관 살짝 뒤로 당기기)", reasonIfWrong = DeductionReason.AspirationSkipped },
                new SequenceAction { kind = SequenceActionKind.InjectSlow, label = "약물 주입 (천천히, 1.5초 이상 유지)", minDurationSec = 1.5f, maxDurationSec = 8f, reasonIfWrong = DeductionReason.InjectionTooFast },
            };
            return s;
        }

        private static ToggleGroupStep BuildStep11_PostInjectionCluster()
        {
            var s = ScriptableObject.CreateInstance<ToggleGroupStep>();
            s.name = "SO_Step_11_PostInjection";
            s.stepId = "STEP_POST_INJECTION";
            s.title = "발침 · 마사지 · 자세 · 후 설명 · 커튼 걷기";
            s.instruction = "삽입 각도 그대로 발침, 소독솜 마사지, 자세 편안하게, 후 기대효과 설명, 커튼 걷기. (KABONE 단계 15-18)";
            s.weight = 4;
            s.feedbackTiming = FeedbackTiming.Deferred;
            s.failHint = "KABONE 단계 15-18 모두 수행되어야 합니다.";
            s.theoryRef = "REF-KABONE-3-15-18";
            s.items = new List<ToggleItem> {
                new ToggleItem { label = "삽입 각도 그대로 발침", required = true },
                new ToggleItem { label = "소독솜으로 누르며 마사지", required = true },
                new ToggleItem { label = "환의 입히고 자세 편안하게", required = true },
                new ToggleItem { label = "주사 후 기대효과 설명", required = true },
                new ToggleItem { label = "커튼(스크린) 걷기", required = true },
            };
            return s;
        }

        private static ToggleGroupStep BuildStep12_DisposalAndRecord()
        {
            var s = ScriptableObject.CreateInstance<ToggleGroupStep>();
            s.name = "SO_Step_12_DisposalRecord";
            s.stepId = "STEP_DISPOSAL_RECORD";
            s.title = "폐기 + 손위생 #4 + 기록";
            s.instruction = "바늘은 캡 없이 손상성폐기물 전용용기, 솜·주사기는 일반 의료폐기물. 물·비누 손위생. 기록지 작성. (KABONE 단계 19-21)";
            s.weight = 15;
            s.feedbackTiming = FeedbackTiming.Deferred;
            s.failHint = "재캡 후 폐기는 감점입니다. 5 rights를 모두 기록하세요.";
            s.theoryRef = "REF-KABONE-3-19,20,21";
            s.items = new List<ToggleItem> {
                new ToggleItem { label = "바늘 캡 없이 손상성폐기물 전용용기 폐기", required = true },
                new ToggleItem { label = "솜·주사기 일반 의료폐기물 전용용기 폐기", required = true },
                new ToggleItem { label = "물·비누 손위생 (15초 이상)", required = true },
                new ToggleItem { label = "기록: 대상자명", required = true },
                new ToggleItem { label = "기록: 약명", required = true },
                new ToggleItem { label = "기록: 용량", required = true },
                new ToggleItem { label = "기록: 투약경로", required = true },
                new ToggleItem { label = "기록: 투약시간", required = true },
            };
            return s;
        }

        // ---------------- scene wiring ----------------

        private struct AllControllers {
            public ChecklistStepController checklist;
            public ToolInteractionStepController tool;
            public DialogueStepController dialogue;
            public SelectionStepController selection;
            public LandmarkPickStepController landmark;
            public SequenceStepController sequence;
            public ToggleGroupStepController toggle;
        }

        private struct UiHandles {
            public HudBinder hud;
            public ChecklistPanelBinder checklistPanel;
            public ItemSelectionPopupBinder itemPopup;
            public ChoicePanelBinder choicePanel;
            public SequenceMiniGameBinder sequencePanel;
            public ToastBinder toast;
            public CompletionBannerBinder complete;
        }

        private static UiHandles BuildUi(Canvas canvas, FeedbackBus bus)
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/SDF_Regular.asset");
            var bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/SDF_Bold.asset") ?? font;
            var togglePrefab = UiBuilder.EnsureToggleRowPrefab(font);
            var choicePrefab = UiBuilder.EnsureChoiceButtonPrefab(font);

            var hudGo = UiBuilder.CreateHudBar(canvas.transform, bold);
            var hud = hudGo.AddComponent<HudBinder>();
            UiBuilder.BindHud(hud, bus, hudGo);

            var leftGo = UiBuilder.CreateChecklistPanel(canvas.transform, font, bold);
            var checklist = leftGo.AddComponent<ChecklistPanelBinder>();
            UiBuilder.BindChecklistPanel(checklist, leftGo, togglePrefab);

            var popupGo = UiBuilder.CreateItemPopup(canvas.transform, font, bold);
            var popup = popupGo.AddComponent<ItemSelectionPopupBinder>();
            UiBuilder.BindItemPopup(popup, popupGo, togglePrefab);

            var choiceGo = UiBuilder.CreateChoicePanel(canvas.transform, font, bold);
            var choice = choiceGo.AddComponent<ChoicePanelBinder>();
            UiBuilder.BindChoicePanel(choice, choiceGo, choicePrefab);

            var seqGo = UiBuilder.CreateSequenceMiniGame(canvas.transform, font, bold);
            var seq = seqGo.AddComponent<SequenceMiniGameBinder>();
            UiBuilder.BindSequenceMiniGame(seq, seqGo);

            var toastGo = UiBuilder.CreateToast(canvas.transform, font);
            var toast = toastGo.AddComponent<ToastBinder>();
            UiBuilder.BindToast(toast, bus, toastGo);

            var completeGo = UiBuilder.CreateCompletionBanner(canvas.transform, font, bold);
            var complete = completeGo.AddComponent<CompletionBannerBinder>();
            UiBuilder.BindCompletion(complete, bus, completeGo);

            return new UiHandles { hud = hud, checklistPanel = checklist, itemPopup = popup, choicePanel = choice, sequencePanel = seq, toast = toast, complete = complete };
        }

        private static AllControllers BuildAllControllers(UiHandles ui)
        {
            DestroyExisting<ChecklistStepController>();
            DestroyExisting<ToolInteractionStepController>();
            DestroyExisting<DialogueStepController>();
            DestroyExisting<SelectionStepController>();
            DestroyExisting<LandmarkPickStepController>();
            DestroyExisting<SequenceStepController>();
            DestroyExisting<ToggleGroupStepController>();

            var checklist = MakeController<ChecklistStepController>("ChecklistStepController");
            SetRef(checklist, "panel", ui.checklistPanel);

            var tool = MakeController<ToolInteractionStepController>("ToolInteractionStepController");
            SetRef(tool, "pourPanel", ui.checklistPanel);
            SetRef(tool, "itemPopup", ui.itemPopup);

            var dialogue = MakeController<DialogueStepController>("DialogueStepController");
            SetRef(dialogue, "panel", ui.choicePanel);

            var selection = MakeController<SelectionStepController>("SelectionStepController");
            SetRef(selection, "panel", ui.choicePanel);

            var landmark = MakeController<LandmarkPickStepController>("LandmarkPickStepController");
            SetRef(landmark, "panel", ui.choicePanel);

            var sequence = MakeController<SequenceStepController>("SequenceStepController");
            SetRef(sequence, "panel", ui.sequencePanel);

            var toggle = MakeController<ToggleGroupStepController>("ToggleGroupStepController");
            SetRef(toggle, "panel", ui.checklistPanel);

            return new AllControllers {
                checklist = checklist, tool = tool, dialogue = dialogue,
                selection = selection, landmark = landmark, sequence = sequence, toggle = toggle
            };
        }

        private static SaveService EnsureSaveService()
        {
            var existing = Object.FindFirstObjectByType<SaveService>();
            if (existing != null) return existing;
            var go = new GameObject("SaveService");
            return go.AddComponent<SaveService>();
        }

        private static void BuildRunner(NursingScenario scenario, FeedbackBus bus, AllControllers c, HudBinder hud, SaveService save)
        {
            var existing = Object.FindFirstObjectByType<ScenarioRunner>();
            if (existing != null) Object.DestroyImmediate(existing.gameObject);
            var go = new GameObject("ScenarioRunner");
            var runner = go.AddComponent<ScenarioRunner>();
            var so = new SerializedObject(runner);
            so.FindProperty("scenario").objectReferenceValue = scenario;
            so.FindProperty("bus").objectReferenceValue = bus;
            so.FindProperty("checklistController").objectReferenceValue = c.checklist;
            so.FindProperty("toolController").objectReferenceValue = c.tool;
            so.FindProperty("dialogueController").objectReferenceValue = c.dialogue;
            so.FindProperty("selectionController").objectReferenceValue = c.selection;
            so.FindProperty("landmarkController").objectReferenceValue = c.landmark;
            so.FindProperty("sequenceController").objectReferenceValue = c.sequence;
            so.FindProperty("toggleController").objectReferenceValue = c.toggle;
            so.FindProperty("hud").objectReferenceValue = hud;
            so.FindProperty("saveService").objectReferenceValue = save;
            so.ApplyModifiedProperties();
        }

        // ---------------- helpers ----------------

        private static T AddSub<T>(NursingScenario scenario, T sub) where T : ScenarioStep
        {
            AssetDatabase.AddObjectToAsset(sub, scenario);
            return sub;
        }

        private static void DeleteSubAssets(NursingScenario scenario)
        {
            var path = AssetDatabase.GetAssetPath(scenario);
            var assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (var a in assets) {
                if (a == scenario) continue;
                if (a is ScenarioStep) Object.DestroyImmediate(a, true);
            }
            scenario.steps.Clear();
        }

        private static T MakeController<T>(string name) where T : Component
        {
            var go = new GameObject(name);
            return go.AddComponent<T>();
        }

        private static void DestroyExisting<T>() where T : Component
        {
            var existing = Object.FindFirstObjectByType<T>();
            if (existing != null) Object.DestroyImmediate(existing.gameObject);
        }

        private static void SetRef(Component c, string fieldName, Object reference)
        {
            var so = new SerializedObject(c);
            var prop = so.FindProperty(fieldName);
            if (prop != null) prop.objectReferenceValue = reference;
            so.ApplyModifiedProperties();
        }

        private static void EnsureChannelsAndBus()
        {
            if (AssetDatabase.LoadAssetAtPath<FeedbackBus>(BusPath) == null) {
                Phase1WiringTool.CreateChannelsAndBus();
            }
        }

        private static void EnsureDir(string dir)
        {
            if (!AssetDatabase.IsValidFolder(dir)) {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }
        }

        private static void EnsureInteractableLayer()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProp = tagManager.FindProperty("layers");
            for (int i = 0; i < layersProp.arraySize; i++) {
                if (layersProp.GetArrayElementAtIndex(i).stringValue == InteractableLayerName) return;
            }
            for (int i = 8; i < layersProp.arraySize; i++) {
                var el = layersProp.GetArrayElementAtIndex(i);
                if (string.IsNullOrEmpty(el.stringValue)) {
                    el.stringValue = InteractableLayerName;
                    tagManager.ApplyModifiedProperties();
                    AssetDatabase.SaveAssets();
                    return;
                }
            }
        }

        private static void EnsureEventSystem()
        {
            if (Object.FindFirstObjectByType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }

        private static void EnsureInteractionManager()
        {
            var mgr = Object.FindFirstObjectByType<InteractionManager>();
            if (mgr == null) {
                var go = new GameObject("InteractionManager");
                mgr = go.AddComponent<InteractionManager>();
            }
            int layer = LayerMask.NameToLayer(InteractableLayerName);
            if (layer >= 0) {
                var so = new SerializedObject(mgr);
                var maskProp = so.FindProperty("interactableMask");
                if (maskProp != null) maskProp.intValue = 1 << layer;
                var camProp = so.FindProperty("sourceCamera");
                if (camProp != null && camProp.objectReferenceValue == null) camProp.objectReferenceValue = Camera.main;
                so.ApplyModifiedProperties();
            }
        }

        private static void EnsurePump()
        {
            var existing = GameObject.Find("HandSanitizerPump");
            if (existing == null) {
                existing = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                existing.name = "HandSanitizerPump";
                existing.transform.position = new Vector3(-1.6f, 0.8f, 0.4f);
                existing.transform.localScale = new Vector3(0.12f, 0.2f, 0.12f);
            }
            int layer = LayerMask.NameToLayer(InteractableLayerName);
            if (layer >= 0) existing.layer = layer;
            if (existing.GetComponent<PumpInteractable>() == null) existing.AddComponent<PumpInteractable>();
            var so = new SerializedObject(existing.GetComponent<PumpInteractable>());
            var idProp = so.FindProperty("id"); if (idProp != null) idProp.stringValue = "HandSanitizerPump";
            var nameProp = so.FindProperty("displayName"); if (nameProp != null) nameProp.stringValue = "손소독제 펌프";
            var rendProp = so.FindProperty("highlightRenderer");
            if (rendProp != null && rendProp.objectReferenceValue == null) rendProp.objectReferenceValue = existing.GetComponent<Renderer>();
            so.ApplyModifiedProperties();
        }

        private static void EnsureCabinetInteractable()
        {
            var cabinet = GameObject.Find("Cabinet_Placeholder");
            if (cabinet == null) return;
            int layer = LayerMask.NameToLayer(InteractableLayerName);
            if (layer >= 0) {
                cabinet.layer = layer;
                foreach (Transform t in cabinet.transform) t.gameObject.layer = layer;
            }
            var body = cabinet.transform.Find("Body");
            if (body != null && body.GetComponent<Collider>() == null) body.gameObject.AddComponent<BoxCollider>();
            if (cabinet.GetComponent<CabinetInteractable>() == null) cabinet.AddComponent<CabinetInteractable>();
            var so = new SerializedObject(cabinet.GetComponent<CabinetInteractable>());
            var idProp = so.FindProperty("id"); if (idProp != null) idProp.stringValue = "Cabinet";
            var nameProp = so.FindProperty("displayName"); if (nameProp != null) nameProp.stringValue = "물품 캐비닛";
            var rendProp = so.FindProperty("highlightRenderer");
            if (rendProp != null && rendProp.objectReferenceValue == null) {
                var rend = cabinet.GetComponentInChildren<Renderer>();
                if (rend) rendProp.objectReferenceValue = rend;
            }
            so.ApplyModifiedProperties();
        }

        private static void EnsurePhase0Placeholders()
        {
            if (GameObject.Find("Floor") == null) {
                var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
                floor.name = "Floor";
                floor.transform.localScale = new Vector3(2f, 1f, 2f);
            }
            if (GameObject.Find("Patient_Placeholder") == null) {
                var patient = new GameObject("Patient_Placeholder");
                patient.transform.position = new Vector3(0f, 0.5f, 0f);
                var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                body.name = "Body";
                body.transform.SetParent(patient.transform, false);
            }
            if (GameObject.Find("Tray_Placeholder") == null) {
                var tray = new GameObject("Tray_Placeholder");
                tray.transform.position = new Vector3(1.5f, 0.8f, 0f);
                var top = GameObject.CreatePrimitive(PrimitiveType.Cube);
                top.name = "TrayTop";
                top.transform.SetParent(tray.transform, false);
                top.transform.localScale = new Vector3(0.6f, 0.05f, 0.4f);
            }
            if (GameObject.Find("Cabinet_Placeholder") == null) {
                var cab = new GameObject("Cabinet_Placeholder");
                cab.transform.position = new Vector3(-2f, 1f, 0f);
                var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
                body.name = "Body";
                body.transform.SetParent(cab.transform, false);
                body.transform.localScale = new Vector3(0.5f, 2f, 0.4f);
            }
            var mainCam = GameObject.Find("Main Camera");
            if (mainCam != null && mainCam.transform.position == Vector3.zero) {
                mainCam.transform.position = new Vector3(0f, 1.6f, -3f);
                mainCam.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
            }
        }

        private static Canvas EnsureCanvas(string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null) Object.DestroyImmediate(existing);
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 10;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            return canvas;
        }
    }
}
