using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace NursingSim.EditorTools
{
    /// <summary>
    /// Extracts each top-level wrapper child of E1_1.prefab as a standalone prefab
    /// under Prefabs/Environment/E1_Parts/. Inner mesh local transforms are preserved,
    /// so placing all extracted parts at world (0,0,0) reconstructs the original room layout.
    /// </summary>
    public static class E1PartsExtractor
    {
        private const string E1Path = "Assets/_Project/Prefabs/Environment/E1_1.prefab";
        private const string OutDir = "Assets/_Project/Prefabs/Environment/E1_Parts";
        private const string SceneGroupName = "Collada visual scene group";

        [MenuItem("Tools/Nursing Sim/Phase 2/4. Extract E1_1 Parts to Prefabs")]
        public static void Extract()
        {
            var e1Prefab = AssetDatabase.LoadAssetAtPath<GameObject>(E1Path);
            if (e1Prefab == null)
            {
                EditorUtility.DisplayDialog("Extract E1_1 Parts",
                    $"E1_1.prefab을 찾을 수 없습니다:\n{E1Path}",
                    "확인");
                return;
            }

            if (!AssetDatabase.IsValidFolder(OutDir))
            {
                Directory.CreateDirectory(OutDir);
                AssetDatabase.Refresh();
            }

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(e1Prefab);
            instance.hideFlags = HideFlags.HideAndDontSave;

            var sceneGroup = FindChildRecursive(instance.transform, SceneGroupName);
            if (sceneGroup == null)
            {
                Object.DestroyImmediate(instance);
                EditorUtility.DisplayDialog("Extract E1_1 Parts",
                    $"E1_1 내부에서 '{SceneGroupName}' 노드를 찾을 수 없습니다.",
                    "확인");
                return;
            }

            var children = new List<Transform>();
            foreach (Transform t in sceneGroup) children.Add(t);

            int saved = 0;
            var usedNames = new HashSet<string>();
            foreach (var child in children)
            {
                var name = GetMeaningfulName(child, usedNames);
                usedNames.Add(name);
                var path = $"{OutDir}/{name}.prefab";

                // Clone the subtree so we can save it without affecting the source prefab.
                var clone = Object.Instantiate(child.gameObject);
                clone.name = name;
                PrefabUtility.SaveAsPrefabAsset(clone, path);
                Object.DestroyImmediate(clone);

                Debug.Log($"[E1PartsExtractor] '{child.name}' → {path}");
                saved++;
            }

            Object.DestroyImmediate(instance);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("E1 Parts Extracted",
                $"{saved}개 prefab 생성 완료.\n위치: {OutDir}\n\n다음: Phase 2 > 5. Build Treatment Room from Parts",
                "확인");
        }

        private static string GetMeaningfulName(Transform t, HashSet<string> used)
        {
            // Try inner mesh material name (e.g., child "Commode-material" → "Commode").
            string name = t.name;
            if (t.childCount > 0)
            {
                var meshNode = t.GetChild(0);
                var mn = meshNode.name;
                if (mn.EndsWith("-material"))
                {
                    name = mn.Substring(0, mn.Length - "-material".Length);
                }
            }
            // Strip ".001" / ".002" suffix from raw wrapper names like "Cube.001".
            int dot = name.IndexOf('.');
            if (dot > 0 && name.Substring(0, dot).Length > 2)
            {
                name = name.Substring(0, dot);
            }
            // De-duplicate.
            string baseName = name;
            int idx = 1;
            while (used.Contains(name)) name = $"{baseName}_{++idx}";
            return name;
        }

        private static Transform FindChildRecursive(Transform t, string name)
        {
            if (t.name == name) return t;
            foreach (Transform c in t)
            {
                var found = FindChildRecursive(c, name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
