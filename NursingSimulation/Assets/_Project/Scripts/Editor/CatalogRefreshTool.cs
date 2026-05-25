using NursingSim.Core;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NursingSim.EditorTools
{
    /// <summary>
    /// Replaces primitive placeholders in the current scene with prefabs from AssetCatalog.
    /// Run this menu after assigning a new prefab to a catalog slot — no need to delete and
    /// re-wire each placeholder by hand.
    /// </summary>
    public static class CatalogRefreshTool
    {
        // (role, sceneObjectPath, mode)
        // mode "single": the named GameObject IS the visual; replace MeshFilter/Renderer/Collider in place, keep scripts
        // mode "child":  the named GameObject is a wrapper; replace its single visual child entirely
        private static readonly (string role, string scenePath, string mode)[] Mappings =
        {
            ("HandSanitizerPump", "HandSanitizerPump",           "single"),
            ("PatientBody",       "Patient_Placeholder/Body",     "child"),
            ("Tray",              "Tray_Placeholder/TrayTop",     "child"),
            ("Cabinet",           "Cabinet_Placeholder/Body",     "child"),
            ("Syringe3cc",        "Tray_Placeholder/Syringe",     "child"),
            ("Ampoule",           "Tray_Placeholder/Ampoule",     "child"),
            ("AlcoholSwab",       "Tray_Placeholder/AlcoholSwab", "child"),
            ("Gauze",             "Tray_Placeholder/Gauze",       "child"),
            ("SharpsContainer",   "SharpsContainer",              "single"),
            ("Bed",               "Bed_Placeholder",              "single"),
            ("Floor",             "Room/Floor",                   "single"),
            ("Wall_N",            "Room/Wall_N",                  "single"),
            ("Wall_S",            "Room/Wall_S",                  "single"),
            ("Wall_E",            "Room/Wall_E",                  "single"),
            ("Wall_W",            "Room/Wall_W",                  "single"),
            ("Door",              "Room/Door",                    "single"),
            ("Ceiling",           "Room/Ceiling",                 "single"),
        };

        [MenuItem("Tools/Nursing Sim/Phase 0+/Refresh Scene Visuals from Catalog")]
        public static void RefreshAll()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/_Project/Data/AssetCatalog.asset");
            if (catalog == null)
            {
                EditorUtility.DisplayDialog("Catalog Refresh",
                    "AssetCatalog.asset이 없습니다.\nTools > Nursing Sim > Phase 0+ > Create or Open AssetCatalog 먼저 실행하세요.",
                    "확인");
                return;
            }

            int refreshed = 0;
            int skippedNoPrefab = 0;
            int skippedNotInScene = 0;

            foreach (var (role, scenePath, mode) in Mappings)
            {
                var slot = catalog.Find(role);
                if (slot == null || slot.prefab == null)
                {
                    skippedNoPrefab++;
                    continue;
                }

                var target = FindByPath(scenePath);
                if (target == null)
                {
                    skippedNotInScene++;
                    continue;
                }

                if (mode == "single") ReplaceInPlace(target, slot);
                else ReplaceChildEntirely(target, slot);

                refreshed++;
                Debug.Log($"[Phase0+] Refreshed '{role}' at '{scenePath}' (offset={slot.instancePosition}, rot={slot.instanceRotationEuler}, scale={slot.instanceScale}).");
            }

            if (refreshed > 0)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            }

            EditorUtility.DisplayDialog("Catalog Refresh",
                $"교체됨: {refreshed}\n슬롯 prefab 없음: {skippedNoPrefab}\n씬에 없음: {skippedNotInScene}",
                "확인");
        }

        // ---- helpers ----

        private static GameObject FindByPath(string path)
        {
            var parts = path.Split('/');
            var root = GameObject.Find(parts[0]);
            if (root == null || parts.Length == 1) return root;
            var child = root.transform.Find(parts[1]);
            return child != null ? child.gameObject : null;
        }

        // mode="single": keep the GameObject (and its scripts) but swap visual + collider for the prefab as a child.
        // Visual child carries the catalog's instance transform; host position is preserved. Idempotent —
        // removes both primitive components AND any previous "Visual" child from earlier Refresh runs.
        private static void ReplaceInPlace(GameObject host, AssetCatalog.Slot slot)
        {
            var mf = host.GetComponent<MeshFilter>();
            if (mf != null) Object.DestroyImmediate(mf);
            var mr = host.GetComponent<MeshRenderer>();
            if (mr != null) Object.DestroyImmediate(mr);
            var col = host.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
            var existingVisual = host.transform.Find("Visual");
            if (existingVisual != null) Object.DestroyImmediate(existingVisual.gameObject);

            var visual = (GameObject)PrefabUtility.InstantiatePrefab(slot.prefab);
            visual.name = "Visual";
            visual.transform.SetParent(host.transform, false);
            ApplyInstanceTransform(visual.transform, slot);
            SetLayerRecursively(visual, host.layer);
        }

        // mode="child": destroy the named child and instantiate prefab in its place.
        // Position comes from the original placeholder (so tray layout is preserved), but
        // rotation/scale come from the catalog slot (so prefab is NOT squished to placeholder scale).
        private static void ReplaceChildEntirely(GameObject child, AssetCatalog.Slot slot)
        {
            var parent = child.transform.parent;
            var name = child.name;
            var placeholderLocalPos = child.transform.localPosition;
            int layer = child.layer;

            Object.DestroyImmediate(child);

            var newChild = (GameObject)PrefabUtility.InstantiatePrefab(slot.prefab);
            newChild.name = name;
            newChild.transform.SetParent(parent, false);
            // Preserve placeholder position, then add catalog offset.
            newChild.transform.localPosition = placeholderLocalPos + slot.instancePosition;
            newChild.transform.localRotation = Quaternion.Euler(slot.instanceRotationEuler);
            newChild.transform.localScale = slot.instanceScale;
            SetLayerRecursively(newChild, layer);
        }

        private static void ApplyInstanceTransform(Transform t, AssetCatalog.Slot slot)
        {
            t.localPosition = slot.instancePosition;
            t.localRotation = Quaternion.Euler(slot.instanceRotationEuler);
            t.localScale = slot.instanceScale;
        }

        private static void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform t in go.transform) SetLayerRecursively(t.gameObject, layer);
        }
    }
}
