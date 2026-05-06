using System;
using UnityEngine;

namespace NursingSim.Core.Interaction
{
    [DisallowMultipleComponent]
    public abstract class InteractableBase : MonoBehaviour, IInteractable
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private Renderer highlightRenderer;
        [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0.2f);

        private MaterialPropertyBlock mpb;
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");
        private Color originalColor;
        private bool cachedOriginal;

        public string Id => string.IsNullOrEmpty(id) ? name : id;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? Id : displayName;

        public event Action Clicked;

        protected virtual void Awake()
        {
            if (!highlightRenderer) highlightRenderer = GetComponentInChildren<Renderer>();
            mpb = new MaterialPropertyBlock();
            CacheOriginalColor();
        }

        private void CacheOriginalColor()
        {
            if (!highlightRenderer || cachedOriginal) return;
            var mat = highlightRenderer.sharedMaterial;
            if (mat == null) return;
            if (mat.HasProperty(BaseColorId)) originalColor = mat.GetColor(BaseColorId);
            else if (mat.HasProperty(ColorId)) originalColor = mat.GetColor(ColorId);
            else originalColor = Color.white;
            cachedOriginal = true;
        }

        public virtual void OnHoverEnter()
        {
            ApplyTint(Color.Lerp(originalColor, highlightColor, 0.6f), highlightColor * 0.3f);
        }

        public virtual void OnHoverExit()
        {
            ApplyTint(originalColor, Color.black);
        }

        public virtual void OnClick()
        {
            Clicked?.Invoke();
            HandleClick();
        }

        protected abstract void HandleClick();

        private void ApplyTint(Color baseCol, Color emission)
        {
            if (!highlightRenderer) return;
            highlightRenderer.GetPropertyBlock(mpb);
            var mat = highlightRenderer.sharedMaterial;
            if (mat != null && mat.HasProperty(BaseColorId)) mpb.SetColor(BaseColorId, baseCol);
            else mpb.SetColor(ColorId, baseCol);
            mpb.SetColor(EmissionId, emission);
            highlightRenderer.SetPropertyBlock(mpb);
        }
    }
}
