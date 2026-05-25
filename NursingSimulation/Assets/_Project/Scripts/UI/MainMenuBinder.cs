using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NursingSim.UI
{
    public class MainMenuBinder : MonoBehaviour
    {
        [SerializeField] private Button scenarioCardButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button historyButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private SettingsModalBinder settingsModal;
        [SerializeField] private RecentHistoryModalBinder historyModal;
        [SerializeField] private LoadingOverlayBinder loadingOverlay;
        [SerializeField] private string targetSceneName = "Simulation_IMInjection";

        private const string VolumeKey = "settings.masterVolume";

        private void Awake()
        {
            // 시작 시 PlayerPrefs의 마스터 볼륨을 AudioListener에 적용.
            AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f));

            if (scenarioCardButton) scenarioCardButton.onClick.AddListener(StartScenario);
            if (settingsButton) settingsButton.onClick.AddListener(() => { if (settingsModal) settingsModal.Show(); });
            if (historyButton) historyButton.onClick.AddListener(() => { if (historyModal) historyModal.Show(); });
            if (quitButton) quitButton.onClick.AddListener(QuitApp);
        }

        private void StartScenario()
        {
            if (string.IsNullOrEmpty(targetSceneName)) return;
            if (scenarioCardButton) scenarioCardButton.interactable = false;
            if (loadingOverlay) {
                loadingOverlay.BeginLoad(targetSceneName);
            } else {
                SceneManager.LoadScene(targetSceneName);
            }
        }

        private void QuitApp()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
