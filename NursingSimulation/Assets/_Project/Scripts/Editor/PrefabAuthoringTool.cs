using System.IO;
using NursingSim.Gameplay.Hand3D;
using UnityEditor;
using UnityEngine;

namespace NursingSim.EditorTools
{
    public static class PrefabAuthoringTool
    {
        private const string ToolPrefabDir = "Assets/_Project/Prefabs/Tools";
        private const string EnvPrefabDir = "Assets/_Project/Prefabs/Environment";
        private const string CharPrefabDir = "Assets/_Project/Prefabs/Characters";

        [MenuItem("Tools/Nursing Sim/Phase 0+/Create or Open AssetCatalog")]
        private static void CreateOrOpenAssetCatalog()
        {
            var catalog = AssetCatalogHelper.GetOrCreate();
            Selection.activeObject = catalog;
            EditorGUIUtility.PingObject(catalog);
            Debug.Log("[Phase0+] AssetCatalog ready. Inspector에서 role 슬롯에 prefab을 드래그하세요.");
        }

        [MenuItem("Assets/Nursing Sim/Build Tool Prefab", true)]
        private static bool ValidateBuildTool() => GetSelectedModel() != null;

        [MenuItem("Assets/Nursing Sim/Build Tool Prefab")]
        private static void BuildToolPrefab()
        {
            BuildPrefab(ToolPrefabDir, attachGrabbable: true, attachRigidbody: true);
        }

        [MenuItem("Assets/Nursing Sim/Build Environment Prefab", true)]
        private static bool ValidateBuildEnv() => GetSelectedModel() != null;

        [MenuItem("Assets/Nursing Sim/Build Environment Prefab")]
        private static void BuildEnvironmentPrefab()
        {
            BuildPrefab(EnvPrefabDir, attachGrabbable: false, attachRigidbody: false);
        }

        [MenuItem("Assets/Nursing Sim/Build Character Prefab", true)]
        private static bool ValidateBuildChar() => GetSelectedModel() != null;

        [MenuItem("Assets/Nursing Sim/Build Character Prefab")]
        private static void BuildCharacterPrefab()
        {
            BuildPrefab(CharPrefabDir, attachGrabbable: false, attachRigidbody: false);
        }

        // ---- helpers ----

        private static GameObject GetSelectedModel()
        {
            var sel = Selection.activeObject as GameObject;
            if (sel == null) return null;
            var path = AssetDatabase.GetAssetPath(sel);
            if (string.IsNullOrEmpty(path)) return null;
            var ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext != ".fbx" && ext != ".obj" && ext != ".gltf" && ext != ".glb") return null;
            return sel;
        }

        private static void BuildPrefab(string dir, bool attachGrabbable, bool attachRigidbody)
        {
            var model = GetSelectedModel();
            if (model == null)
            {
                EditorUtility.DisplayDialog("Build Prefab", "FBX/OBJ/GLTF 모델을 Project 창에서 선택하세요.", "확인");
                return;
            }

            if (!AssetDatabase.IsValidFolder(dir))
            {
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
            instance.name = model.name;

            if (instance.GetComponentInChildren<Collider>() == null)
            {
                // Convex MeshCollider only works for solid 3D meshes; flat/coplanar surfaces (walls,
                // glass panels, floors) fail PhysX's QuickHull. So we only force convex for tools
                // that need dynamic Rigidbody collision. Environment/character prefabs get non-convex.
                AddDefaultCollider(instance, useConvex: attachRigidbody);
            }

            if (attachRigidbody && instance.GetComponent<Rigidbody>() == null)
            {
                var rb = instance.AddComponent<Rigidbody>();
                rb.mass = 0.1f;
                rb.linearDamping = 4f;
                rb.angularDamping = 6f;
            }

            if (attachGrabbable && instance.GetComponent<GrabbablePhysicalTool>() == null)
            {
                instance.AddComponent<GrabbablePhysicalTool>();
                var attachPoint = new GameObject("AttachPoint");
                attachPoint.transform.SetParent(instance.transform, false);
            }

            var savedPath = $"{dir}/{model.name}.prefab";
            savedPath = AssetDatabase.GenerateUniqueAssetPath(savedPath);
            var saved = PrefabUtility.SaveAsPrefabAsset(instance, savedPath);
            Object.DestroyImmediate(instance);

            Selection.activeObject = saved;
            EditorGUIUtility.PingObject(saved);
            Debug.Log($"[PrefabAuthoring] Created {savedPath}. Tip: open AssetCatalog.asset and assign this prefab to the matching role slot.");
        }

        private static void AddDefaultCollider(GameObject go, bool useConvex)
        {
            var renderers = go.GetComponentsInChildren<MeshRenderer>();
            foreach (var r in renderers)
            {
                if (r.GetComponent<Collider>() == null)
                {
                    var mc = r.gameObject.AddComponent<MeshCollider>();
                    if (useConvex) mc.convex = true;
                }
            }
            if (renderers.Length == 0)
            {
                go.AddComponent<BoxCollider>();
            }
        }
    }
}
