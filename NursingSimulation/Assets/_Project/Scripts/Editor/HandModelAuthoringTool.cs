using System.IO;
using System.Collections.Generic;
using NursingSim.Gameplay.Hand3D;
using UnityEditor;
using UnityEngine;

namespace NursingSim.EditorTools
{
    public static class HandModelAuthoringTool
    {
        private const string SourceModelPath = "Assets/ThirdParty/ArmTutorial/doctorArms.fbx";
        private const string SourceControllerPath = "Assets/ThirdParty/ArmTutorial/armsController.controller";
        private const string ClinicalControllerPath = "Assets/_Project/Art/Animations/Hands/ClinicalHand.controller";
        private const string BodyMaterialPath = "Assets/ThirdParty/ArmTutorial/Scenes/Ch16_Body.mat";
        private const string LowerBodyMaterialPath = "Assets/ThirdParty/ArmTutorial/Scenes/MATERIAL/Ch16_body1.mat";
        private const string EyelashesMaterialPath = "Assets/ThirdParty/ArmTutorial/Scenes/MATERIAL/Ch16_eyelashes.mat";
        private const string RightHandPrefabPath = "Assets/_Project/Prefabs/Characters/PlayerHand_Right.prefab";
        private const string LeftHandPrefabPath = "Assets/_Project/Prefabs/Characters/PlayerHand_Left.prefab";
        private const string GeneratedMeshFolder = "Assets/_Project/Art/Models/Hands/Generated";

        [MenuItem("Tools/Nursing Sim/Phase 3/0. Build Player Hand Prefabs")]
        public static void BuildPlayerHandPrefabs()
        {
            ArmTutorialMaterialFixTool.FixArmTutorialMaterials();
            ClinicalHandAnimationAuthoringTool.BuildClinicalHandAnimationAssets();
            AssetDatabase.ImportAsset(SourceModelPath, ImportAssetOptions.ForceUpdate);

            var model = AssetDatabase.LoadAssetAtPath<GameObject>(SourceModelPath);
            if (model == null)
            {
                EditorUtility.DisplayDialog("Build Player Hand", $"손 모델을 찾을 수 없습니다.\n{SourceModelPath}", "확인");
                return;
            }

            EnsureGeneratedMeshFolder();
            BuildHandPrefab(model, RightHandPrefabPath, "PlayerHand_Right", HandSide.Right);
            BuildHandPrefab(model, LeftHandPrefabPath, "PlayerHand_Left", HandSide.Left);
        }

        public static void BuildPlayerHandPrefab()
        {
            BuildPlayerHandPrefabs();
        }

        private static void BuildHandPrefab(GameObject model, string prefabPath, string instanceName, HandSide handSide)
        {
            var prefabDir = Path.GetDirectoryName(prefabPath);
            if (!AssetDatabase.IsValidFolder(prefabDir))
            {
                Directory.CreateDirectory(prefabDir);
                AssetDatabase.Refresh();
            }

            var instance = new GameObject(instanceName);
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
            visual.name = "ArmVisual";
            visual.transform.SetParent(instance.transform, false);
            PrefabUtility.UnpackPrefabInstance(visual, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);

            AssignArmTutorialMaterials(visual);
            PrepareSingleArmVisual(visual, handSide);
            AlignVisualShoulderToRoot(visual.transform, handSide);
            EnsureRuntimeComponents(instance, handSide);
            EnsureRigAndAnchors(instance, handSide);
            AssignAnimatorController(instance);
            AssignPalm(instance);

            var saved = PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            Object.DestroyImmediate(instance);

            Selection.activeObject = saved;
            EditorGUIUtility.PingObject(saved);
            Debug.Log($"[HandModelAuthoring] Created {prefabPath}");
        }

        private static void EnsureRuntimeComponents(GameObject instance, HandSide handSide)
        {
            var rb = instance.GetComponent<Rigidbody>();
            if (rb == null) rb = instance.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;

            var sc = instance.GetComponent<SphereCollider>();
            if (sc == null) sc = instance.AddComponent<SphereCollider>();
            sc.radius = 0.04f;
            sc.isTrigger = true;

            if (instance.GetComponent<Hand3DController>() == null) instance.AddComponent<Hand3DController>();
            if (instance.GetComponent<HandPoseController>() == null) instance.AddComponent<HandPoseController>();
            if (instance.GetComponent<ArmRigReferences>() == null) instance.AddComponent<ArmRigReferences>();
            if (instance.GetComponent<TwoBoneArmIK>() == null) instance.AddComponent<TwoBoneArmIK>();
            if (instance.GetComponent<FingerPoseController>() == null) instance.AddComponent<FingerPoseController>();

            var ctrl = instance.GetComponent<Hand3DController>();
            var so = new SerializedObject(ctrl);
            so.FindProperty("handSide").enumValueIndex = (int)handSide;
            so.FindProperty("acceptInput").boolValue = true;
            so.FindProperty("startLocked").boolValue = false;
            ApplyHandControlDefaults(so);
            so.ApplyModifiedProperties();
            ApplyArmIkDefaults(instance.GetComponent<TwoBoneArmIK>());
        }

        private static void AssignAnimatorController(GameObject instance)
        {
            foreach (var childAnimator in instance.GetComponentsInChildren<Animator>(true))
            {
                if (childAnimator.gameObject != instance) Object.DestroyImmediate(childAnimator);
            }

            var animator = instance.GetComponent<Animator>();
            if (animator == null) animator = instance.AddComponent<Animator>();

            var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ClinicalControllerPath);
            if (controller == null) controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(SourceControllerPath);
            if (controller != null) animator.runtimeAnimatorController = controller;
        }

        private static void AssignArmTutorialMaterials(GameObject instance)
        {
            var body = AssetDatabase.LoadAssetAtPath<Material>(BodyMaterialPath);
            var lowerBody = AssetDatabase.LoadAssetAtPath<Material>(LowerBodyMaterialPath);
            var eyelashes = AssetDatabase.LoadAssetAtPath<Material>(EyelashesMaterialPath);

            foreach (var renderer in instance.GetComponentsInChildren<Renderer>(true))
            {
                var material = PickMaterial(renderer.gameObject.name, body, lowerBody, eyelashes);
                if (material == null) continue;
                renderer.sharedMaterial = material;
            }
        }

        private static Material PickMaterial(string rendererName, Material body, Material lowerBody, Material eyelashes)
        {
            if (rendererName.Contains("Eyelashes")) return eyelashes ? eyelashes : body;
            if (rendererName.Contains("Pants") || rendererName.Contains("Shoes")) return lowerBody ? lowerBody : body;
            if (rendererName.StartsWith("Ch16_", System.StringComparison.Ordinal)) return body;
            return null;
        }

        private static void PrepareSingleArmVisual(GameObject visual, HandSide handSide)
        {
            foreach (var renderer in visual.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer is SkinnedMeshRenderer skinned && IsArmMeshRenderer(skinned))
                {
                    var sideMesh = CreateSingleSideMeshAsset(skinned, renderer.name, handSide);
                    skinned.sharedMesh = sideMesh;
                    skinned.localBounds = sideMesh.bounds;
                    skinned.updateWhenOffscreen = true;
                    continue;
                }

                renderer.enabled = false;
            }
        }

        private static bool IsArmMeshRenderer(SkinnedMeshRenderer renderer)
        {
            return renderer.sharedMesh != null &&
                   (renderer.gameObject.name == "Ch16_Body1" || renderer.gameObject.name == "Ch16_Shirt");
        }

        private static Mesh CreateSingleSideMeshAsset(SkinnedMeshRenderer sourceRenderer, string rendererName, HandSide handSide)
        {
            var assetName = $"{rendererName}_{handSide}.asset";
            var assetPath = $"{GeneratedMeshFolder}/{assetName}";
            var copy = CreateSingleSideMesh(sourceRenderer, handSide);

            AssetDatabase.DeleteAsset(assetPath);
            AssetDatabase.CreateAsset(copy, assetPath);
            AssetDatabase.ImportAsset(assetPath);
            return AssetDatabase.LoadAssetAtPath<Mesh>(assetPath);
        }

        private static Mesh CreateSingleSideMesh(SkinnedMeshRenderer sourceRenderer, HandSide handSide)
        {
            var source = sourceRenderer.sharedMesh;
            var copy = new Mesh
            {
                name = $"{source.name}_{handSide}",
                indexFormat = source.indexFormat,
                vertices = source.vertices,
                normals = source.normals,
                tangents = source.tangents,
                colors = source.colors,
                uv = source.uv,
                uv2 = source.uv2,
                uv3 = source.uv3,
                uv4 = source.uv4,
                bindposes = source.bindposes,
                boneWeights = source.boneWeights,
                subMeshCount = source.subMeshCount
            };

            var vertices = source.vertices;
            var boneWeights = source.boneWeights;
            var used = new List<Vector3>();
            for (int submesh = 0; submesh < source.subMeshCount; submesh++)
            {
                var sourceTriangles = source.GetTriangles(submesh);
                var keptTriangles = new List<int>(sourceTriangles.Length);
                for (int i = 0; i < sourceTriangles.Length; i += 3)
                {
                    int a = sourceTriangles[i];
                    int b = sourceTriangles[i + 1];
                    int c = sourceTriangles[i + 2];
                    float centerX = (vertices[a].x + vertices[b].x + vertices[c].x) / 3f;
                    if (!IsTriangleOnSide(centerX, handSide)) continue;
                    if (!IsArmTriangle(sourceRenderer, boneWeights, a, b, c, handSide)) continue;

                    keptTriangles.Add(a);
                    keptTriangles.Add(b);
                    keptTriangles.Add(c);
                    used.Add(vertices[a]);
                    used.Add(vertices[b]);
                    used.Add(vertices[c]);
                }

                copy.SetTriangles(keptTriangles, submesh);
            }

            copy.bounds = CalculateBounds(used);
            return copy;
        }

        private static bool IsArmTriangle(SkinnedMeshRenderer renderer, BoneWeight[] boneWeights, int a, int b, int c, HandSide handSide)
        {
            if (boneWeights == null || boneWeights.Length == 0) return true;

            int armWeightedVertices = 0;
            if (IsArmWeightedVertex(renderer, boneWeights, a, handSide)) armWeightedVertices++;
            if (IsArmWeightedVertex(renderer, boneWeights, b, handSide)) armWeightedVertices++;
            if (IsArmWeightedVertex(renderer, boneWeights, c, handSide)) armWeightedVertices++;
            return armWeightedVertices >= 2;
        }

        private static bool IsArmWeightedVertex(SkinnedMeshRenderer renderer, BoneWeight[] boneWeights, int vertexIndex, HandSide handSide)
        {
            if (vertexIndex < 0 || vertexIndex >= boneWeights.Length) return false;

            var weight = boneWeights[vertexIndex];
            return IsArmBoneWeight(renderer, weight.boneIndex0, weight.weight0, handSide) ||
                   IsArmBoneWeight(renderer, weight.boneIndex1, weight.weight1, handSide) ||
                   IsArmBoneWeight(renderer, weight.boneIndex2, weight.weight2, handSide) ||
                   IsArmBoneWeight(renderer, weight.boneIndex3, weight.weight3, handSide);
        }

        private static bool IsArmBoneWeight(SkinnedMeshRenderer renderer, int boneIndex, float weight, HandSide handSide)
        {
            if (weight < 0.05f || renderer.bones == null || boneIndex < 0 || boneIndex >= renderer.bones.Length) return false;
            var bone = renderer.bones[boneIndex];
            if (bone == null) return false;

            string prefix = handSide == HandSide.Right ? "mixamorig:Right" : "mixamorig:Left";
            string name = bone.name;
            return name == $"{prefix}Arm" ||
                   name == $"{prefix}ForeArm" ||
                   name == $"{prefix}Hand" ||
                   name.StartsWith($"{prefix}Hand", System.StringComparison.Ordinal);
        }

        private static bool IsTriangleOnSide(float centerX, HandSide handSide)
        {
            return handSide == HandSide.Right ? centerX >= 0f : centerX <= 0f;
        }

        private static Bounds CalculateBounds(List<Vector3> vertices)
        {
            if (vertices.Count == 0) return new Bounds(Vector3.zero, Vector3.one * 0.01f);

            var bounds = new Bounds(vertices[0], Vector3.zero);
            for (int i = 1; i < vertices.Count; i++) bounds.Encapsulate(vertices[i]);
            return bounds;
        }

        private static void EnsureRigAndAnchors(GameObject instance, HandSide handSide)
        {
            var rig = instance.GetComponent<ArmRigReferences>();
            rig.AutoBind(handSide);

            var target = rig.HandTarget;
            if (target != null)
            {
                var targetBody = target.GetComponent<Rigidbody>();
                if (targetBody == null) targetBody = target.gameObject.AddComponent<Rigidbody>();
                targetBody.useGravity = false;
                targetBody.isKinematic = true;
                var targetCollider = target.GetComponent<SphereCollider>();
                if (targetCollider == null) targetCollider = target.gameObject.AddComponent<SphereCollider>();
                targetCollider.radius = 0.035f;
                targetCollider.isTrigger = false;
            }
        }

        private static void AlignVisualShoulderToRoot(Transform visualRoot, HandSide handSide)
        {
            var shoulderBoneName = handSide == HandSide.Right ? "mixamorig:RightShoulder" : "mixamorig:LeftShoulder";
            var shoulderBone = FindChildRecursive(visualRoot, shoulderBoneName);
            if (shoulderBone == null) return;

            var shoulderLocal = visualRoot.InverseTransformPoint(shoulderBone.position);
            visualRoot.localPosition = -shoulderLocal;
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == childName) return child;
            }

            return null;
        }

        private static void EnsureGeneratedMeshFolder()
        {
            EnsureFolder("Assets/_Project/Art");
            EnsureFolder("Assets/_Project/Art/Models");
            EnsureFolder("Assets/_Project/Art/Models/Hands");
            EnsureFolder(GeneratedMeshFolder);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }

        private static void AssignPalm(GameObject instance)
        {
            var rig = instance.GetComponent<ArmRigReferences>();
            var palm = rig != null ? rig.Palm : instance.transform.Find("Palm");
            var ctrl = instance.GetComponent<Hand3DController>();
            if (palm == null || ctrl == null) return;

            var so = new SerializedObject(ctrl);
            so.FindProperty("rig").objectReferenceValue = rig;
            so.FindProperty("palm").objectReferenceValue = palm;
            so.FindProperty("handTarget").objectReferenceValue = rig != null ? rig.HandTarget : null;
            if (rig != null && rig.HandTarget != null)
            {
                so.FindProperty("handTargetBody").objectReferenceValue = rig.HandTarget.GetComponent<Rigidbody>();
            }
            ApplyHandControlDefaults(so);
            so.ApplyModifiedProperties();
        }

        private static void ApplyHandControlDefaults(SerializedObject so)
        {
            so.FindProperty("keyboardMoveSpeed").floatValue = 0.65f;
            so.FindProperty("useSceneAuthoredTargetPose").boolValue = true;
            so.FindProperty("keyboardLiftSpeed").floatValue = 0.45f;
            so.FindProperty("depthMoveSpeed").floatValue = 0.002f;
            so.FindProperty("workspaceRadius").floatValue = 1.1f;
            so.FindProperty("handSideLateralRange").vector2Value = new Vector2(-0.7f, 0.85f);
            so.FindProperty("handVerticalRange").vector2Value = new Vector2(-0.75f, 0.32f);
            so.FindProperty("handForwardRange").vector2Value = new Vector2(0.02f, 1.18f);
            so.FindProperty("constrainTargetFromShoulder").boolValue = true;
            so.FindProperty("shoulderOutwardRange").vector2Value = new Vector2(0.22f, 0.72f);
            so.FindProperty("enableCollisionBlocking").boolValue = true;
            so.FindProperty("collisionRadius").floatValue = 0.035f;
            so.FindProperty("handShellCollisionRadius").floatValue = 0.05f;
            so.FindProperty("fingerCollisionRadius").floatValue = 0.018f;
            so.FindProperty("collisionSkin").floatValue = 0.008f;
            so.FindProperty("ignoreOversizedConcaveMeshBlockers").boolValue = true;
            so.FindProperty("oversizedConcaveMeshBlockerMaxExtent").floatValue = 3f;
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
    }
}
