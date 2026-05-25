using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NursingSim.EditorTools
{
    /// <summary>
    /// Repositions scene placeholders (Patient, Tray, Pump, Sharps) relative to E1_1 room's
    /// built-in Bed-material node, after CatalogRefreshTool has instantiated the E1_1 prefab
    /// under Floor/Visual. Also deactivates Cabinet_Placeholder since E1_1 has built-in cabinets.
    /// </summary>
    public static class Phase2AlignTool
    {
        [MenuItem("Tools/Nursing Sim/Phase 2/3. Align Scene to Room")]
        public static void AlignToRoom()
        {
            var room = GameObject.Find("Room");
            if (room == null)
            {
                EditorUtility.DisplayDialog("Align to Room",
                    "씬에서 'Room'을 찾을 수 없습니다.\n먼저 Phase 2 > 5. Build Treatment Room from Parts 를 실행하세요.",
                    "확인");
                return;
            }

            // Prefer the actual mesh node ("Bed-material") over its empty wrapper ("Bed").
            var bed = FindMaterialNode(room.transform, "Bed") ?? FindByNameContains(room.transform, "Bed");
            if (bed == null)
            {
                EditorUtility.DisplayDialog("Align to Room",
                    "Room 내에서 침대 노드를 찾을 수 없습니다.\nE1_Parts에 Bed 부품이 있는지 확인하세요.",
                    "확인");
                return;
            }

            // Compute bed top surface in world space using Renderer bounds.
            var bedRenderer = bed.GetComponentInChildren<Renderer>();
            Vector3 bedTop;
            if (bedRenderer != null)
            {
                var b = bedRenderer.bounds;
                bedTop = new Vector3(b.center.x, b.max.y, b.center.z);
            }
            else
            {
                bedTop = bed.position + Vector3.up * 0.5f;
            }

            MovePlaceholder("Patient_Placeholder", bedTop + new Vector3(0f, 0.05f, 0f));
            MovePlaceholder("Tray_Placeholder",    bedTop + new Vector3( 1.2f, -0.3f, 0f));
            MovePlaceholder("HandSanitizerPump",   bedTop + new Vector3(-1.5f, -0.6f, 0f));
            MovePlaceholder("SharpsContainer",     bedTop + new Vector3( 1.5f, -1.0f, 0.3f));

            // E1_1 already has Cabinet1 + Cabinet2 → hide our redundant placeholder.
            var cabinet = GameObject.Find("Cabinet_Placeholder");
            if (cabinet != null && cabinet.activeSelf)
            {
                cabinet.SetActive(false);
                Debug.Log("[Phase2 Align] Cabinet_Placeholder 비활성화 (E1_1 Cabinet1이 대체).");
            }

            // Floor host renderer was destroyed by Refresh; nothing else to align.

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            EditorUtility.DisplayDialog("Align to Room",
                $"침대('{bed.name}') 상면 {bedTop} 기준 재배치 완료.\n\n환자 자세가 어색하면 AssetCatalog의 PatientBody.Instance Rotation Euler 미세 조정.",
                "확인");
            Debug.Log($"[Phase2 Align] Bed '{bed.name}' at {bed.position}, top {bedTop}. Placeholders repositioned.");
        }

        // Prefers material mesh nodes (e.g., "Bed-material") which have non-zero local translation.
        private static Transform FindMaterialNode(Transform root, string substring)
        {
            if (root.name.Contains(substring) && root.name.EndsWith("-material")) return root;
            foreach (Transform child in root)
            {
                var found = FindMaterialNode(child, substring);
                if (found != null) return found;
            }
            return null;
        }

        private static Transform FindByNameContains(Transform root, string substring)
        {
            if (root.name.Contains(substring)) return root;
            foreach (Transform child in root)
            {
                var found = FindByNameContains(child, substring);
                if (found != null) return found;
            }
            return null;
        }

        private static void MovePlaceholder(string name, Vector3 worldPos)
        {
            var go = GameObject.Find(name);
            if (go == null)
            {
                Debug.LogWarning($"[Phase2 Align] '{name}'를 씬에서 찾지 못함 — 스킵.");
                return;
            }
            go.transform.position = worldPos;
            Debug.Log($"[Phase2 Align] {name} → {worldPos}");
        }
    }
}
