using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace NursingSim.EditorTools
{
    public static class ClinicalHandAnimationAuthoringTool
    {
        private const string SourceModelPath = "Assets/ThirdParty/ArmTutorial/doctorArms.fbx";
        private const string Folder = "Assets/_Project/Art/Animations/Hands";
        private const string ControllerPath = Folder + "/ClinicalHand.controller";

        [MenuItem("Tools/Nursing Sim/Phase 3/0b. Build Clinical Hand Animation Assets")]
        public static void BuildClinicalHandAnimationAssets()
        {
            EnsureFolder(Folder);

            var probeRoot = BuildPathProbe();
            try
            {
                var idle = CreateClip("Hand_Idle.anim", probeRoot, 0f, 0f, 0f, 0f, false);
                var open = CreateClip("Hand_Open.anim", probeRoot, 0f, 0f, 0f, 0f, false);
                var grip = CreateClip("Hand_Grip.anim", probeRoot, 1f, 1f, 1f, 1f, false);
                var pinch = CreateClip("Hand_Pinch.anim", probeRoot, 0.75f, 0.8f, 0.1f, 0.1f, false);
                var rub = CreateClip("Hand_RubLoop.anim", probeRoot, 0.12f, 0.1f, 0.08f, 0.08f, true);
                var pump = CreateClip("Hand_PumpPress.anim", probeRoot, 0.35f, 0.45f, 0.25f, 0.2f, false);
                CreateController(idle, open, grip, pinch, rub, pump);
            }
            finally
            {
                if (probeRoot != null) Object.DestroyImmediate(probeRoot);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ClinicalHandAnimation] Clinical hand clips and controller generated.");
        }

        private static GameObject BuildPathProbe()
        {
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(SourceModelPath);
            if (model == null)
            {
                Debug.LogWarning($"[ClinicalHandAnimation] Source model not found: {SourceModelPath}");
                return new GameObject("ClinicalHandPathProbe");
            }

            var root = new GameObject("ClinicalHandPathProbe");
            var visual = (GameObject)PrefabUtility.InstantiatePrefab(model);
            visual.name = "ArmVisual";
            visual.transform.SetParent(root.transform, false);
            return root;
        }

        private static AnimationClip CreateClip(
            string fileName,
            GameObject probeRoot,
            float thumbCurl,
            float indexCurl,
            float middleCurl,
            float ringCurl,
            bool looping)
        {
            var path = $"{Folder}/{fileName}";
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip { frameRate = 30f };
                AssetDatabase.CreateAsset(clip, path);
            }
            else
            {
                ClearCurves(clip);
            }

            clip.frameRate = 30f;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = looping;
            AnimationUtility.SetAnimationClipSettings(clip, settings);

            AddHandCurves(clip, probeRoot.transform, "Right", thumbCurl, indexCurl, middleCurl, ringCurl, looping);
            AddHandCurves(clip, probeRoot.transform, "Left", thumbCurl, indexCurl, middleCurl, ringCurl, looping);
            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static void AddHandCurves(AnimationClip clip, Transform root, string side, float thumb, float index, float middle, float ring, bool looping)
        {
            AddFingerCurves(clip, root, side, "Thumb", thumb, new Vector3(-12f, 7f, 0f), looping);
            AddFingerCurves(clip, root, side, "Index", index, new Vector3(-32f, 0f, 0f), looping);
            AddFingerCurves(clip, root, side, "Middle", middle, new Vector3(-32f, 0f, 0f), looping);
            AddFingerCurves(clip, root, side, "Ring", ring, new Vector3(-32f, 0f, 0f), looping);
            AddFingerCurves(clip, root, side, "Pinky", ring, new Vector3(-32f, 0f, 0f), looping);

            if (looping)
            {
                var hand = FindChildRecursive(root, $"mixamorig:{side}Hand");
                if (hand != null)
                {
                    string handPath = AnimationUtility.CalculateTransformPath(hand, root);
                    AddRotationCurve(clip, handPath, Quaternion.Euler(0f, 0f, side == "Right" ? -5f : 5f), Quaternion.Euler(0f, 0f, side == "Right" ? 5f : -5f));
                }
            }
        }

        private static void AddFingerCurves(AnimationClip clip, Transform root, string side, string finger, float curl, Vector3 euler, bool looping)
        {
            for (int i = 1; i <= 4; i++)
            {
                var bone = FindChildRecursive(root, $"mixamorig:{side}Hand{finger}{i}");
                if (bone == null) continue;
                string path = AnimationUtility.CalculateTransformPath(bone, root);
                float weight = i == 1 ? 0.65f : 1f;
                var rot = Quaternion.Euler(euler * (curl * weight));
                if (looping) AddRotationCurve(clip, path, rot, Quaternion.Inverse(rot));
                else AddRotationCurve(clip, path, Quaternion.identity, rot);
            }
        }

        private static void AddRotationCurve(AnimationClip clip, string path, Quaternion a, Quaternion b)
        {
            var times = new[] { 0f, 0.25f, 0.5f };
            var rots = new[] { a, b, a };
            SetCurve(clip, path, "m_LocalRotation.x", times, new[] { rots[0].x, rots[1].x, rots[2].x });
            SetCurve(clip, path, "m_LocalRotation.y", times, new[] { rots[0].y, rots[1].y, rots[2].y });
            SetCurve(clip, path, "m_LocalRotation.z", times, new[] { rots[0].z, rots[1].z, rots[2].z });
            SetCurve(clip, path, "m_LocalRotation.w", times, new[] { rots[0].w, rots[1].w, rots[2].w });
        }

        private static void SetCurve(AnimationClip clip, string path, string property, float[] times, float[] values)
        {
            var curve = new AnimationCurve();
            for (int i = 0; i < times.Length; i++) curve.AddKey(new Keyframe(times[i], values[i]));
            clip.SetCurve(path, typeof(Transform), property, curve);
        }

        private static void CreateController(AnimationClip idle, AnimationClip open, AnimationClip grip, AnimationClip pinch, AnimationClip rub, AnimationClip pump)
        {
            if (File.Exists(ControllerPath)) AssetDatabase.DeleteAsset(ControllerPath);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter("GripAmount", AnimatorControllerParameterType.Float);
            controller.AddParameter("FingerCurl", AnimatorControllerParameterType.Float);
            controller.AddParameter("IsRubbing", AnimatorControllerParameterType.Bool);
            controller.AddParameter("PumpPress", AnimatorControllerParameterType.Trigger);

            var sm = controller.layers[0].stateMachine;
            sm.states = System.Array.Empty<ChildAnimatorState>();
            var idleState = sm.AddState("Idle");
            idleState.motion = idle;
            sm.defaultState = idleState;
            sm.AddState("Open").motion = open;
            sm.AddState("Grip").motion = grip;
            sm.AddState("Pinch").motion = pinch;
            sm.AddState("RubLoop").motion = rub;
            sm.AddState("PumpPress").motion = pump;
            EditorUtility.SetDirty(controller);
        }

        private static void ClearCurves(AnimationClip clip)
        {
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
            {
                AnimationUtility.SetEditorCurve(clip, binding, null);
            }
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == childName) return child;
            }

            return null;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }
    }
}
