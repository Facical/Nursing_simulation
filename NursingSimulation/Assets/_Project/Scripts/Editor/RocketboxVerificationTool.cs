using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NursingSim.EditorTools
{
    public static class RocketboxVerificationTool
    {
        private const string RocketboxRoot = "Assets/ThirdParty/Rocketbox";
        private const string SimScenePath = "Assets/_Project/Scenes/Simulation_IMInjection.unity";
        private const string TestAvatarFbx =
            "Assets/ThirdParty/Rocketbox/Professions/Medical_Male_01/Export/Medical_Male_01.fbx";

        [MenuItem("Tools/Nursing Sim/Rocketbox/1. Set Rigs to Humanoid")]
        public static void SetRigsToHumanoid()
        {
            int count = 0;
            foreach (var fbx in FindRocketboxBodyFbx())
            {
                var importer = AssetImporter.GetAtPath(fbx) as ModelImporter;
                if (importer == null) continue;
                if (importer.animationType != ModelImporterAnimationType.Human)
                {
                    importer.animationType = ModelImporterAnimationType.Human;
                    importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                    importer.SaveAndReimport();
                }
                count++;
                Debug.Log($"[Rocketbox] Humanoid rig set: {fbx}");
            }
            EditorUtility.DisplayDialog("Rocketbox", $"Humanoid rig 변환 완료: {count}개 FBX", "확인");
        }

        [MenuItem("Tools/Nursing Sim/Rocketbox/2. Verify and Upgrade Materials to URP")]
        public static void UpgradeMaterialsToURP()
        {
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null)
            {
                EditorUtility.DisplayDialog("Rocketbox", "URP/Lit 셰이더를 찾을 수 없습니다. URP 설치 확인 필요.", "확인");
                return;
            }

            int upgraded = 0, alreadyUrp = 0;
            var guids = AssetDatabase.FindAssets("t:Material", new[] { RocketboxRoot });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (mat == null) continue;
                if (mat.shader != null && mat.shader.name.StartsWith("Universal Render Pipeline/"))
                {
                    alreadyUrp++;
                    continue;
                }
                if (TryUpgradeMaterial(mat, urpLit)) upgraded++;
            }

            // FBX에 내장된 sub-asset 재질도 검사
            foreach (var fbx in FindRocketboxBodyFbx())
            {
                var subAssets = AssetDatabase.LoadAllAssetsAtPath(fbx);
                foreach (var asset in subAssets)
                {
                    var mat = asset as Material;
                    if (mat == null) continue;
                    if (mat.shader != null && mat.shader.name.StartsWith("Universal Render Pipeline/"))
                    {
                        alreadyUrp++;
                        continue;
                    }
                    if (TryUpgradeMaterial(mat, urpLit)) upgraded++;
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog(
                "Rocketbox",
                $"재질 검사 완료\n- URP로 업그레이드: {upgraded}개\n- 이미 URP: {alreadyUrp}개",
                "확인");
        }

        [MenuItem("Tools/Nursing Sim/Rocketbox/3. Place Test Avatar in Simulation Scene")]
        public static void PlaceTestAvatar()
        {
            var fbx = AssetDatabase.LoadAssetAtPath<GameObject>(TestAvatarFbx);
            if (fbx == null)
            {
                EditorUtility.DisplayDialog("Rocketbox", $"FBX 없음: {TestAvatarFbx}", "확인");
                return;
            }

            var scene = EditorSceneManager.OpenScene(SimScenePath, OpenSceneMode.Single);

            var existing = GameObject.Find("Rocketbox_TestAvatar");
            if (existing != null) Object.DestroyImmediate(existing);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(fbx);
            instance.name = "Rocketbox_TestAvatar";
            instance.transform.position = new Vector3(0.8f, 0f, 0.5f);
            instance.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);

            Selection.activeGameObject = instance;
            SceneView.FrameLastActiveSceneView();

            EditorUtility.DisplayDialog(
                "Rocketbox",
                "Medical_Male_01 배치 완료.\nScene 뷰에서 환자 왼쪽에 서 있습니다.\n분홍색 아니면 URP 재질 OK, 회색이면 Step 2 재실행.",
                "확인");
        }

        [MenuItem("Tools/Nursing Sim/Rocketbox/Run All Verification (1+2+3)")]
        public static void RunAll()
        {
            SetRigsToHumanoid();
            UpgradeMaterialsToURP();
            PlaceTestAvatar();
        }

        [MenuItem("Tools/Nursing Sim/Rocketbox/Remove Test Avatar (cleanup)")]
        public static void RemoveTestAvatar()
        {
            var scene = EditorSceneManager.OpenScene(SimScenePath, OpenSceneMode.Single);
            var existing = GameObject.Find("Rocketbox_TestAvatar");
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                Debug.Log("[Rocketbox] Test avatar removed from Simulation_IMInjection.");
                EditorUtility.DisplayDialog("Rocketbox", "Rocketbox_TestAvatar 제거 + 씬 저장 완료.", "확인");
            }
            else
            {
                EditorUtility.DisplayDialog("Rocketbox", "Rocketbox_TestAvatar 오브젝트가 씬에 없음.", "확인");
            }
        }

        private static IEnumerable<string> FindRocketboxBodyFbx()
        {
            var guids = AssetDatabase.FindAssets("t:Model", new[] { RocketboxRoot });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase)) continue;
                var file = Path.GetFileNameWithoutExtension(path);
                if (file.EndsWith("_facial", System.StringComparison.OrdinalIgnoreCase)) continue;
                yield return path;
            }
        }

        private static bool TryUpgradeMaterial(Material mat, Shader urpLit)
        {
            Texture mainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
            Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;
            Texture bumpMap = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;

            mat.shader = urpLit;
            if (mat.HasProperty("_BaseMap") && mainTex != null) mat.SetTexture("_BaseMap", mainTex);
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
            if (mat.HasProperty("_BumpMap") && bumpMap != null) mat.SetTexture("_BumpMap", bumpMap);

            EditorUtility.SetDirty(mat);
            return true;
        }
    }
}
