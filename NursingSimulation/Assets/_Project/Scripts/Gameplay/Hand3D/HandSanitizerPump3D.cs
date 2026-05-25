using UnityEngine;
using UnityEngine.InputSystem;

namespace NursingSim.Gameplay.Hand3D
{
    /// <summary>
    /// Detects "press" actions when the hand is close to the pump and the player clicks LMB.
    /// Reuses PumpInteractable.AnyPumped so the existing UI ToolInteractionStepController and
    /// the new 3D variant both work.
    /// </summary>
    public class HandSanitizerPump3D : MonoBehaviour
    {
        [SerializeField] private Transform plungerVisual;
        [SerializeField] private float plungerCompressDist = 0.02f;
        [SerializeField] private float plungerReturnSpeed = 0.5f;
        [SerializeField] private float pressTriggerRadius = 0.15f;
        [SerializeField] private float pressCooldownSec = 0.4f;
        [SerializeField] private bool allowDirectClick = true;
        [SerializeField] private Camera sourceCamera;
        [SerializeField] private LayerMask directClickMask = ~0;
        [SerializeField] private float directClickDistance = 6f;

        private Vector3 plungerHomeLocalPos;
        private float plungerOffset;
        private float lastPressTime = -10f;
        private Hand3DController hand;

        private void Awake()
        {
            if (plungerVisual == null) plungerVisual = FindChildRecursive(transform, "top");
            if (sourceCamera == null) sourceCamera = Camera.main;
            CachePlungerHome();
        }

        private void Update()
        {
            ReturnPlunger();

            if (Mouse.current == null) return;
            if (!Mouse.current.leftButton.wasPressedThisFrame) return;
            if (TryPress(requireHandInRange: true, alsoRaiseLegacyPump: true)) return;
            if (allowDirectClick && IsPointerOverThisPump())
            {
                TryPress(requireHandInRange: false, alsoRaiseLegacyPump: true);
            }
        }

        public bool PressFromDirectClick()
        {
            if (!allowDirectClick) return false;
            return TryPress(requireHandInRange: false, alsoRaiseLegacyPump: false);
        }

        private bool TryPress(bool requireHandInRange, bool alsoRaiseLegacyPump)
        {
            hand = Hand3DController.ActiveInputHand;
            if (hand == null) hand = Object.FindFirstObjectByType<Hand3DController>();
            if (requireHandInRange && hand == null) return false;
            if (Time.time - lastPressTime < pressCooldownSec) return false;
            if (requireHandInRange && Vector3.Distance(hand.Palm.position, PressPoint) > pressTriggerRadius) return false;

            lastPressTime = Time.time;
            plungerOffset = plungerCompressDist;
            HandActionEvents.RaisePumpPressed(transform, PressPoint);
            if (alsoRaiseLegacyPump) PumpInteractable.RaisePumpFromExternal();
            return true;
        }

        private bool IsPointerOverThisPump()
        {
            if (sourceCamera == null) sourceCamera = Camera.main;
            if (sourceCamera == null || Mouse.current == null) return false;

            var ray = sourceCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            var hits = Physics.RaycastAll(ray, directClickDistance, directClickMask, QueryTriggerInteraction.Collide);
            foreach (var hit in hits)
            {
                if (hit.collider != null && hit.collider.transform.IsChildOf(transform)) return true;
            }

            return false;
        }

        private void ReturnPlunger()
        {
            if (plungerOffset <= 0f || plungerVisual == null) return;
            plungerOffset = Mathf.Max(0f, plungerOffset - plungerReturnSpeed * Time.deltaTime);
            plungerVisual.localPosition = plungerHomeLocalPos - Vector3.up * plungerOffset;
        }

        private Vector3 PressPoint => plungerVisual != null ? plungerVisual.position : transform.position;

        private void CachePlungerHome()
        {
            if (plungerVisual != null) plungerHomeLocalPos = plungerVisual.localPosition;
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                if (child.name == childName) return child;
            }

            return null;
        }
    }
}
