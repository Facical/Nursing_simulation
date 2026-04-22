using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NursingSim.EditorTools
{
    public static class Phase0SetupTool
    {
        private const string ScenePath = "Assets/_Project/Scenes/Simulation_IMInjection.unity";

        [MenuItem("Tools/Nursing Sim/Setup Phase 0 Placeholders")]
        public static void SetupPlaceholders()
        {
            var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);

            ClearIfExists("Floor");
            ClearIfExists("Patient_Placeholder");
            ClearIfExists("Tray_Placeholder");
            ClearIfExists("Cabinet_Placeholder");

            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            floor.name = "Floor";
            floor.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            floor.transform.localScale = new Vector3(2f, 1f, 2f);

            var patient = new GameObject("Patient_Placeholder");
            patient.transform.position = new Vector3(0f, 0.5f, 0f);
            var patientBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            patientBody.name = "Body";
            patientBody.transform.SetParent(patient.transform, false);

            var tray = new GameObject("Tray_Placeholder");
            tray.transform.position = new Vector3(1.5f, 0.8f, 0f);
            var trayTop = GameObject.CreatePrimitive(PrimitiveType.Cube);
            trayTop.name = "TrayTop";
            trayTop.transform.SetParent(tray.transform, false);
            trayTop.transform.localScale = new Vector3(0.6f, 0.05f, 0.4f);

            var cabinet = new GameObject("Cabinet_Placeholder");
            cabinet.transform.position = new Vector3(-2f, 1f, 0f);
            var cabinetBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cabinetBody.name = "Body";
            cabinetBody.transform.SetParent(cabinet.transform, false);
            cabinetBody.transform.localScale = new Vector3(0.5f, 2f, 0.4f);

            var mainCam = GameObject.Find("Main Camera");
            if (mainCam != null)
            {
                mainCam.transform.position = new Vector3(0f, 1.6f, -3f);
                mainCam.transform.rotation = Quaternion.Euler(10f, 0f, 0f);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("[Phase0] Simulation_IMInjection placeholders created and saved.");
            EditorUtility.DisplayDialog(
                "Phase 0 Setup",
                "Simulation_IMInjection 씬에 처치실 플레이스홀더를 배치했습니다.\nPlay 버튼으로 확인하세요.",
                "확인");
        }

        private static void ClearIfExists(string name)
        {
            var existing = GameObject.Find(name);
            if (existing != null) Object.DestroyImmediate(existing);
        }
    }
}
