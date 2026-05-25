using UnityEngine;

namespace NursingSim.Gameplay.Hand3D
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(40)]
    public class TwoBoneArmIK : MonoBehaviour
    {
        [SerializeField] private ArmRigReferences rig;
        [SerializeField] private Transform target;
        [SerializeField] private Transform elbowHint;
        [SerializeField] private float reachSoftness = 0.98f;
        [SerializeField] private float minElbowFlexionDegrees = 12f;
        [SerializeField] private float maxElbowFlexionDegrees = 105f;
        [SerializeField] private float maxWristSwingDegrees = 28f;
        [SerializeField] private bool useAnatomicalElbowPole = true;
        [SerializeField] private float elbowPoleDownWeight = 0.85f;
        [SerializeField] private float elbowPoleOutwardWeight = 0.45f;
        [SerializeField] private float elbowPoleHintWeight = 0.25f;
        [SerializeField] private float elbowPoleRestWeight = 0.18f;
        [SerializeField] private float elbowPoleSmoothSpeed = 12f;
        [SerializeField] private bool solveInLateUpdate = true;

        private Quaternion upperArmRestLocal;
        private Quaternion foreArmRestLocal;
        private Quaternion handRestLocal;
        private Vector3 restPoleLocalDirection;
        private Vector3 previousPoleDirection;
        private float upperLength;
        private float lowerLength;
        private bool initialized;
        private bool hasRestPole;
        private bool hasPreviousPole;

        public ArmRigReferences Rig => rig;
        public Transform Target => target;
        public float MaxReach => Mathf.Min((upperLength + lowerLength) * Mathf.Clamp01(reachSoftness), DistanceForElbowFlexion(minElbowFlexionDegrees));
        public float MinReach => DistanceForElbowFlexion(maxElbowFlexionDegrees);
        public float MaxWristSwingDegrees => maxWristSwingDegrees;

        private void Awake()
        {
            Initialize();
        }

        private void LateUpdate()
        {
            if (solveInLateUpdate) SolveImmediate();
        }

        public void Initialize()
        {
            if (rig == null) rig = GetComponent<ArmRigReferences>();
            if (rig == null) return;

            if (!rig.HasArmChain)
            {
                var hand = GetComponent<Hand3DController>();
                rig.AutoBind(hand != null ? hand.Side : HandSide.Right);
            }

            target = target != null ? target : rig.HandTarget;
            elbowHint = elbowHint != null ? elbowHint : rig.ElbowHint;

            if (!rig.HasArmChain) return;
            upperArmRestLocal = rig.UpperArm.localRotation;
            foreArmRestLocal = rig.ForeArm.localRotation;
            handRestLocal = rig.Hand.localRotation;
            upperLength = Vector3.Distance(rig.UpperArm.position, rig.ForeArm.position);
            lowerLength = Vector3.Distance(rig.ForeArm.position, rig.Hand.position);
            initialized = upperLength > 0.0001f && lowerLength > 0.0001f;
            CacheRestPoleDirection();
        }

        public Vector3 GetClampedTargetPosition()
        {
            if (!initialized) Initialize();
            if (!initialized || target == null) return target != null ? target.position : transform.position;

            var root = rig.UpperArm.position;
            var rootToTarget = target.position - root;
            var reachDirection = rootToTarget.sqrMagnitude > 0.000001f ? rootToTarget.normalized : GetFallbackReachDirection();
            float distance = Mathf.Clamp(rootToTarget.magnitude, MinReach, MaxReach);
            return root + reachDirection * distance;
        }

        public void SolveImmediate()
        {
            if (!initialized) Initialize();
            if (!initialized || target == null) return;

            rig.UpperArm.localRotation = upperArmRestLocal;
            rig.ForeArm.localRotation = foreArmRestLocal;
            rig.Hand.localRotation = handRestLocal;

            var clampedTarget = GetClampedTargetPosition();
            var elbowPosition = CalculateElbowPosition(rig.UpperArm.position, clampedTarget);

            RotateBoneToward(rig.UpperArm, rig.ForeArm.position, elbowPosition);
            RotateBoneToward(rig.ForeArm, rig.Hand.position, clampedTarget);
            ApplyLimitedWristRotation();
        }

        private static void RotateBoneToward(Transform bone, Vector3 currentEnd, Vector3 desiredEnd)
        {
            var bonePosition = bone.position;
            var current = currentEnd - bonePosition;
            var desired = desiredEnd - bonePosition;
            if (current.sqrMagnitude < 0.000001f || desired.sqrMagnitude < 0.000001f) return;
            bone.rotation = Quaternion.FromToRotation(current, desired) * bone.rotation;
        }

        private Vector3 CalculateElbowPosition(Vector3 root, Vector3 handPosition)
        {
            var rootToHand = handPosition - root;
            float handDistance = rootToHand.magnitude;
            if (handDistance < 0.0001f) return rig.ForeArm.position;

            var reachDirection = rootToHand / handDistance;
            var poleDirection = GetPoleDirection(root, reachDirection);
            float upperSquared = upperLength * upperLength;
            float lowerSquared = lowerLength * lowerLength;
            float distanceSquared = handDistance * handDistance;
            float alongReach = (upperSquared - lowerSquared + distanceSquared) / (2f * handDistance);
            float awayFromReach = Mathf.Sqrt(Mathf.Max(0f, upperSquared - alongReach * alongReach));

            return root + reachDirection * alongReach + poleDirection * awayFromReach;
        }

        private Vector3 GetPoleDirection(Vector3 root, Vector3 reachDirection)
        {
            if (useAnatomicalElbowPole)
            {
                var anatomicalPole = GetAnatomicalPoleDirection(reachDirection);
                if (anatomicalPole.sqrMagnitude > 0.000001f)
                {
                    var hintPole = GetHintPoleDirection(root, reachDirection);
                    if (hintPole.sqrMagnitude > 0.000001f && Vector3.Dot(anatomicalPole, hintPole) > 0f)
                    {
                        return StabilizePoleDirection(Vector3.Slerp(anatomicalPole, hintPole, Mathf.Clamp01(elbowPoleHintWeight)).normalized, reachDirection);
                    }

                    return StabilizePoleDirection(anatomicalPole, reachDirection);
                }
            }

            var pole = GetHintPoleDirection(root, reachDirection);
            if (pole.sqrMagnitude < 0.000001f)
            {
                pole = ProjectPole(transform.up, reachDirection);
            }

            if (pole.sqrMagnitude < 0.000001f)
            {
                pole = ProjectPole(Vector3.up, reachDirection);
            }

            if (pole.sqrMagnitude < 0.000001f)
            {
                pole = ProjectPole(Vector3.right, reachDirection);
            }

            return StabilizePoleDirection(pole.normalized, reachDirection);
        }

        private void CacheRestPoleDirection()
        {
            hasPreviousPole = false;
            hasRestPole = false;
            if (!initialized || rig == null || rig.UpperArm == null || rig.ForeArm == null || rig.Hand == null) return;

            var restReach = rig.Hand.position - rig.UpperArm.position;
            if (restReach.sqrMagnitude < 0.000001f) return;

            var restPole = ProjectPole(rig.ForeArm.position - rig.UpperArm.position, restReach.normalized);
            if (restPole.sqrMagnitude < 0.000001f) return;

            restPoleLocalDirection = transform.InverseTransformDirection(restPole.normalized);
            hasRestPole = true;
        }

        private Vector3 StabilizePoleDirection(Vector3 desiredPole, Vector3 reachDirection)
        {
            if (desiredPole.sqrMagnitude < 0.000001f) return desiredPole;

            var pole = desiredPole.normalized;
            var restPole = GetRestPoleDirection(reachDirection);
            if (restPole.sqrMagnitude > 0.000001f)
            {
                pole = BlendSameHemisphere(pole, restPole, elbowPoleRestWeight);
            }

            var previousPole = GetPreviousPoleDirection(reachDirection);
            if (previousPole.sqrMagnitude > 0.000001f)
            {
                if (Vector3.Dot(pole, previousPole) < 0f) pole = -pole;
                float t = Application.isPlaying ? Mathf.Clamp01(Time.deltaTime * elbowPoleSmoothSpeed) : 1f;
                pole = Vector3.Slerp(previousPole, pole, t).normalized;
            }

            previousPoleDirection = pole;
            hasPreviousPole = true;
            return pole;
        }

        private Vector3 GetRestPoleDirection(Vector3 reachDirection)
        {
            if (!hasRestPole) return Vector3.zero;
            var restPole = ProjectPole(transform.TransformDirection(restPoleLocalDirection), reachDirection);
            return restPole.sqrMagnitude > 0.000001f ? restPole.normalized : Vector3.zero;
        }

        private Vector3 GetPreviousPoleDirection(Vector3 reachDirection)
        {
            if (!hasPreviousPole) return Vector3.zero;
            var previousPole = ProjectPole(previousPoleDirection, reachDirection);
            return previousPole.sqrMagnitude > 0.000001f ? previousPole.normalized : Vector3.zero;
        }

        private static Vector3 BlendSameHemisphere(Vector3 basePole, Vector3 secondaryPole, float weight)
        {
            if (secondaryPole.sqrMagnitude < 0.000001f) return basePole;
            if (Vector3.Dot(basePole, secondaryPole) < 0f) secondaryPole = -secondaryPole;
            return Vector3.Slerp(basePole, secondaryPole.normalized, Mathf.Clamp01(weight)).normalized;
        }

        private Vector3 GetAnatomicalPoleDirection(Vector3 reachDirection)
        {
            float sideSign = rig != null && rig.Side == HandSide.Left ? -1f : 1f;
            var down = -transform.up;
            var outward = transform.right * sideSign;
            var pole = ProjectPole(down * elbowPoleDownWeight + outward * elbowPoleOutwardWeight, reachDirection);
            if (pole.sqrMagnitude > 0.000001f) return pole.normalized;

            pole = ProjectPole(down, reachDirection);
            if (pole.sqrMagnitude > 0.000001f) return pole.normalized;

            pole = ProjectPole(outward, reachDirection);
            return pole.sqrMagnitude > 0.000001f ? pole.normalized : Vector3.zero;
        }

        private Vector3 GetHintPoleDirection(Vector3 root, Vector3 reachDirection)
        {
            var pole = Vector3.zero;
            if (elbowHint != null)
            {
                pole = ProjectPole(elbowHint.position - root, reachDirection);
            }

            if (pole.sqrMagnitude < 0.000001f)
            {
                pole = ProjectPole(rig.ForeArm.position - root, reachDirection);
            }

            return pole.sqrMagnitude > 0.000001f ? pole.normalized : Vector3.zero;
        }

        private static Vector3 ProjectPole(Vector3 pole, Vector3 reachDirection)
        {
            return Vector3.ProjectOnPlane(pole, reachDirection);
        }

        private Vector3 GetFallbackReachDirection()
        {
            if (rig != null && rig.UpperArm != null && rig.Hand != null)
            {
                var current = rig.Hand.position - rig.UpperArm.position;
                if (current.sqrMagnitude > 0.000001f) return current.normalized;
            }

            if (transform.forward.sqrMagnitude > 0.000001f) return transform.forward;
            return Vector3.forward;
        }

        private float DistanceForElbowFlexion(float flexionDegrees)
        {
            if (!initialized || upperLength <= 0f || lowerLength <= 0f) return 0f;
            float flexion = Mathf.Clamp(flexionDegrees, 0f, 179f) * Mathf.Deg2Rad;
            float distanceSquared = upperLength * upperLength + lowerLength * lowerLength + 2f * upperLength * lowerLength * Mathf.Cos(flexion);
            return Mathf.Sqrt(Mathf.Max(0f, distanceSquared));
        }

        private void ApplyLimitedWristRotation()
        {
            if (target == null || rig.Hand == null || rig.ForeArm == null) return;

            var neutralWrist = rig.ForeArm.rotation * handRestLocal;
            rig.Hand.rotation = ClampRotation(target.rotation, neutralWrist, maxWristSwingDegrees);
        }

        private static Quaternion ClampRotation(Quaternion desired, Quaternion reference, float maxDegrees)
        {
            float angle = Quaternion.Angle(reference, desired);
            if (angle <= maxDegrees || angle < 0.001f) return desired;
            return Quaternion.Slerp(reference, desired, Mathf.Clamp01(maxDegrees / angle));
        }
    }
}
