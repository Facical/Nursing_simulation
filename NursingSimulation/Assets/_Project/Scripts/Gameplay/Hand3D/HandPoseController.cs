using UnityEngine;

namespace NursingSim.Gameplay.Hand3D
{
    [DisallowMultipleComponent]
    public class HandPoseController : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string grabbedBool = "IsGrabbing";
        [SerializeField] private string syringeBool = "IsHoldingSyringe";
        [SerializeField] private string rubbingBool = "IsRubbing";
        [SerializeField] private string pumpTrigger = "PumpPress";

        private bool isGrabbing;
        private bool isHoldingSyringe;
        private float rubbingUntil;

        private void Awake()
        {
            if (animator == null) animator = GetComponentInChildren<Animator>();
            ApplyBools();
        }

        private void OnEnable()
        {
            HandActionEvents.Grabbed += OnGrabbed;
            HandActionEvents.Released += OnReleased;
            HandActionEvents.PumpPressed += OnPumpPressed;
            HandActionEvents.Rubbed += OnRubbed;
        }

        private void OnDisable()
        {
            HandActionEvents.Grabbed -= OnGrabbed;
            HandActionEvents.Released -= OnReleased;
            HandActionEvents.PumpPressed -= OnPumpPressed;
            HandActionEvents.Rubbed -= OnRubbed;
        }

        private void OnGrabbed(HandToolPayload payload)
        {
            isGrabbing = true;
            isHoldingSyringe = payload.Kind == ToolKind.Syringe;
            ApplyBools();
        }

        private void OnReleased(HandToolPayload payload)
        {
            isGrabbing = false;
            isHoldingSyringe = false;
            ApplyBools();
        }

        private void OnPumpPressed(HandSimplePayload payload)
        {
            SetTrigger(pumpTrigger);
        }

        private void OnRubbed(float deltaSec)
        {
            rubbingUntil = Time.time + Mathf.Max(0.15f, deltaSec);
            SetBool(rubbingBool, true);
        }

        private void Update()
        {
            if (rubbingUntil > 0f && Time.time > rubbingUntil)
            {
                rubbingUntil = 0f;
                SetBool(rubbingBool, false);
            }
        }

        private void ApplyBools()
        {
            SetBool(grabbedBool, isGrabbing);
            SetBool(syringeBool, isHoldingSyringe);
        }

        private void SetBool(string parameter, bool value)
        {
            if (animator == null || string.IsNullOrEmpty(parameter)) return;
            if (!HasParameter(parameter, AnimatorControllerParameterType.Bool)) return;
            animator.SetBool(parameter, value);
        }

        private void SetTrigger(string parameter)
        {
            if (animator == null || string.IsNullOrEmpty(parameter)) return;
            if (!HasParameter(parameter, AnimatorControllerParameterType.Trigger)) return;
            animator.SetTrigger(parameter);
        }

        private bool HasParameter(string parameter, AnimatorControllerParameterType type)
        {
            foreach (var p in animator.parameters)
            {
                if (p.name == parameter && p.type == type) return true;
            }

            return false;
        }
    }
}
