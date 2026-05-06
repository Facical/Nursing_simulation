using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace NursingSim.Core.Interaction
{
    [DisallowMultipleComponent]
    public class InteractionManager : MonoBehaviour
    {
        [SerializeField] private Camera sourceCamera;
        [SerializeField] private LayerMask interactableMask = ~0;
        [SerializeField] private float maxDistance = 50f;

        private IInteractable currentHover;

        private void Awake()
        {
            if (!sourceCamera) sourceCamera = Camera.main;
        }

        private void Update()
        {
            if (!sourceCamera) return;
            if (Mouse.current == null) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) {
                ClearHover();
                return;
            }

            var mousePos = Mouse.current.position.ReadValue();
            var ray = sourceCamera.ScreenPointToRay(mousePos);
            IInteractable hit = null;
            if (Physics.Raycast(ray, out var info, maxDistance, interactableMask, QueryTriggerInteraction.Collide)) {
                hit = info.collider.GetComponentInParent<IInteractable>();
            }

            if (hit != currentHover) {
                currentHover?.OnHoverExit();
                hit?.OnHoverEnter();
                currentHover = hit;
            }

            if (currentHover != null && Mouse.current.leftButton.wasPressedThisFrame) {
                currentHover.OnClick();
            }
        }

        private void OnDisable()
        {
            ClearHover();
        }

        private void ClearHover()
        {
            if (currentHover != null) {
                currentHover.OnHoverExit();
                currentHover = null;
            }
        }
    }
}
