using System.Collections.Generic;
using System.IO;
using NursingSim.Core.Runner;
using NursingSim.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NursingSim.EditorTools
{
    public static class Phase3MainMenuTool
    {
        private const string MainMenuScenePath = "Assets/_Project/Scenes/MainMenu.unity";
        private const string SimulationScenePath = "Assets/_Project/Scenes/Simulation_IMInjection.unity";

        [MenuItem("Tools/Nursing Sim/Phase 3/0. Wire MainMenu Scene")]
        public static void WireMainMenu()
        {
            EnsureSceneFile(MainMenuScenePath);
            EnsureBuildSettingsRegistration();

            var scene = EditorSceneManager.OpenScene(MainMenuScenePath, OpenSceneMode.Single);

            // 기존 루트 cleanup (idempotent).
            CleanupExisting<EventSystem>();
            CleanupExisting<Canvas>();
            CleanupExisting<MainMenuBinder>();
            CleanupExisting<SettingsModalBinder>();
            CleanupExisting<RecentHistoryModalBinder>();
            CleanupExisting<LoadingOverlayBinder>();
            CleanupExisting<SaveService>();

            new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));

            var saveGo = new GameObject("SaveService");
            var save = saveGo.AddComponent<SaveService>();

            var canvas = CreateCanvas("Canvas_MainMenu");

            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/SDF_Regular.asset");
            var bold = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Art/Fonts/SDF_Bold.asset") ?? font;

            var layoutGo = UiBuilder.CreateMainMenuLayout(canvas.transform, font, bold);
            var settingsGo = UiBuilder.CreateSettingsModal(canvas.transform, font, bold);
            var historyGo = UiBuilder.CreateRecentHistoryModal(canvas.transform, font, bold);
            var loadingGo = UiBuilder.CreateLoadingOverlay(canvas.transform, font, bold);
            var rowPrefab = UiBuilder.EnsureHistoryRowPrefab(font, bold);

            var settingsBinder = settingsGo.AddComponent<SettingsModalBinder>();
            UiBuilder.BindSettingsModal(settingsBinder, settingsGo);

            var historyBinder = historyGo.AddComponent<RecentHistoryModalBinder>();
            UiBuilder.BindRecentHistoryModal(historyBinder, historyGo, rowPrefab, save);

            var loadingBinder = loadingGo.AddComponent<LoadingOverlayBinder>();
            UiBuilder.BindLoadingOverlay(loadingBinder, loadingGo);

            var menuBinder = layoutGo.AddComponent<MainMenuBinder>();
            UiBuilder.BindMainMenu(menuBinder, layoutGo, settingsGo, historyGo, loadingGo);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[Phase3 MainMenu] MainMenu scene wired (title + scenario card + 3 buttons + version + settings/history modals).");
            EditorUtility.DisplayDialog("Phase 3 — MainMenu Wiring",
                "MainMenu.unity에 진입 화면이 배치되었습니다.\n시나리오 카드 클릭 → Simulation_IMInjection 씬으로 직행합니다.",
                "확인");
        }

        private static Canvas CreateCanvas(string name)
        {
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

        private static void CleanupExisting<T>() where T : Component
        {
            var all = Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var c in all) {
                if (c == null) continue;
                Object.DestroyImmediate(c.gameObject);
            }
        }

        private static void EnsureSceneFile(string path)
        {
            if (File.Exists(path)) return;
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(newScene, path);
        }

        private static void EnsureBuildSettingsRegistration()
        {
            var current = EditorBuildSettings.scenes;
            bool hasMain = false, hasSim = false;
            foreach (var s in current) {
                if (s.path == MainMenuScenePath) hasMain = true;
                if (s.path == SimulationScenePath) hasSim = true;
            }
            if (hasMain && hasSim) return;

            var list = new List<EditorBuildSettingsScene>(current);
            if (!hasMain) list.Insert(0, new EditorBuildSettingsScene(MainMenuScenePath, true));
            if (!hasSim) list.Add(new EditorBuildSettingsScene(SimulationScenePath, true));
            EditorBuildSettings.scenes = list.ToArray();
        }
    }
}
