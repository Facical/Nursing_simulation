using NursingSim.Core.Runner;
using NursingSim.Data;
using NursingSim.Gameplay;
using NursingSim.Gameplay.Hand3D;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NursingSim.EditorTools
{
    public static class Phase3WiringTool
    {
        private const string SimScenePath = "Assets/_Project/Scenes/Simulation_IMInjection.unity";
        private const string PumpName = "HandSanitizerPump";
        private const string SinkName = "Main_23m_0";
        private const string FaucetWaterZoneName = "FaucetWaterZone";
        private const string TreatmentTableName = "Main_63m_0";
        private const string RightHandName = "PlayerHand_Right";
        private const string LeftHandName = "PlayerHand_Left";
        private const string CloseupVCamName = "VCam_HandHygiene_Closeup";
        private const string FirstPersonRigName = "FirstPersonHandCameraRig";
        private const string PumpPrefabPath = "Assets/_Project/Prefabs/Tools/HandSanitizerPump.prefab";
        private const string RightHandPrefabPath = "Assets/_Project/Prefabs/Characters/PlayerHand_Right.prefab";
        private const string LeftHandPrefabPath = "Assets/_Project/Prefabs/Characters/PlayerHand_Left.prefab";

        [MenuItem("Tools/Nursing Sim/Phase 3/1. Spike: Wire Step 2 손위생 (3D)")]
        public static void WireStep2HandHygiene3D()
        {
            WireHandHygiene3D(showDialog: true);
        }

        [MenuItem("Tools/Nursing Sim/Phase 3/2. Set Hand Hygiene Camera From Scene View")]
        public static void SetHandHygieneCameraFromSceneView()
        {
            SetHandHygieneCameraFromSceneView(showDialog: true);
        }

        public static void WireHandHygiene3DAutomated()
        {
            WireHandHygiene3D(showDialog: false);
        }

        public static void SetHandHygieneCameraFromSceneViewAutomated()
        {
            SetHandHygieneCameraFromSceneView(showDialog: false);
        }

        private static void WireHandHygiene3D(bool showDialog)
        {
            var scene = EditorSceneManager.OpenScene(SimScenePath, OpenSceneMode.Single);

            var runner = Object.FindFirstObjectByType<ScenarioRunner>();
            if (runner == null)
            {
                const string message = "ScenarioRunner를 찾을 수 없습니다. 먼저 Phase 2 > 2. Wire Simulation Scene (Full)을 실행하세요.";
                if (showDialog) EditorUtility.DisplayDialog("Phase 3 Wiring", message, "확인");
                else Debug.LogWarning($"[Phase3] {message}");
                return;
            }

            HandModelAuthoringTool.BuildPlayerHandPrefabs();

            var pump = EnsurePumpAugmented();
            EnsureFaucetWaterInteraction();
            var cameraRig = EnsureFirstPersonRig(pump.transform);
            var rightHand = EnsureHand(RightHandName, RightHandPrefabPath, cameraRig.RightShoulderAnchor.position, HandSide.Right);
            var leftHand = EnsureHand(LeftHandName, LeftHandPrefabPath, cameraRig.LeftShoulderAnchor.position, HandSide.Left);
            ParentHandToAnchor(rightHand, cameraRig.RightShoulderAnchor);
            ParentHandToAnchor(leftHand, cameraRig.LeftShoulderAnchor);
            var vcam = EnsureCloseupVCam(cameraRig);
            var toolCtrl3D = EnsureToolController3D();
            var handHygieneAnimator = EnsureHandHygieneAnimator(pump, leftHand, rightHand);
            AssignHandHygieneAnimator(toolCtrl3D, handHygieneAnimator);
            AssignControllerToRunner(runner, toolCtrl3D);
            EnablePourStepsIn3DSwitch(runner);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log("[Phase3] Step 2 손위생 3D 스파이크 와이어링 완료. ▶ Play.");
            if (showDialog)
            {
                EditorUtility.DisplayDialog("Phase 3 Wiring",
                    "손위생 3D 스파이크가 씬에 배치되었습니다.\n\n조작:\n· F1: 조작법 보기/닫기\n· A/S/D/F/Space: 현재 선택한 손 이동\n· 마우스 휠: 손 높이 조절\n· RMB+마우스: 손목 회전\n· LMB: 집기/놓기, 펌프 가까이에서는 누르기\n· Z/X/C/V/B: 엄지/검지/중지/약지/소지 curl\n· Left Shift: 왼손/오른손 전환\n· Ctrl/R: 앉기/일어서기",
                    "확인");
            }
        }

        // -------- helpers --------

        private static void SetHandHygieneCameraFromSceneView(bool showDialog)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                const string playingMessage = "Play 모드를 종료한 뒤 Scene View 카메라를 저장하세요.";
                if (showDialog) EditorUtility.DisplayDialog("Hand Hygiene Camera", playingMessage, "확인");
                else Debug.LogWarning($"[Phase3] {playingMessage}");
                return;
            }

            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null || sceneView.camera == null)
            {
                const string missingMessage = "활성 Scene View 카메라를 찾을 수 없습니다.";
                if (showDialog) EditorUtility.DisplayDialog("Hand Hygiene Camera", missingMessage, "확인");
                else Debug.LogWarning($"[Phase3] {missingMessage}");
                return;
            }

            var rigGo = GameObject.Find(FirstPersonRigName);
            if (rigGo == null) rigGo = new GameObject(FirstPersonRigName);
            var rig = rigGo.GetComponent<FirstPersonHandCameraRig>();
            if (rig == null) rig = rigGo.AddComponent<FirstPersonHandCameraRig>();

            var sceneCamera = sceneView.camera.transform;
            var cameraPosition = sceneCamera.position;
            var lookAt = cameraPosition + sceneCamera.forward * 2f;
            rig.Configure(cameraPosition, lookAt);

            var so = new SerializedObject(rig);
            so.FindProperty("applyToMainCameraOnStart").boolValue = false;
            so.ApplyModifiedProperties();

            ParentHandToAnchor(GameObject.Find(RightHandName), rig.RightShoulderAnchor);
            ParentHandToAnchor(GameObject.Find(LeftHandName), rig.LeftShoulderAnchor);

            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Debug.Log($"[Phase3] Scene View pose captured for hand hygiene camera. camera={cameraPosition:F2}, euler={sceneCamera.eulerAngles:F1}");
            if (showDialog)
            {
                EditorUtility.DisplayDialog("Hand Hygiene Camera",
                    $"현재 Scene View 위치를 손위생 카메라로 저장했습니다.\n\nPosition: {cameraPosition:F2}\nRotation: {sceneCamera.eulerAngles:F1}",
                    "확인");
            }
        }

        private static GameObject EnsureHand(string handName, string prefabPath, Vector3 position, HandSide handSide)
        {
            var existing = GameObject.Find(handName);
            if (existing != null)
            {
                if (ShouldReplacePlaceholder(existing, prefabPath) || AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null)
                {
                    var existingTransform = existing.transform;
                    position = existingTransform.position;
                    Object.DestroyImmediate(existing);
                }
                else
                {
                    ConfigureHand(existing, handSide);
                    return existing;
                }
            }

            if (!AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath))
            {
                HandModelAuthoringTool.BuildPlayerHandPrefabs();
            }

            existing = GameObject.Find(handName);
            if (existing != null)
            {
                ConfigureHand(existing, handSide);
                return existing;
            }

            GameObject go = null;
            var handPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (handPrefab != null)
            {
                go = (GameObject)PrefabUtility.InstantiatePrefab(handPrefab);
                go.name = handName;
            }
            if (go == null)
            {
                go = new GameObject(handName);
                // Visual placeholder — replaced by armTutorial hand prefab after import.
                var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                visual.name = "HandVisual_Placeholder";
                visual.transform.SetParent(go.transform, false);
                visual.transform.localScale = Vector3.one * 0.08f;
                var col = visual.GetComponent<Collider>();
                if (col) Object.DestroyImmediate(col);
                var rend = visual.GetComponent<Renderer>();
                if (rend) rend.sharedMaterial.color = handSide == HandSide.Right ? new Color(0.3f, 0.6f, 0.9f, 1f) : new Color(0.45f, 0.75f, 0.55f, 1f);
            }

            go.transform.position = position;
            ConfigureHand(go, handSide);
            return go;
        }

        private static bool ShouldReplacePlaceholder(GameObject existing, string prefabPath)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) == null) return false;
            if (existing.transform.Find("HandVisual_Placeholder") != null) return true;
            return existing.GetComponentsInChildren<Renderer>(true).Length == 0;
        }

        private static void ConfigureHand(GameObject go, HandSide handSide)
        {
            if (go.GetComponent<Rigidbody>() == null) go.AddComponent<Rigidbody>();
            if (go.GetComponent<SphereCollider>() == null) go.AddComponent<SphereCollider>();
            if (go.GetComponent<Hand3DController>() == null) go.AddComponent<Hand3DController>();
            if (go.GetComponent<HandPoseController>() == null) go.AddComponent<HandPoseController>();
            if (go.GetComponent<ArmRigReferences>() == null) go.AddComponent<ArmRigReferences>();
            if (go.GetComponent<TwoBoneArmIK>() == null) go.AddComponent<TwoBoneArmIK>();
            if (go.GetComponent<FingerPoseController>() == null) go.AddComponent<FingerPoseController>();

            var sc = go.GetComponent<SphereCollider>();
            sc.radius = 0.04f;
            sc.isTrigger = true;

            var rig = go.GetComponent<ArmRigReferences>();
            rig.AutoBind(handSide);
            var palm = rig.Palm;

            var target = rig.HandTarget;
            if (target != null)
            {
                var rb = target.GetComponent<Rigidbody>();
                if (rb == null) rb = target.gameObject.AddComponent<Rigidbody>();
                rb.useGravity = false;
                rb.isKinematic = true;

                var targetCollider = target.GetComponent<SphereCollider>();
                if (targetCollider == null) targetCollider = target.gameObject.AddComponent<SphereCollider>();
                targetCollider.radius = 0.035f;
                targetCollider.isTrigger = false;
            }

            var ctrl = go.GetComponent<Hand3DController>();
            var ctrlSo = new SerializedObject(ctrl);
            ctrlSo.FindProperty("handSide").enumValueIndex = (int)handSide;
            ctrlSo.FindProperty("acceptInput").boolValue = true;
            ctrlSo.FindProperty("startLocked").boolValue = false;
            ctrlSo.FindProperty("rig").objectReferenceValue = rig;
            ctrlSo.FindProperty("handTarget").objectReferenceValue = target;
            ctrlSo.FindProperty("handTargetBody").objectReferenceValue = target != null ? target.GetComponent<Rigidbody>() : null;
            ctrlSo.FindProperty("useSceneAuthoredTargetPose").boolValue = true;
            ctrlSo.FindProperty("palm").objectReferenceValue = palm;
            ctrlSo.FindProperty("keyboardMoveSpeed").floatValue = 0.65f;
            ctrlSo.FindProperty("keyboardLiftSpeed").floatValue = 0.45f;
            ctrlSo.FindProperty("depthMoveSpeed").floatValue = 0.002f;
            ctrlSo.FindProperty("workspaceRadius").floatValue = 1.1f;
            ctrlSo.FindProperty("handSideLateralRange").vector2Value = new Vector2(-0.7f, 0.85f);
            ctrlSo.FindProperty("handVerticalRange").vector2Value = new Vector2(-0.75f, 0.32f);
            ctrlSo.FindProperty("handForwardRange").vector2Value = new Vector2(0.02f, 1.18f);
            ctrlSo.FindProperty("constrainTargetFromShoulder").boolValue = true;
            ctrlSo.FindProperty("shoulderOutwardRange").vector2Value = new Vector2(0.22f, 0.72f);
            ctrlSo.FindProperty("enableCollisionBlocking").boolValue = true;
            ctrlSo.FindProperty("collisionRadius").floatValue = 0.035f;
            ctrlSo.FindProperty("handShellCollisionRadius").floatValue = 0.05f;
            ctrlSo.FindProperty("fingerCollisionRadius").floatValue = 0.018f;
            ctrlSo.FindProperty("collisionSkin").floatValue = 0.008f;
            ctrlSo.FindProperty("ignoreOversizedConcaveMeshBlockers").boolValue = true;
            ctrlSo.FindProperty("oversizedConcaveMeshBlockerMaxExtent").floatValue = 3f;
            ctrlSo.ApplyModifiedProperties();
            ApplyArmIkDefaults(go.GetComponent<TwoBoneArmIK>());
        }

        private static void ApplyArmIkDefaults(TwoBoneArmIK ik)
        {
            if (ik == null) return;

            var so = new SerializedObject(ik);
            so.FindProperty("maxElbowFlexionDegrees").floatValue = 105f;
            so.FindProperty("useAnatomicalElbowPole").boolValue = true;
            so.FindProperty("elbowPoleDownWeight").floatValue = 0.85f;
            so.FindProperty("elbowPoleOutwardWeight").floatValue = 0.45f;
            so.FindProperty("elbowPoleHintWeight").floatValue = 0.25f;
            so.FindProperty("elbowPoleRestWeight").floatValue = 0.18f;
            so.FindProperty("elbowPoleSmoothSpeed").floatValue = 12f;
            so.ApplyModifiedProperties();
        }

        private static void ParentHandToAnchor(GameObject hand, Transform anchor)
        {
            if (hand == null || anchor == null) return;
            hand.transform.SetParent(anchor, false);
            hand.transform.localPosition = Vector3.zero;
            hand.transform.localRotation = Quaternion.identity;
        }

        private static Transform EnsureChildAnchor(Transform parent, string name, Vector3 localPosition)
        {
            var found = parent.Find(name);
            if (found != null)
            {
                found.localPosition = localPosition;
                return found;
            }
            var anchor = new GameObject(name).transform;
            anchor.SetParent(parent, false);
            anchor.localPosition = localPosition;
            return anchor;
        }

        private static GameObject EnsurePumpAugmented()
        {
            var pumpPosition = CalculatePumpPosition();
            var pump = GameObject.Find(PumpName);
            if (ShouldReplacePumpWithPrefab(pump))
            {
                if (pump != null) Object.DestroyImmediate(pump);
                pump = InstantiatePumpPrefabOrFallback();
            }

            pump.name = PumpName;
            pump.transform.position = pumpPosition;
            pump.transform.rotation = Quaternion.Euler(270f, 0f, 0f);
            ConfigurePumpVisual(pump);
            ConfigurePumpInteraction(pump);
            PlacePumpOnTreatmentTable(pump);
            return pump;
        }

        private static bool ShouldReplacePumpWithPrefab(GameObject pump)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PumpPrefabPath);
            if (pump == null) return true;
            if (prefab == null) return false;
            if (pump.transform.Find("PumpPlunger") != null || pump.transform.Find("PumpNozzle") != null) return true;
            return FindChildRecursive(pump.transform, "Collada visual scene group") == null;
        }

        private static GameObject InstantiatePumpPrefabOrFallback()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PumpPrefabPath);
            if (prefab != null)
            {
                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                instance.name = PumpName;
                return instance;
            }

            return AssetCatalogHelper.SpawnRole("HandSanitizerPump", PumpName, PrimitiveType.Cylinder, new Vector3(0.08f, 0.12f, 0.08f));
        }

        private static Vector3 CalculatePumpPosition()
        {
            var sink = GameObject.Find(SinkName);
            var table = GameObject.Find(TreatmentTableName);
            if (sink != null && table != null)
            {
                var sinkBounds = CalculateRendererBounds(sink);
                var tableBounds = CalculateRendererBounds(table);
                bool sinkOnPositiveXSide = sinkBounds.center.x >= tableBounds.center.x;
                float x = sinkOnPositiveXSide ? tableBounds.max.x - 0.16f : tableBounds.min.x + 0.16f;
                float z = Mathf.Clamp(sinkBounds.center.z + 0.05f, tableBounds.min.z + 0.12f, tableBounds.max.z - 0.12f);
                float y = tableBounds.max.y + 0.16f;
                return new Vector3(x, y, z);
            }

            if (sink != null)
            {
                var sinkBounds = CalculateRendererBounds(sink);
                return new Vector3(sinkBounds.min.x - 0.18f, sinkBounds.min.y + 0.6f, sinkBounds.center.z);
            }

            return new Vector3(-0.5f, 0.85f, 0.2f);
        }

        private static Bounds CalculateRendererBounds(GameObject go)
        {
            var renderers = go.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);

            var bounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }

            return bounds;
        }

        private static void ConfigurePumpVisual(GameObject pump)
        {
            RemoveGeneratedPumpPart(pump.transform, "PumpPlunger");
            RemoveGeneratedPumpPart(pump.transform, "PumpNozzle");
            pump.transform.localScale = Vector3.one * 0.13f;
        }

        private static void RemoveGeneratedPumpPart(Transform parent, string name)
        {
            var child = parent.Find(name);
            if (child != null) Object.DestroyImmediate(child.gameObject);
        }

        private static void ConfigurePumpInteraction(GameObject pump)
        {
            var collider = pump.GetComponent<Collider>();
            if (collider == null)
            {
                var box = pump.AddComponent<BoxCollider>();
                box.size = new Vector3(1.6f, 2.4f, 1.6f);
                box.center = Vector3.zero;
                collider = box;
            }
            collider.isTrigger = false;

            var pump3D = pump.GetComponent<HandSanitizerPump3D>();
            if (pump3D == null) pump3D = pump.AddComponent<HandSanitizerPump3D>();

            var interactable = pump.GetComponent<PumpInteractable>();
            if (interactable == null) interactable = pump.AddComponent<PumpInteractable>();

            var plunger = FindChildRecursive(pump.transform, "top");
            var renderer = pump.GetComponentInChildren<Renderer>();

            var pump3DSo = new SerializedObject(pump3D);
            pump3DSo.FindProperty("plungerVisual").objectReferenceValue = plunger;
            pump3DSo.FindProperty("pressTriggerRadius").floatValue = 0.35f;
            pump3DSo.FindProperty("allowDirectClick").boolValue = true;
            var cameraProp = pump3DSo.FindProperty("sourceCamera");
            if (cameraProp != null && cameraProp.objectReferenceValue == null) cameraProp.objectReferenceValue = Camera.main;
            var clickDistanceProp = pump3DSo.FindProperty("directClickDistance");
            if (clickDistanceProp != null) clickDistanceProp.floatValue = 8f;
            pump3DSo.ApplyModifiedProperties();

            var interactableSo = new SerializedObject(interactable);
            interactableSo.FindProperty("id").stringValue = "HandSanitizerPump";
            interactableSo.FindProperty("displayName").stringValue = "손소독제";
            interactableSo.FindProperty("highlightRenderer").objectReferenceValue = renderer;
            interactableSo.ApplyModifiedProperties();
        }

        private static void PlacePumpOnTreatmentTable(GameObject pump)
        {
            var table = GameObject.Find(TreatmentTableName);
            if (table == null) return;

            var tableBounds = CalculateRendererBounds(table);
            var pumpBounds = CalculateRendererBounds(pump);
            float offsetY = tableBounds.max.y + 0.01f - pumpBounds.min.y;
            pump.transform.position += Vector3.up * offsetY;
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == childName) return child;
            }

            return null;
        }

        private static FaucetWaterInteraction EnsureFaucetWaterInteraction()
        {
            var sink = GameObject.Find(SinkName);
            var zone = GameObject.Find(FaucetWaterZoneName);
            bool createdZone = zone == null;
            if (zone == null) zone = new GameObject(FaucetWaterZoneName);

            var water = zone.GetComponent<FaucetWaterInteraction>();
            if (water == null) water = zone.AddComponent<FaucetWaterInteraction>();

            bool hasAuthoredStream = zone.transform.Find("WaterStreamStart") != null && zone.transform.Find("WaterStreamEnd") != null;
            if (createdZone || !hasAuthoredStream)
            {
                var sinkBounds = sink != null ? CalculateRendererBounds(sink) : new Bounds(new Vector3(1.15f, 0.85f, -2.71f), new Vector3(0.64f, 0.88f, 0.60f));
                var streamStart = new Vector3(sinkBounds.center.x, sinkBounds.max.y + 0.08f, sinkBounds.center.z);
                var streamEnd = new Vector3(sinkBounds.center.x, sinkBounds.center.y - 0.04f, sinkBounds.center.z);
                zone.transform.position = streamStart;
                water.Configure(streamStart, streamEnd);
            }
            else
            {
                water.RefreshVisualFromAnchors();
            }

            var collider = zone.GetComponent<SphereCollider>();
            if (collider == null) collider = zone.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = 0.24f;

            var so = new SerializedObject(water);
            so.FindProperty("activationRadius").floatValue = 0.24f;
            so.FindProperty("interactionEnabled").boolValue = false;
            so.ApplyModifiedProperties();
            return water;
        }

        private static FirstPersonHandCameraRig EnsureFirstPersonRig(Transform pump)
        {
            var go = GameObject.Find(FirstPersonRigName);
            if (go == null) go = new GameObject(FirstPersonRigName);
            var rig = go.GetComponent<FirstPersonHandCameraRig>();
            if (rig == null) rig = go.AddComponent<FirstPersonHandCameraRig>();

            var cameraPosition = pump.position + new Vector3(0.42f, 0.50f, -0.50f);
            var lookAt = pump.position + new Vector3(0.02f, 0.08f, 0.02f);
            rig.Configure(cameraPosition, lookAt);
            var so = new SerializedObject(rig);
            so.FindProperty("applyToMainCameraOnStart").boolValue = false;
            so.ApplyModifiedProperties();
            return rig;
        }

        private static CinemachineCamera EnsureCloseupVCam(FirstPersonHandCameraRig cameraRig)
        {
            var go = GameObject.Find(CloseupVCamName);
            if (go == null) go = new GameObject(CloseupVCamName);
            var vcam = go.GetComponent<CinemachineCamera>();
            if (vcam == null) vcam = go.AddComponent<CinemachineCamera>();
            vcam.Priority = 20;
            go.transform.position = cameraRig.CameraPose.position;
            go.transform.rotation = cameraRig.CameraPose.rotation;
            return vcam;
        }

        private static ToolInteraction3DStepController EnsureToolController3D()
        {
            var existing = Object.FindFirstObjectByType<ToolInteraction3DStepController>();
            if (existing != null) return existing;
            var hostName = "ToolInteraction3DStepController";
            var host = GameObject.Find(hostName);
            if (host == null) host = new GameObject(hostName);
            return host.AddComponent<ToolInteraction3DStepController>();
        }

        private static HandHygieneAnimator EnsureHandHygieneAnimator(GameObject pump, GameObject leftHand, GameObject rightHand)
        {
            var existing = Object.FindFirstObjectByType<HandHygieneAnimator>();
            var host = existing != null ? existing.gameObject : GameObject.Find("HandHygieneAnimator");
            if (host == null) host = new GameObject("HandHygieneAnimator");
            var animator = host.GetComponent<HandHygieneAnimator>();
            if (animator == null) animator = host.AddComponent<HandHygieneAnimator>();

            var so = new SerializedObject(animator);
            so.FindProperty("pumpReference").objectReferenceValue = pump != null ? pump.transform : null;
            so.FindProperty("leftHand").objectReferenceValue = leftHand != null ? leftHand.GetComponent<Hand3DController>() : null;
            so.FindProperty("rightHand").objectReferenceValue = rightHand != null ? rightHand.GetComponent<Hand3DController>() : null;
            so.FindProperty("leftFingers").objectReferenceValue = leftHand != null ? leftHand.GetComponent<FingerPoseController>() : null;
            so.FindProperty("rightFingers").objectReferenceValue = rightHand != null ? rightHand.GetComponent<FingerPoseController>() : null;
            so.FindProperty("leftAnimator").objectReferenceValue = leftHand != null ? leftHand.GetComponent<Animator>() : null;
            so.FindProperty("rightAnimator").objectReferenceValue = rightHand != null ? rightHand.GetComponent<Animator>() : null;
            so.ApplyModifiedProperties();
            return animator;
        }

        private static void AssignHandHygieneAnimator(ToolInteraction3DStepController ctrl, HandHygieneAnimator animator)
        {
            var so = new SerializedObject(ctrl);
            var prop = so.FindProperty("handHygieneAnimator");
            if (prop != null && prop.objectReferenceValue != animator)
            {
                prop.objectReferenceValue = animator;
                so.ApplyModifiedProperties();
            }
        }

        private static void AssignControllerToRunner(ScenarioRunner runner, ToolInteraction3DStepController ctrl)
        {
            var so = new SerializedObject(runner);
            var prop = so.FindProperty("toolController3D");
            if (prop != null && prop.objectReferenceValue != ctrl)
            {
                prop.objectReferenceValue = ctrl;
                so.ApplyModifiedProperties();
            }
        }

        private static void EnablePourStepsIn3DSwitch(ScenarioRunner runner)
        {
            var scenarioField = new SerializedObject(runner).FindProperty("scenario");
            if (scenarioField == null || scenarioField.objectReferenceValue == null) return;
            var scenario = scenarioField.objectReferenceValue as NursingScenario;
            if (scenario == null) return;

            var so = new SerializedObject(runner);
            var listProp = so.FindProperty("physicalStepIds");
            if (listProp == null) return;

            int added = 0;
            foreach (var step in scenario.steps)
            {
                if (step is not ToolInteractionStep tool || tool.kind != InteractionKind.Pour) continue;
                if (string.IsNullOrEmpty(tool.stepId)) continue;
                if (ContainsString(listProp, tool.stepId)) continue;

                int idx = listProp.arraySize;
                listProp.InsertArrayElementAtIndex(idx);
                listProp.GetArrayElementAtIndex(idx).stringValue = tool.stepId;
                added++;
            }

            so.ApplyModifiedProperties();
            if (added == 0) Debug.Log("[Phase3] 추가할 Pour-kind ToolInteractionStep이 없습니다.");
            else Debug.Log($"[Phase3] physicalStepIds에 Pour step {added}개 추가.");
        }

        private static bool ContainsString(SerializedProperty listProp, string value)
        {
            for (int i = 0; i < listProp.arraySize; i++)
            {
                if (listProp.GetArrayElementAtIndex(i).stringValue == value) return true;
            }
            return false;
        }
    }
}
