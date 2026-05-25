using UnityEngine;

namespace NursingSim.Gameplay.Hand3D
{
    [DisallowMultipleComponent]
    public class HandHygieneAnimator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Hand3DController leftHand;
        [SerializeField] private Hand3DController rightHand;
        [SerializeField] private FingerPoseController leftFingers;
        [SerializeField] private FingerPoseController rightFingers;
        [SerializeField] private Animator leftAnimator;
        [SerializeField] private Animator rightAnimator;
        [SerializeField] private Transform pumpReference;

        [Header("Cleanser Visual")]
        [SerializeField] private bool showCleanserVisual;
        [SerializeField] private Color foamColor = new Color(0.9f, 1f, 1f, 0.72f);
        [SerializeField] private float foamMaxScale = 0.018f;

        [Header("Rub Pose")]
        [SerializeField] private Vector3 rubCenterWorldOffset = new Vector3(0f, 0.12f, -0.22f);
        [SerializeField] private float handSeparation = 0.12f;
        [SerializeField] private float rubTravel = 0.045f;
        [SerializeField] private float rubLift = 0.012f;
        [SerializeField] private float rubFrequency = 3.1f;
        [SerializeField] private float blendSpeed = 7f;
        [SerializeField] private float rubInputHoldSec = 0.22f;

        [Header("Hand Rotation")]
        [SerializeField] private Vector3 leftRubEulerOffset = new Vector3(0f, 0f, 10f);
        [SerializeField] private Vector3 rightRubEulerOffset = new Vector3(0f, 0f, -10f);
        [SerializeField] private float rollAmount = 7f;

        private Quaternion leftBaseRotation;
        private Quaternion rightBaseRotation;
        private Vector3 rubCenter;
        private float blend;
        private float lastRubInputTime = -10f;
        private bool active;
        private bool rubEnabled;
        private float cleanserAmount;
        private bool hasManualRubCenter;
        private Vector3 manualRubCenter;
        private GameObject leftFoamVisual;
        private GameObject rightFoamVisual;
        private Material foamMaterial;
        private bool cameraPoseCached;
        private Vector3 previousCameraPosition;
        private Quaternion previousCameraRotation;

        private static readonly int GripAmountHash = Animator.StringToHash("GripAmount");
        private static readonly int IsRubbingHash = Animator.StringToHash("IsRubbing");
        private static readonly int PumpPressHash = Animator.StringToHash("PumpPress");
        private static readonly int IdleStateHash = Animator.StringToHash("Idle");
        private static readonly int RubLoopStateHash = Animator.StringToHash("RubLoop");
        private static readonly int PumpPressStateHash = Animator.StringToHash("PumpPress");

        public bool IsActive => active;
        public bool IsRubbing => rubEnabled && blend > 0.01f;

        private void Awake()
        {
            ResolveHands();
        }

        private void Update()
        {
            if (!active)
            {
                if (blend > 0f)
                {
                    blend = Mathf.MoveTowards(blend, 0f, Time.deltaTime * blendSpeed);
                    if (blend <= 0f) ClearOverrides();
                }
                return;
            }

            if (leftHand == null || rightHand == null) ResolveHands();
            if (leftHand == null || rightHand == null) return;

            blend = Mathf.MoveTowards(blend, rubEnabled ? 1f : 0f, Time.deltaTime * blendSpeed);
            if (blend <= 0f) return;

            ApplyRubPose();
        }

        private void LateUpdate()
        {
            if (active) ApplyFirstPersonCameraPose();
        }

        public void Begin(Transform pump)
        {
            ResolveHands();
            pumpReference = pump != null ? pump : pumpReference;
            rubCenter = CalculateRubCenter();
            if (leftHand != null) leftBaseRotation = leftHand.HandTarget.rotation;
            if (rightHand != null) rightBaseRotation = rightHand.HandTarget.rotation;
            SetFingerInput(true);
            SetAnimatorRub(false);
            SetAnimatorGrip(0f);
            CrossFadeHands(IdleStateHash, 0.05f);
            ApplyFirstPersonCameraPose();
            lastRubInputTime = -10f;
            rubEnabled = false;
            cleanserAmount = 0f;
            hasManualRubCenter = false;
            UpdateCleanserVisuals();
            active = true;
        }

        public void RegisterPumpPress(Vector3 pressPoint, int currentPumpCount, int requiredPumpCount, bool beginRubWhenReady = true)
        {
            if (!active) Begin(pumpReference);
            TriggerPumpPress();
            cleanserAmount = Mathf.Clamp01(Mathf.Max(cleanserAmount, currentPumpCount / Mathf.Max(1f, requiredPumpCount)));
            UpdateCleanserVisuals();
            if (pressPoint != Vector3.zero) rubCenter = pressPoint + rubCenterWorldOffset;
            if (beginRubWhenReady && currentPumpCount >= requiredPumpCount)
            {
                StartRubPose();
            }
        }

        public void RegisterWaterContact(Vector3 contactPosition)
        {
            if (!active) Begin(pumpReference);
            if (contactPosition != Vector3.zero)
            {
                manualRubCenter = contactPosition + Vector3.up * 0.04f;
                rubCenter = manualRubCenter;
                hasManualRubCenter = true;
            }
            cleanserAmount = Mathf.Max(cleanserAmount, 0.65f);
            UpdateCleanserVisuals();
        }

        public void StartRubPose(Vector3? centerOverride = null)
        {
            rubEnabled = true;
            SetAnimatorRub(true);
            SetAnimatorGrip(0.15f);
            CrossFadeHands(RubLoopStateHash, 0.08f);
            hasManualRubCenter = centerOverride.HasValue;
            if (centerOverride.HasValue) manualRubCenter = centerOverride.Value;
            rubCenter = CalculateRubCenter();
        }

        public void NotifyRubInput(float deltaSec)
        {
            if (deltaSec <= 0f) return;
            if (!active) Begin(pumpReference);
            rubEnabled = true;
            SetAnimatorRub(true);
            lastRubInputTime = Time.time;
        }

        public void End()
        {
            active = false;
            rubEnabled = false;
            SetAnimatorRub(false);
            SetAnimatorGrip(0f);
            CrossFadeHands(IdleStateHash, 0.08f);
            SetFingerInput(true);
            cleanserAmount = 0f;
            hasManualRubCenter = false;
            UpdateCleanserVisuals();
            RestoreCameraPose();
        }

        private void ApplyRubPose()
        {
            var center = CalculateRubCenter();
            rubCenter = Vector3.Lerp(rubCenter, center, Time.deltaTime * 5f);

            float inputWeight = Time.time - lastRubInputTime <= rubInputHoldSec ? 1f : 0.35f;
            float wave = Mathf.Sin(Time.time * rubFrequency * Mathf.PI * 2f) * inputWeight;
            float lift = Mathf.Sin(Time.time * rubFrequency * Mathf.PI * 4f) * rubLift * inputWeight;
            float scrub = wave * rubTravel;

            var leftOffset = new Vector3(-handSeparation * 0.5f, lift, scrub);
            var rightOffset = new Vector3(handSeparation * 0.5f, -lift * 0.5f, -scrub);
            var leftPos = Vector3.Lerp(leftHand.HandTarget.position, rubCenter + leftOffset, blend);
            var rightPos = Vector3.Lerp(rightHand.HandTarget.position, rubCenter + rightOffset, blend);

            float roll = wave * rollAmount;
            var leftRot = leftBaseRotation * Quaternion.Euler(leftRubEulerOffset + new Vector3(0f, 0f, roll));
            var rightRot = rightBaseRotation * Quaternion.Euler(rightRubEulerOffset + new Vector3(0f, 0f, -roll));

            leftHand.SetProceduralPose(leftPos, leftRot);
            rightHand.SetProceduralPose(rightPos, rightRot);
            if (leftFingers != null) leftFingers.SetClinicalPose(0.06f, 0.04f, 0.04f, 0.04f, 0.04f);
            if (rightFingers != null) rightFingers.SetClinicalPose(0.06f, 0.04f, 0.04f, 0.04f, 0.04f);
            SetAnimatorGrip(0.06f);
        }

        private Vector3 CalculateRubCenter()
        {
            if (hasManualRubCenter) return manualRubCenter;
            if (pumpReference != null) return pumpReference.position + rubCenterWorldOffset;
            return transform.position + rubCenterWorldOffset;
        }

        private void ResolveHands()
        {
            var hands = Object.FindObjectsByType<Hand3DController>(FindObjectsSortMode.None);
            foreach (var hand in hands)
            {
                if (hand.Side == HandSide.Left)
                {
                    leftHand = hand;
                    leftFingers = hand.GetComponent<FingerPoseController>();
                    leftAnimator = hand.GetComponent<Animator>();
                }
                else if (hand.Side == HandSide.Right)
                {
                    rightHand = hand;
                    rightFingers = hand.GetComponent<FingerPoseController>();
                    rightAnimator = hand.GetComponent<Animator>();
                }
            }
        }

        private void ClearOverrides()
        {
            if (leftHand != null) leftHand.ClearProceduralPose();
            if (rightHand != null) rightHand.ClearProceduralPose();
            cleanserAmount = 0f;
            hasManualRubCenter = false;
            UpdateCleanserVisuals();
        }

        private void OnDisable()
        {
            ClearOverrides();
            SetFingerInput(true);
            RestoreCameraPose();
        }

        private void SetFingerInput(bool enabled)
        {
            if (leftFingers != null) leftFingers.SetInputEnabled(enabled);
            if (rightFingers != null) rightFingers.SetInputEnabled(enabled);
        }

        private void TriggerPumpPress()
        {
            SetAnimatorTrigger(leftAnimator, PumpPressHash);
            SetAnimatorTrigger(rightAnimator, PumpPressHash);
            CrossFadeHands(PumpPressStateHash, 0.04f);
        }

        private void SetAnimatorRub(bool value)
        {
            SetAnimatorBool(leftAnimator, IsRubbingHash, value);
            SetAnimatorBool(rightAnimator, IsRubbingHash, value);
        }

        private void SetAnimatorGrip(float value)
        {
            SetAnimatorFloat(leftAnimator, GripAmountHash, value);
            SetAnimatorFloat(rightAnimator, GripAmountHash, value);
        }

        private void CrossFadeHands(int stateHash, float transitionSec)
        {
            CrossFadeIfStateExists(leftAnimator, stateHash, transitionSec);
            CrossFadeIfStateExists(rightAnimator, stateHash, transitionSec);
        }

        private static void SetAnimatorTrigger(Animator animator, int parameterHash)
        {
            if (!HasParameter(animator, parameterHash, AnimatorControllerParameterType.Trigger)) return;
            animator.ResetTrigger(parameterHash);
            animator.SetTrigger(parameterHash);
        }

        private static void SetAnimatorBool(Animator animator, int parameterHash, bool value)
        {
            if (!HasParameter(animator, parameterHash, AnimatorControllerParameterType.Bool)) return;
            animator.SetBool(parameterHash, value);
        }

        private static void SetAnimatorFloat(Animator animator, int parameterHash, float value)
        {
            if (!HasParameter(animator, parameterHash, AnimatorControllerParameterType.Float)) return;
            animator.SetFloat(parameterHash, value);
        }

        private static void CrossFadeIfStateExists(Animator animator, int stateHash, float transitionSec)
        {
            if (animator == null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController == null) return;
            if (!animator.HasState(0, stateHash)) return;
            animator.CrossFade(stateHash, transitionSec, 0);
        }

        private static bool HasParameter(Animator animator, int parameterHash, AnimatorControllerParameterType type)
        {
            if (animator == null || animator.runtimeAnimatorController == null) return false;
            foreach (var parameter in animator.parameters)
            {
                if (parameter.nameHash == parameterHash && parameter.type == type) return true;
            }
            return false;
        }

        private void UpdateCleanserVisuals()
        {
            if (!showCleanserVisual)
            {
                ApplyCleanserVisual(leftFoamVisual, 0f);
                ApplyCleanserVisual(rightFoamVisual, 0f);
                return;
            }
            if (cleanserAmount <= 0.01f && leftFoamVisual == null && rightFoamVisual == null) return;
            EnsureCleanserVisuals();
            ApplyCleanserVisual(leftFoamVisual, cleanserAmount);
            ApplyCleanserVisual(rightFoamVisual, cleanserAmount);
        }

        private void EnsureCleanserVisuals()
        {
            if (foamMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Unlit");
                if (shader == null) shader = Shader.Find("Sprites/Default");
                foamMaterial = new Material(shader);
                foamMaterial.color = foamColor;
            }

            leftFoamVisual = EnsureFoamVisual(leftFoamVisual, leftHand != null ? leftHand.Palm : null, "SoapFoam_Left");
            rightFoamVisual = EnsureFoamVisual(rightFoamVisual, rightHand != null ? rightHand.Palm : null, "SoapFoam_Right");
        }

        private GameObject EnsureFoamVisual(GameObject existing, Transform parent, string visualName)
        {
            if (existing != null || parent == null) return existing;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visual.name = visualName;
            visual.transform.SetParent(parent, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.zero;
            var collider = visual.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            var renderer = visual.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = foamMaterial;
            visual.SetActive(false);
            return visual;
        }

        private void ApplyCleanserVisual(GameObject visual, float amount)
        {
            if (visual == null) return;
            bool visible = amount > 0.01f;
            visual.SetActive(visible);
            if (!visible) return;

            float scale = Mathf.Lerp(foamMaxScale * 0.45f, foamMaxScale, Mathf.Clamp01(amount));
            visual.transform.localScale = new Vector3(scale * 1.25f, scale * 0.35f, scale);
            var renderer = visual.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = foamColor;
        }

        private void ApplyFirstPersonCameraPose()
        {
            var cameraRig = Object.FindFirstObjectByType<FirstPersonHandCameraRig>();
            var mainCamera = Camera.main;
            if (cameraRig == null || cameraRig.CameraPose == null || mainCamera == null) return;

            if (!cameraPoseCached)
            {
                previousCameraPosition = mainCamera.transform.position;
                previousCameraRotation = mainCamera.transform.rotation;
                cameraPoseCached = true;
            }

            cameraRig.ApplyToMainCamera();
        }

        private void RestoreCameraPose()
        {
            var mainCamera = Camera.main;
            if (!cameraPoseCached || mainCamera == null) return;
            mainCamera.transform.position = previousCameraPosition;
            mainCamera.transform.rotation = previousCameraRotation;
            cameraPoseCached = false;
        }
    }
}
