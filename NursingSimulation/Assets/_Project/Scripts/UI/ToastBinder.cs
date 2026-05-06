using System.Collections;
using NursingSim.Core.Events;
using NursingSim.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NursingSim.UI
{
    public class ToastBinder : MonoBehaviour
    {
        [SerializeField] private FeedbackBus bus;
        [SerializeField] private CanvasGroup group;
        [SerializeField] private TMP_Text messageLabel;
        [SerializeField] private Image background;
        [SerializeField] private float showSeconds = 3f;
        [SerializeField] private float fadeSeconds = 0.25f;

        private Coroutine running;

        private void Awake()
        {
            if (group) group.alpha = 0f;
        }

        private void OnEnable()
        {
            if (bus && bus.instantFeedback) bus.instantFeedback.OnRaised += OnPayload;
        }

        private void OnDisable()
        {
            if (bus && bus.instantFeedback) bus.instantFeedback.OnRaised -= OnPayload;
        }

        private void OnPayload(InstantFeedbackPayload payload)
        {
            if (messageLabel) messageLabel.text = payload.message;
            if (background) background.color = ColorFor(payload.kind);
            if (running != null) StopCoroutine(running);
            running = StartCoroutine(FadeRoutine());
        }

        private Color ColorFor(FeedbackKind kind)
        {
            switch (kind) {
                case FeedbackKind.Warning: return new Color(0.95f, 0.55f, 0.1f, 0.9f);
                case FeedbackKind.Error:   return new Color(0.85f, 0.2f, 0.2f, 0.9f);
                case FeedbackKind.Success: return new Color(0.2f, 0.75f, 0.3f, 0.9f);
                default: return new Color(0.2f, 0.35f, 0.55f, 0.9f);
            }
        }

        private IEnumerator FadeRoutine()
        {
            if (group) group.alpha = 0f;
            float t = 0f;
            while (t < fadeSeconds) {
                t += Time.unscaledDeltaTime;
                if (group) group.alpha = Mathf.Clamp01(t / fadeSeconds);
                yield return null;
            }
            yield return new WaitForSecondsRealtime(showSeconds);
            t = 0f;
            while (t < fadeSeconds) {
                t += Time.unscaledDeltaTime;
                if (group) group.alpha = 1f - Mathf.Clamp01(t / fadeSeconds);
                yield return null;
            }
            if (group) group.alpha = 0f;
            running = null;
        }
    }
}
