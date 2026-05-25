using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NursingSim.EditorTools
{
    /// <summary>
    /// Builds the treatment Room. Prefers a complete monolithic Room prefab (kidman_room.prefab)
    /// when available. Falls back to tiling individual parts from E1_Parts/.
    /// </summary>
    public static class Phase2RoomBuilder
    {
        private const string PartsDir = "Assets/_Project/Prefabs/Environment/E1_Parts";
        private const string KidmanPath = "Assets/_Project/Prefabs/Environment/kidman_room.prefab";

        [MenuItem("Tools/Nursing Sim/Phase 2/5. Build Treatment Room from Parts")]
        public static void BuildRoom()
        {
            var kidman = AssetDatabase.LoadAssetAtPath<GameObject>(KidmanPath);
            if (kidman != null)
            {
                BuildFromMonolithic(kidman);
                return;
            }

            if (!AssetDatabase.IsValidFolder(PartsDir))
            {
                EditorUtility.DisplayDialog("Build Treatment Room",
                    $"kidman_room.prefab도, {PartsDir} 폴더도 없습니다.\nPhase 2 > 4. Extract E1_1 Parts 먼저 실행하거나 kidman_room.prefab 빌드 후 재시도.",
                    "확인");
                return;
            }

            BuildFromTiles();
        }

        // ---- Path A: Monolithic room prefab (preferred) ----

        private static void BuildFromMonolithic(GameObject roomPrefab)
        {
            var room = GameObject.Find("Room");
            if (room == null) room = new GameObject("Room");

            var existing = new List<Transform>();
            foreach (Transform t in room.transform) existing.Add(t);
            foreach (var t in existing) Object.DestroyImmediate(t.gameObject);

            room.transform.position = Vector3.zero;
            room.transform.rotation = Quaternion.identity;
            room.transform.localScale = Vector3.one;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(roomPrefab);
            instance.name = "Visual";
            instance.transform.SetParent(room.transform, false);
            instance.transform.localPosition = Vector3.zero;
            // kidman_room source is Z-up; rotate -90 X to convert to Unity Y-up.
            instance.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            instance.transform.localScale = Vector3.one;

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Debug.Log($"[Phase2 RoomBuilder] Installed monolithic '{roomPrefab.name}' under Room.");
            EditorUtility.DisplayDialog("Treatment Room Built",
                $"'{roomPrefab.name}' 단일 prefab 사용.\n\nRoom/Visual = kidman_room (-90° X 회전 적용).\n\n다음: Phase 2 > 3. Align Scene to Room 으로 환자/트레이/펌프/샤프스를 침대 근처로 자동 배치.",
                "확인");
        }

        // ---- Path B: Tile from E1_Parts (fallback) ----

        private const float HalfRoom = 3.5f;

        private static void BuildFromTiles()
        {
            var cornerPrefab   = Load("Wall1");
            var straightPrefab = Load("Wall3");
            var doorWallPrefab = Load("DoorWall");
            var windowPrefab   = Load("WindowWall");
            var doorPrefab     = Load("Door");
            var floorPrefab    = Load("Ground");

            if (straightPrefab == null || floorPrefab == null)
            {
                EditorUtility.DisplayDialog("Build Treatment Room",
                    "필수 부품(Wall3, Ground)이 E1_Parts/에 없습니다.\nPhase 2 > 4. Extract E1_1 Parts 먼저 실행하세요.",
                    "확인");
                return;
            }

            var room = GameObject.Find("Room");
            if (room == null) room = new GameObject("Room");
            room.transform.position = Vector3.zero;
            room.transform.localScale = Vector3.one;

            var existing = new List<Transform>();
            foreach (Transform t in room.transform) existing.Add(t);
            foreach (var t in existing) Object.DestroyImmediate(t.gameObject);

            const float wallTarget = 2.5f;
            const float cornerTarget = 1.5f;
            const float doorTarget = 2.0f;
            const float midSpan = 1.4f;
            int wallCount = 0;

            if (cornerPrefab != null)
            {
                Tile(cornerPrefab, room.transform, new Vector3(+HalfRoom, +HalfRoom, 0f), Quaternion.Euler(0f, 0f, 180f), "Corner_NE", cornerTarget);
                Tile(cornerPrefab, room.transform, new Vector3(-HalfRoom, +HalfRoom, 0f), Quaternion.Euler(0f, 0f, 270f), "Corner_NW", cornerTarget);
                Tile(cornerPrefab, room.transform, new Vector3(-HalfRoom, -HalfRoom, 0f), Quaternion.Euler(0f, 0f, 0f),   "Corner_SW", cornerTarget);
                Tile(cornerPrefab, room.transform, new Vector3(+HalfRoom, -HalfRoom, 0f), Quaternion.Euler(0f, 0f, 90f),  "Corner_SE", cornerTarget);
                wallCount += 4;
            }

            Tile(straightPrefab,  room.transform, new Vector3(-midSpan, -HalfRoom, 0f), Quaternion.Euler(0f, 0f, 90f),  "Wall_Front_L", wallTarget);
            Tile(doorWallPrefab ?? straightPrefab, room.transform, new Vector3(0f, -HalfRoom, 0f), Quaternion.Euler(0f, 0f, 90f),  "Wall_Front_C", wallTarget);
            Tile(straightPrefab,  room.transform, new Vector3(+midSpan, -HalfRoom, 0f), Quaternion.Euler(0f, 0f, 90f),  "Wall_Front_R", wallTarget);
            Tile(straightPrefab,  room.transform, new Vector3(-midSpan, +HalfRoom, 0f), Quaternion.Euler(0f, 0f, -90f), "Wall_Back_L", wallTarget);
            Tile(windowPrefab ?? straightPrefab,   room.transform, new Vector3(0f, +HalfRoom, 0f), Quaternion.Euler(0f, 0f, -90f), "Wall_Back_C", wallTarget);
            Tile(straightPrefab,  room.transform, new Vector3(+midSpan, +HalfRoom, 0f), Quaternion.Euler(0f, 0f, -90f), "Wall_Back_R", wallTarget);
            Tile(straightPrefab,  room.transform, new Vector3(+HalfRoom, -midSpan, 0f), Quaternion.identity,            "Wall_Right_S", wallTarget);
            Tile(straightPrefab,  room.transform, new Vector3(+HalfRoom, 0f,        0f), Quaternion.identity,            "Wall_Right_C", wallTarget);
            Tile(straightPrefab,  room.transform, new Vector3(+HalfRoom, +midSpan, 0f), Quaternion.identity,            "Wall_Right_N", wallTarget);
            Tile(straightPrefab,  room.transform, new Vector3(-HalfRoom, -midSpan, 0f), Quaternion.Euler(0f, 0f, 180f), "Wall_Left_S", wallTarget);
            Tile(straightPrefab,  room.transform, new Vector3(-HalfRoom, 0f,        0f), Quaternion.Euler(0f, 0f, 180f), "Wall_Left_C", wallTarget);
            Tile(straightPrefab,  room.transform, new Vector3(-HalfRoom, +midSpan, 0f), Quaternion.Euler(0f, 0f, 180f), "Wall_Left_N", wallTarget);
            wallCount += 12;

            int floorCount = 0;
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Tile(floorPrefab, room.transform, new Vector3(x * 2.5f, y * 2.5f, 0f), Quaternion.identity, $"Floor_{x+1}{y+1}", wallTarget);
                    floorCount++;
                }
            }

            if (doorPrefab != null)
            {
                Tile(doorPrefab, room.transform, new Vector3(0f, -HalfRoom + 0.05f, 0f), Quaternion.Euler(0f, 0f, 90f), "Door", doorTarget);
            }

            room.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());

            Debug.Log($"[Phase2 RoomBuilder] Tiled {wallCount} walls + {floorCount} floor tiles + 1 door in 7×7m room.");
            EditorUtility.DisplayDialog("Treatment Room Built (Tiled)",
                $"kidman_room.prefab이 없어 E1_Parts tiling 사용.\n· 벽 {wallCount}개\n· 바닥 {floorCount}개\n· 문 1개\n\n향후 kidman_room.prefab을 빌드하면 자동으로 그 prefab을 우선 사용.",
                "확인");
        }

        private static GameObject Load(string name)
        {
            return AssetDatabase.LoadAssetAtPath<GameObject>($"{PartsDir}/{name}.prefab");
        }

        private static void Tile(GameObject prefab, Transform parent, Vector3 localPos, Quaternion localRot, string name, float targetSize)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = name;
            instance.transform.SetParent(parent, false);
            instance.transform.localPosition = localPos;
            instance.transform.localRotation = localRot;

            var rend = instance.GetComponentInChildren<Renderer>();
            if (rend != null)
            {
                var size = rend.bounds.size;
                float maxDim = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
                if (maxDim > 0.001f)
                {
                    float factor = targetSize / maxDim;
                    instance.transform.localScale *= factor;
                }
            }
        }
    }
}
