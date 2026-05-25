using UnityEngine;
using UnityEngine.UI;

namespace NursingSim.UI
{
    public class SettingsModalBinder : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private Toggle subtitlesToggle;
        [SerializeField] private Toggle fontSmallToggle;
        [SerializeField] private Toggle fontMediumToggle;
        [SerializeField] private Toggle fontLargeToggle;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Button closeButton;

        private const string SubtitleKey = "settings.subtitles";
        private const string FontScaleKey = "settings.fontScale";
        private const string VolumeKey = "settings.masterVolume";

        private void Awake()
        {
            if (root) root.SetActive(false);
            LoadFromPrefs();
            if (subtitlesToggle) subtitlesToggle.onValueChanged.AddListener(OnSubtitlesChanged);
            if (fontSmallToggle) fontSmallToggle.onValueChanged.AddListener(v => { if (v) WriteFontScale(0); });
            if (fontMediumToggle) fontMediumToggle.onValueChanged.AddListener(v => { if (v) WriteFontScale(1); });
            if (fontLargeToggle) fontLargeToggle.onValueChanged.AddListener(v => { if (v) WriteFontScale(2); });
            if (volumeSlider) volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            if (closeButton) closeButton.onClick.AddListener(Hide);
        }

        public void Show()
        {
            LoadFromPrefs();
            if (root) root.SetActive(true);
        }

        public void Hide()
        {
            if (root) root.SetActive(false);
        }

        private void LoadFromPrefs()
        {
            if (subtitlesToggle) subtitlesToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt(SubtitleKey, 1) == 1);
            int scale = Mathf.Clamp(PlayerPrefs.GetInt(FontScaleKey, 1), 0, 2);
            if (fontSmallToggle) fontSmallToggle.SetIsOnWithoutNotify(scale == 0);
            if (fontMediumToggle) fontMediumToggle.SetIsOnWithoutNotify(scale == 1);
            if (fontLargeToggle) fontLargeToggle.SetIsOnWithoutNotify(scale == 2);
            if (volumeSlider) volumeSlider.SetValueWithoutNotify(Mathf.Clamp01(PlayerPrefs.GetFloat(VolumeKey, 1f)));
        }

        private void OnSubtitlesChanged(bool on)
        {
            PlayerPrefs.SetInt(SubtitleKey, on ? 1 : 0);
            PlayerPrefs.Save();
        }

        private void WriteFontScale(int scale)
        {
            PlayerPrefs.SetInt(FontScaleKey, scale);
            PlayerPrefs.Save();
        }

        private void OnVolumeChanged(float v)
        {
            float clamped = Mathf.Clamp01(v);
            AudioListener.volume = clamped;
            PlayerPrefs.SetFloat(VolumeKey, clamped);
            PlayerPrefs.Save();
        }
    }
}
