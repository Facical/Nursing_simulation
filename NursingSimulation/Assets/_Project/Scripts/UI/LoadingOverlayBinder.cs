using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NursingSim.UI
{
    public class LoadingOverlayBinder : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private RectTransform spinner;
        [SerializeField] private TMP_Text messageLabel;
        [SerializeField] private TMP_Text progressLabel;
        [SerializeField] private Slider progressBar;
        [SerializeField] private float spinnerDegPerSec = 360f;
        [SerializeField] private float fillDurationSec = 1.5f;
        [SerializeField] private float fadeOutSec = 0.15f;
        [SerializeField] private string activationMessage = "씬 준비 중…";

        private const string PersistentCanvasName = "PersistentLoadingCanvas";

        private bool spinning;
        private CanvasGroup rootCanvasGroup;

        private void Awake()
        {
            if (root) {
                root.SetActive(false);
                rootCanvasGroup = root.GetComponent<CanvasGroup>();
            }
        }

        private void Update()
        {
            if (spinning && spinner) {
                spinner.localEulerAngles += new Vector3(0f, 0f, -spinnerDegPerSec * Time.unscaledDeltaTime);
            }
        }

        public void BeginLoad(string sceneName, string message = "불러오는 중…")
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (root) root.SetActive(true);
            if (rootCanvasGroup) rootCanvasGroup.alpha = 1f;
            if (messageLabel) messageLabel.text = message;
            SetProgress(0f);
            spinning = true;
            BecomePersistent();
            Debug.Log($"[LoadingOverlay] BeginLoad scene='{sceneName}' fillDuration={fillDurationSec}s");
            StartCoroutine(LoadRoutine(sceneName));
        }

        // 씬 전환 후에도 오버레이를 유지하기 위해 DontDestroyOnLoad 캔버스로 옮긴다.
        // 이 트릭이 핵심: activation 메인 스레드 블록이 끝난 직후 새 씬에서 fade-out으로 사라져
        // 사용자 눈에는 "100% 도달 → 자연스러운 페이드" 흐름이 된다 (다른 게임 패턴).
        private void BecomePersistent()
        {
            var current = transform.parent;
            if (current != null && current.name == PersistentCanvasName) return;

            var canvasGo = new GameObject(PersistentCanvasName,
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32700; // 항상 최상단
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            DontDestroyOnLoad(canvasGo);

            transform.SetParent(canvasGo.transform, false);
            if (transform is RectTransform rt) {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale = Vector3.one;
            }
        }

        private IEnumerator LoadRoutine(string sceneName)
        {
            float totalStart = Time.realtimeSinceStartup;
            var op = SceneManager.LoadSceneAsync(sceneName);
            if (op == null) {
                Debug.LogError($"[LoadingOverlay] LoadSceneAsync returned null for '{sceneName}'.");
                Cleanup();
                yield break;
            }
            op.allowSceneActivation = false;

            // Phase 1: 시간 기반 가짜 진행률. op.progress(백그라운드 로드)와 분리.
            float elapsed = 0f;
            float duration = Mathf.Max(0.1f, fillDurationSec);
            while (true) {
                elapsed += Time.unscaledDeltaTime;
                bool loadReady = op.progress >= 0.89f;
                float timeT = Mathf.Clamp01(elapsed / duration);
                float cap = loadReady ? 1f : 0.99f;
                float displayed = Mathf.Min(timeT, cap);
                SetProgress(displayed);
                if (loadReady && timeT >= 1f) break;
                yield return null;
            }
            SetProgress(1f);

            // Phase 2: 메시지 변경 후 한 프레임 표시 — "100%에서 멈춤"이 아니라 "씬 준비 중…"으로 인지.
            if (messageLabel && !string.IsNullOrEmpty(activationMessage)) {
                messageLabel.text = activationMessage;
            }
            yield return null;

            // Phase 3: activation 트리거. 메인 스레드 블록 동안 Update가 멈추므로 스피너도 정지.
            float activationStart = Time.realtimeSinceStartup;
            op.allowSceneActivation = true;
            while (!op.isDone) yield return null;
            float activationSec = Time.realtimeSinceStartup - activationStart;
            float totalSec = Time.realtimeSinceStartup - totalStart;
            Debug.Log($"[LoadingOverlay] Activation took {activationSec:F2}s (total {totalSec:F2}s) — Awake/OnEnable 부하 지표");

            // Phase 4: 새 씬에서 fade-out (DontDestroyOnLoad 덕분에 이 코루틴이 새 씬에서 재개됨).
            yield return FadeOutRoutine();
            Cleanup();
        }

        private IEnumerator FadeOutRoutine()
        {
            // 스피너는 fade-out 동안에도 계속 회전 (활동감 유지).
            if (rootCanvasGroup == null) {
                spinning = false;
                yield break;
            }
            float elapsed = 0f;
            float duration = Mathf.Max(0.05f, fadeOutSec);
            while (elapsed < duration) {
                elapsed += Time.unscaledDeltaTime;
                rootCanvasGroup.alpha = Mathf.Clamp01(1f - (elapsed / duration));
                yield return null;
            }
            rootCanvasGroup.alpha = 0f;
            spinning = false;
        }

        private void Cleanup()
        {
            // BecomePersistent 후라면 parent는 PersistentLoadingCanvas. 그 캔버스 통째 Destroy.
            // BeginLoad 호출 전이라면 parent는 기존 Canvas_MainMenu → 그 경우 Cleanup 호출되지 않음.
            var parent = transform.parent;
            if (parent != null && parent.name == PersistentCanvasName) {
                Destroy(parent.gameObject);
            } else {
                Destroy(gameObject);
            }
        }

        private void SetProgress(float t)
        {
            if (progressBar) progressBar.value = t;
            if (progressLabel) progressLabel.text = $"{Mathf.RoundToInt(t * 100f)}%";
        }
    }
}
