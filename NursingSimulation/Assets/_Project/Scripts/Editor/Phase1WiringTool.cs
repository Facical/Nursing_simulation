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
    public static class Phase1WiringTool
    {
        private const string EventsDir = "Assets/_Project/Data/Events";
        private const string ScenariosDir = "Assets/_Project/Data/Scenarios/IMInjection";
        private const string SimScenePath = "Assets/_Project/Scenes/Simulation_IMInjection.unity";
        private const string BusPath = EventsDir + "/FeedbackBus.asset";
        private const string ScenarioPath = ScenariosDir + "/SO_Scenario_IMInjection_Phase1.asset";
        private const string InteractableLayerName = "Interactable";

        [MenuItem("Tools/Nursing Sim/Phase 1/1. Create Channels + Bus")]
        public static void CreateChannelsAndBus()
        {
            EnsureDir(EventsDir);
            var stepStarted        = GetOrCreate<StepStartedChannel>(EventsDir + "/Channel_StepStarted.asset");
            var stepProgress       = GetOrCreate<StepProgressChannel>(EventsDir + "/Channel_StepProgress.asset");
            var stepCompleted      = GetOrCreate<StepCompletedChannel>(EventsDir + "/Channel_StepCompleted.asset");
            var instantFeedback    = GetOrCreate<InstantFeedbackChannel>(EventsDir + "/Channel_InstantFeedback.asset");
            var scoreChanged       = GetOrCreate<ScoreChangedChannel>(EventsDir + "/Channel_ScoreChanged.asset");
            var scenarioCompleted  = GetOrCreate<ScenarioCompletedChannel>(EventsDir + "/Channel_ScenarioCompleted.asset");

            var bus = AssetDatabase.LoadAssetAtPath<FeedbackBus>(BusPath);
            if (bus == null) {
                bus = ScriptableObject.CreateInstance<FeedbackBus>();
                AssetDatabase.CreateAsset(bus, BusPath);
            }
            bus.stepStarted = stepStarted;
            bus.stepProgress = stepProgress;
            bus.stepCompleted = stepCompleted;
            bus.instantFeedback = instantFeedback;
            bus.scoreChanged = scoreChanged;
            bus.scenarioCompleted = scenarioCompleted;
            EditorUtility.SetDirty(bus);
            AssetDatabase.SaveAssets();
            Debug.Log("[Phase1] Channels + FeedbackBus ensured at " + BusPath);
        }

        [MenuItem("Tools/Nursing Sim/Phase 1/2. Create Mini Scenario")]
        public static void CreateMiniScenario()
        {
            EnsureDir(ScenariosDir);

            var scenario = AssetDatabase.LoadAssetAtPath<NursingScenario>(ScenarioPath);
            if (scenario == null) {
                scenario = ScriptableObject.CreateInstance<NursingScenario>();
                scenario.scenarioId = "SCN_IM_INJECTION_PHASE1";
                scenario.title = "근육주사 투약 (Phase 1 축소판)";
                scenario.briefingText = "Phase 1 프로토타입: 손위생과 물품준비 2단계만 플레이합니다.";
                scenario.level = ScenarioLevel.Easy;
                scenario.maxScore = 15;
                scenario.sceneKey = "Simulation_IMInjection";
                AssetDatabase.CreateAsset(scenario, ScenarioPath);
            }
            scenario.steps.Clear();

            var hand = ScriptableObject.CreateInstance<ToolInteractionStep>();
            hand.name = "SO_Step_HandHygiene";
            hand.stepId = "STEP_HAND_HYGIENE";
            hand.title = "손위생";
            hand.instruction = "손소독제를 2회 펌프한 뒤 15초 이상 비벼 주세요.";
            hand.weight = 5;
            hand.feedbackTiming = FeedbackTiming.Instant;
            hand.isCriticalGate = true;
            hand.failHint = "WHO 손위생 지침: 펌프 2회 + 15초 이상 비비기.";
            hand.theoryRef = "기본간호학 손위생 p.120 전후";
            hand.kind = InteractionKind.Pour;
            hand.targetTag = "HandSanitizerPump";
            hand.thresholds = new ToolInteractionThresholds { minPumps = 2, minDurationSec = 15f };
            AssetDatabase.AddObjectToAsset(hand, scenario);

            var prep = ScriptableObject.CreateInstance<ToolInteractionStep>();
            prep.name = "SO_Step_SupplyPrep";
            prep.stepId = "STEP_SUPPLY_PREP";
            prep.title = "물품 준비";
            prep.instruction = "캐비닛을 열어 주사에 필요한 물품을 트레이에 담으세요.";
            prep.weight = 10;
            prep.feedbackTiming = FeedbackTiming.Deferred;
            prep.isCriticalGate = false;
            prep.failHint = "성인 IM 기준 23G×1인치, 인출용 21G, 알콜솜 2개가 기본입니다.";
            prep.theoryRef = "기본간호학 근육주사 물품 p.330";
            prep.kind = InteractionKind.Click;
            prep.targetTag = "Cabinet";
            prep.thresholds = new ToolInteractionThresholds {
                items = new List<ChecklistItem> {
                    new ChecklistItem { label = "주사기 3cc", required = true },
                    new ChecklistItem { label = "주사바늘 23G×1인치", required = true },
                    new ChecklistItem { label = "인출용 바늘 21G", required = true },
                    new ChecklistItem { label = "알콜솜 2개", required = true },
                    new ChecklistItem { label = "바이알 (Ceftriaxone 1g)", required = true },
                    new ChecklistItem { label = "증류수 앰플", required = true },
                    new ChecklistItem { label = "장갑", required = true },
                    new ChecklistItem { label = "샤프스 컨테이너", required = true },
                    new ChecklistItem { label = "트레이", required = true },
                    new ChecklistItem { label = "처방전", required = true },
                    new ChecklistItem { label = "인슐린 주사기", distractor = true },
                    new ChecklistItem { label = "26G 피내주사용 바늘", distractor = true },
                    new ChecklistItem { label = "포비돈", distractor = true },
                }
            };
            AssetDatabase.AddObjectToAsset(prep, scenario);

            scenario.steps.Add(hand);
            scenario.steps.Add(prep);

            EditorUtility.SetDirty(scenario);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[Phase1] Mini scenario created at " + ScenarioPath);
        }

        [MenuItem("Tools/Nursing Sim/Phase 1/3. Wire Simulation Scene")]
        public static void WireScene()
        {
            EnsureInteractableLayer();
            CreateChannelsAndBus();
            if (AssetDatabase.LoadAssetAtPath<NursingScenario>(ScenarioPath) == null) {
                CreateMiniScenario();
            }

            var scene = EditorSceneManager.OpenScene(SimScenePath, OpenSceneMode.Single);
            EnsurePhase0Placeholders();

            var bus = AssetDatabase.LoadAssetAtPath<FeedbackBus>(BusPath);
            var scenario = AssetDatabase.LoadAssetAtPath<NursingScenario>(ScenarioPath);

            EnsureEventSystem();
            EnsureInteractionManager();

            var pump = EnsurePump();
            EnsureCabinetInteractable();

            var canvas = EnsureCanvas("Canvas_HUD");
            var ui = BuildUi(canvas, bus);
            var controllers = BuildControllers(ui);
            BuildRunner(scenario, bus, controllers.checklist, controllers.tool, ui.hud);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Phase1] Simulation scene wired and saved.");
            EditorUtility.DisplayDialog(
                "Phase 1 Wiring",
                "Simulation_IMInjection 씬에 Phase 1 플레이 시스템이 배치되었습니다.\n▶ Play 눌러 펌프/캐비닛을 클릭해 보세요.",
                "확인");
        }

        private static void EnsurePhase0Placeholders()
        {
            if (GameObject.Find("Floor") == null) {
                AssetCatalogHelper.SpawnRole("Floor", "Floor", PrimitiveType.Plane, new Vector3(2f, 1f, 2f));
            }
            if (GameObject.Find("Patient_Placeholder") == null) {
                var patient = new GameObject("Patient_Placeholder");
                patient.transform.position = new Vector3(0f, 0.5f, 0f);
                var body = AssetCatalogHelper.SpawnRole("PatientBody", "Body", PrimitiveType.Capsule, Vector3.one);
                body.transform.SetParent(patient.transform, false);
            }
            if (GameObject.Find("Tray_Placeholder") == null) {
                var tray = new GameObject("Tray_Placeholder");
                tray.transform.position = new Vector3(1.5f, 0.8f, 0f);
                var top = AssetCatalogHelper.SpawnRole("Tray", "TrayTop", PrimitiveType.Cube, new Vector3(0.6f, 0.05f, 0.4f));
                top.transform.SetParent(tray.transform, false);
            }
            if (GameObject.Find("Cabinet_Placeholder") == null) {
                var cab = new GameObject("Cabinet_Placeholder");
                cab.transform.position = new Vector3(-2f, 1f, 0f);
                var body = AssetCatalogHelper.SpawnRole("Cabinet", "Body", PrimitiveType.Cube, new Vector3(0.5f, 2f, 0.4f));
                body.transform.SetParent(cab.transform, false);
            }
            var mainCam = GameObject.Find("Main Camera");
            if (mainCam != null && mainCam.transform.position == Vector3.zero) {
                mainCam.transform.position = new Vector3(0f, 1.6f, -3f);
                mainCam.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
            }
        }

        // ---------- helpers ----------

        private static void EnsureDir(string dir)
        {
            if (!AssetDatabase.IsValidFolder(dir)) {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }
        }

        private static T GetOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) {
                asset = ScriptableObject.CreateInstance<T>();
                AssetDatabase.CreateAsset(asset, path);
            }
            return asset;
        }

        private static void EnsureInteractableLayer()
        {
            var tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            var layersProp = tagManager.FindProperty("layers");
            bool exists = false;
            for (int i = 0; i < layersProp.arraySize; i++) {
                if (layersProp.GetArrayElementAtIndex(i).stringValue == InteractableLayerName) { exists = true; break; }
            }
            if (!exists) {
                for (int i = 8; i < layersProp.arraySize; i++) {
                    var el = layersProp.GetArrayElementAtIndex(i);
                    if (string.IsNullOrEmpty(el.stringValue)) {
                        el.stringValue = InteractableLayerName;
                        tagManager.ApplyModifiedProperties();
                        AssetDatabase.SaveAssets();
                        Debug.Log($"[Phase1] Added layer '{InteractableLayerName}' at index {i}");
                        return;
                    }
                }
                Debug.LogWarning("[Phase1] No empty layer slot for 'Interactable'; add it manually.");
            }
        }

        private static void EnsureEventSystem()
        {
            var existing = Object.FindFirstObjectByType<EventSystem>();
            if (existing != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            go.transform.position = Vector3.zero;
        }

        private static InteractionManager EnsureInteractionManager()
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
            return mgr;
        }

        private static GameObject EnsurePump()
        {
            var existing = GameObject.Find("HandSanitizerPump");
            if (existing == null) {
                existing = AssetCatalogHelper.SpawnRole("HandSanitizerPump", "HandSanitizerPump", PrimitiveType.Cylinder, new Vector3(0.12f, 0.2f, 0.12f));
                existing.transform.position = new Vector3(-1.6f, 0.8f, 0.4f);
            }
            existing.tag = "Untagged";
            int layer = LayerMask.NameToLayer(InteractableLayerName);
            if (layer >= 0) existing.layer = layer;
            if (existing.GetComponent<PumpInteractable>() == null) existing.AddComponent<PumpInteractable>();
            var so = new SerializedObject(existing.GetComponent<PumpInteractable>());
            var idProp = so.FindProperty("id"); if (idProp != null) idProp.stringValue = "HandSanitizerPump";
            var nameProp = so.FindProperty("displayName"); if (nameProp != null) nameProp.stringValue = "손소독제 펌프";
            var rendProp = so.FindProperty("highlightRenderer");
            if (rendProp != null && rendProp.objectReferenceValue == null) rendProp.objectReferenceValue = existing.GetComponent<Renderer>();
            so.ApplyModifiedProperties();
            return existing;
        }

        private static void EnsureCabinetInteractable()
        {
            var cabinet = GameObject.Find("Cabinet_Placeholder");
            if (cabinet == null) return;
            int layer = LayerMask.NameToLayer(InteractableLayerName);
            if (layer >= 0) SetLayerRecursively(cabinet, layer);
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

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform t in go.transform) SetLayerRecursively(t.gameObject, layer);
        }

        private static Canvas EnsureCanvas(string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null) { Object.DestroyImmediate(existing); }
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

        private struct UiHandles { public HudBinder hud; public ChecklistPanelBinder checklistPanel; public ItemSelectionPopupBinder itemPopup; public ToastBinder toast; public CompletionBannerBinder complete; }
        private struct ControllerHandles { public ChecklistStepController checklist; public ToolInteractionStepController tool; }

        private static UiHandles BuildUi(Canvas canvas, FeedbackBus bus)
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/SDF_Regular.asset");
            var bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/SDF_Bold.asset") ?? font;

            var togglePrefab = UiBuilder.EnsureToggleRowPrefab(font);

            var hudGo = UiBuilder.CreateHudBar(canvas.transform, bold);
            var hud = hudGo.AddComponent<HudBinder>();
            UiBuilder.BindHud(hud, bus, hudGo);

            var leftGo = UiBuilder.CreateChecklistPanel(canvas.transform, font, bold);
            var checklist = leftGo.AddComponent<ChecklistPanelBinder>();
            UiBuilder.BindChecklistPanel(checklist, leftGo, togglePrefab);

            var popupGo = UiBuilder.CreateItemPopup(canvas.transform, font, bold);
            var popup = popupGo.AddComponent<ItemSelectionPopupBinder>();
            UiBuilder.BindItemPopup(popup, popupGo, togglePrefab);

            var toastGo = UiBuilder.CreateToast(canvas.transform, font);
            var toast = toastGo.AddComponent<ToastBinder>();
            UiBuilder.BindToast(toast, bus, toastGo);

            var completeGo = UiBuilder.CreateCompletionBanner(canvas.transform, font, bold);
            var complete = completeGo.AddComponent<CompletionBannerBinder>();
            UiBuilder.BindCompletion(complete, bus, completeGo);

            return new UiHandles { hud = hud, checklistPanel = checklist, itemPopup = popup, toast = toast, complete = complete };
        }

        private static ControllerHandles BuildControllers(UiHandles ui)
        {
            var existingChecklist = GameObject.Find("ChecklistStepController");
            if (existingChecklist != null) Object.DestroyImmediate(existingChecklist);
            var existingTool = GameObject.Find("ToolInteractionStepController");
            if (existingTool != null) Object.DestroyImmediate(existingTool);

            var checklistGo = new GameObject("ChecklistStepController");
            var checklist = checklistGo.AddComponent<ChecklistStepController>();
            var checklistSo = new SerializedObject(checklist);
            checklistSo.FindProperty("panel").objectReferenceValue = ui.checklistPanel;
            checklistSo.ApplyModifiedProperties();

            var toolGo = new GameObject("ToolInteractionStepController");
            var tool = toolGo.AddComponent<ToolInteractionStepController>();
            var toolSo = new SerializedObject(tool);
            toolSo.FindProperty("pourPanel").objectReferenceValue = ui.checklistPanel;
            toolSo.FindProperty("itemPopup").objectReferenceValue = ui.itemPopup;
            toolSo.ApplyModifiedProperties();

            return new ControllerHandles { checklist = checklist, tool = tool };
        }

        private static void BuildRunner(NursingScenario scenario, FeedbackBus bus, ChecklistStepController checklist, ToolInteractionStepController tool, HudBinder hud)
        {
            var existing = Object.FindFirstObjectByType<ScenarioRunner>();
            if (existing != null) Object.DestroyImmediate(existing.gameObject);
            var go = new GameObject("ScenarioRunner");
            var runner = go.AddComponent<ScenarioRunner>();
            var so = new SerializedObject(runner);
            so.FindProperty("scenario").objectReferenceValue = scenario;
            so.FindProperty("bus").objectReferenceValue = bus;
            so.FindProperty("checklistController").objectReferenceValue = checklist;
            so.FindProperty("toolController").objectReferenceValue = tool;
            so.FindProperty("hud").objectReferenceValue = hud;
            so.ApplyModifiedProperties();
        }
    }
}
