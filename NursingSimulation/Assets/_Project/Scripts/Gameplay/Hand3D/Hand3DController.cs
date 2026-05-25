using UnityEngine;
using UnityEngine.InputSystem;

namespace NursingSim.Gameplay.Hand3D
{
    public enum HandSide
    {
        Left,
        Right
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(Rigidbody))]
    public class Hand3DController : MonoBehaviour
    {
        [Header("Control")]
        [SerializeField] private HandSide handSide = HandSide.Right;
        [Tooltip("Hands with input enabled can become the active desktop hand. Use Left Shift to switch.")]
        [SerializeField] private bool acceptInput = true;
        [Tooltip("A locked hand keeps its current pose so the other hand can continue the procedure.")]
        [SerializeField] private bool startLocked;
        [SerializeField] private ArmRigReferences rig;

        [Header("Target Control")]
        [Tooltip("Camera used to project mouse cursor into world. Defaults to Camera.main.")]
        [SerializeField] private Camera sourceCamera;
        [SerializeField] private Transform handTarget;
        [SerializeField] private Rigidbody handTargetBody;
        [Tooltip("Use the HandTarget position authored in the scene/prefab as the reset pose. Keeps manual editor placement intact.")]
        [SerializeField] private bool useSceneAuthoredTargetPose = true;
        [SerializeField] private Vector3 defaultTargetLocalPosition = new Vector3(0.18f, -0.18f, 0.38f);
        [SerializeField] private float targetMoveSpeed = 0.0018f;
        [SerializeField] private float keyboardMoveSpeed = 0.65f;
        [SerializeField] private float keyboardLiftSpeed = 0.45f;
        [SerializeField] private float depthMoveSpeed = 0.002f;
        [SerializeField] private float wristRotateSpeed = 0.16f;
        [SerializeField] private float workspaceRadius = 1.1f;
        [SerializeField] private Vector2 handSideLateralRange = new Vector2(-0.7f, 0.85f);
        [SerializeField] private Vector2 handVerticalRange = new Vector2(-0.75f, 0.32f);
        [SerializeField] private Vector2 handForwardRange = new Vector2(0.02f, 1.18f);
        [SerializeField] private float followSmoothing = 0.08f;
        [SerializeField] private bool allowMousePlaneMove;

        [Header("Anatomical Limits")]
        [SerializeField] private bool constrainTargetFromShoulder = true;
        [Tooltip("Shoulder-relative outward distance for the wrist target. Prevents cross-body target positions that flip the shoulder.")]
        [SerializeField] private Vector2 shoulderOutwardRange = new Vector2(0.22f, 0.72f);

        [Header("Collision")]
        [SerializeField] private bool enableCollisionBlocking = true;
        [SerializeField] private float collisionRadius = 0.035f;
        [SerializeField] private float handShellCollisionRadius = 0.05f;
        [SerializeField] private float fingerCollisionRadius = 0.018f;
        [SerializeField] private float collisionSkin = 0.008f;
        [SerializeField] private int collisionIterations = 3;
        [SerializeField] private LayerMask collisionMask = ~0;
        [SerializeField] private bool ignoreOversizedConcaveMeshBlockers = true;
        [SerializeField] private float oversizedConcaveMeshBlockerMaxExtent = 3f;

        [Header("Grab")]
        [SerializeField] private float grabRadius = 0.12f;
        [SerializeField] private LayerMask grabbableMask = ~0;
        [SerializeField] private Transform palm;

        [Header("Rub Detection")]
        [Tooltip("Mouse pixels per second above which the hand is considered 'rubbing'. CJK note: typical desktop ~600 px/s casual, ~1200 px/s scrub.")]
        [SerializeField] private float rubPixelsPerSec = 600f;

        private Rigidbody body;
        private Vector3 targetPos;
        private Quaternion targetRotation;
        private Vector3 authoredTargetLocalPosition;
        private Quaternion authoredTargetLocalRotation;
        private bool authoredTargetPoseCached;
        private Vector3 followVelocity;
        private GrabbablePhysicalTool held;
        private bool isLocked;
        private bool proceduralPoseActive;

        private static HandSide activeSide = HandSide.Right;
        private static int inputFrame = -1;
        private static Hand3DController leftHand;
        private static Hand3DController rightHand;

        public Transform Palm => palm != null ? palm : HandTarget;
        public Rigidbody Body => handTargetBody != null ? handTargetBody : body;
        public Transform HandTarget => handTarget != null ? handTarget : transform;
        public GrabbablePhysicalTool Held => held;
        public HandSide Side => handSide;
        public bool IsActiveInputHand => acceptInput && activeSide == handSide;
        public bool IsLocked => isLocked;
        public static HandSide ActiveSide => activeSide;
        public static Hand3DController ActiveInputHand => GetHand(activeSide);

        public static void SetActiveInputSide(HandSide side)
        {
            SetActiveSide(side);
        }

        public static void ResetAllTargetPoses()
        {
            leftHand?.ResetTargetPose();
            rightHand?.ResetTargetPose();
        }

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.useGravity = false;
            body.isKinematic = true; // Kinematic so it tracks cursor; tools attach via FixedJoint.
            if (!sourceCamera) sourceCamera = Camera.main;
            ResolveRigReferences();
            CacheAuthoredTargetPose();
            targetPos = HandTarget.position;
            targetRotation = HandTarget.rotation;
            isLocked = startLocked;
            ResetTargetPose();
        }

        private void OnEnable()
        {
            RegisterHand(this);
        }

        private void OnDisable()
        {
            UnregisterHand(this);
        }

        private void Update()
        {
            ProcessGlobalHandInput();

            if (!acceptInput) return;
            if (activeSide != handSide) return;
            if (proceduralPoseActive)
            {
                if (Mouse.current != null) UpdateRubSignal();
                return;
            }
            if (isLocked) return;
            if (!sourceCamera) return;

            UpdateTargetFromInput();
            if (Mouse.current != null)
            {
                HandleGrabInput();
                UpdateRubSignal();
            }
        }

        private void FixedUpdate()
        {
            if (handTarget != null)
            {
                handTarget.position = Vector3.SmoothDamp(handTarget.position, targetPos, ref followVelocity, followSmoothing);
                float t = Mathf.Clamp01(Time.fixedDeltaTime / Mathf.Max(followSmoothing, 0.001f));
                handTarget.rotation = Quaternion.Slerp(handTarget.rotation, targetRotation, t);
                if (handTargetBody != null) handTargetBody.MovePosition(handTarget.position);
                return;
            }

            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref followVelocity, followSmoothing);
            if (proceduralPoseActive)
            {
                float t = Mathf.Clamp01(Time.fixedDeltaTime / Mathf.Max(followSmoothing, 0.001f));
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, t);
            }
        }

        public void SetProceduralPose(Vector3 position, Quaternion rotation)
        {
            proceduralPoseActive = true;
            targetPos = ResolveCollisionLimitedPosition(position);
            targetRotation = rotation;
        }

        public void ClearProceduralPose()
        {
            proceduralPoseActive = false;
            targetPos = HandTarget.position;
            targetRotation = HandTarget.rotation;
            followVelocity = Vector3.zero;
        }

        public void ResetTargetPose()
        {
            if (handTarget == null)
            {
                targetPos = transform.position;
                targetRotation = transform.rotation;
                return;
            }

            proceduralPoseActive = false;
            targetPos = transform.TransformPoint(GetResetTargetLocalPosition());
            targetRotation = ResolveResetTargetRotation();
            handTarget.position = targetPos;
            handTarget.rotation = targetRotation;
            followVelocity = Vector3.zero;
        }

        private void CacheAuthoredTargetPose()
        {
            if (handTarget == null || authoredTargetPoseCached) return;
            authoredTargetLocalPosition = handTarget.localPosition;
            authoredTargetLocalRotation = handTarget.localRotation;
            authoredTargetPoseCached = true;

            if (useSceneAuthoredTargetPose)
            {
                ExpandWorkspaceToInclude(authoredTargetLocalPosition);
            }
        }

        private Quaternion ResolveResetTargetRotation()
        {
            if (useSceneAuthoredTargetPose && authoredTargetPoseCached && handTarget != null)
            {
                return transform.rotation * authoredTargetLocalRotation;
            }

            if (rig != null && rig.Hand != null)
            {
                return rig.Hand.rotation;
            }

            return handTarget != null ? handTarget.rotation : transform.rotation;
        }

        private Vector3 GetResetTargetLocalPosition()
        {
            return useSceneAuthoredTargetPose && authoredTargetPoseCached
                ? authoredTargetLocalPosition
                : GetDefaultTargetLocalPosition();
        }

        private Vector3 GetDefaultTargetLocalPosition()
        {
            var local = defaultTargetLocalPosition;
            float sideSign = handSide == HandSide.Left ? 1f : -1f;
            local.x = Mathf.Abs(local.x) * sideSign;
            local.y = Mathf.Clamp(local.y, handVerticalRange.x, handVerticalRange.y);
            local.z = Mathf.Clamp(Mathf.Abs(local.z), handForwardRange.x, handForwardRange.y);
            return local;
        }

        private void ExpandWorkspaceToInclude(Vector3 local)
        {
            float sideSign = handSide == HandSide.Left ? 1f : -1f;
            float signedLateral = local.x * sideSign;
            const float margin = 0.08f;
            handSideLateralRange.x = Mathf.Min(handSideLateralRange.x, signedLateral - margin);
            handSideLateralRange.y = Mathf.Max(handSideLateralRange.y, signedLateral + margin);
            handVerticalRange.x = Mathf.Min(handVerticalRange.x, local.y - margin);
            handVerticalRange.y = Mathf.Max(handVerticalRange.y, local.y + margin);
            handForwardRange.x = Mathf.Min(handForwardRange.x, local.z - margin);
            handForwardRange.y = Mathf.Max(handForwardRange.y, local.z + margin);
            workspaceRadius = Mathf.Max(workspaceRadius, local.magnitude + margin);
        }

        private void UpdateTargetFromInput()
        {
            bool changed = false;
            if (Mouse.current != null)
            {
                var mouseDelta = Mouse.current.delta.ReadValue();
                if (Mouse.current.rightButton.isPressed)
                {
                    var yaw = Quaternion.AngleAxis(mouseDelta.x * wristRotateSpeed, sourceCamera.transform.up);
                    var pitch = Quaternion.AngleAxis(-mouseDelta.y * wristRotateSpeed, sourceCamera.transform.right);
                    targetRotation = yaw * pitch * targetRotation;
                    changed = changed || mouseDelta.sqrMagnitude > 0.0001f;
                }
                else if (allowMousePlaneMove)
                {
                    var move = sourceCamera.transform.right * (mouseDelta.x * targetMoveSpeed) +
                               sourceCamera.transform.up * (mouseDelta.y * targetMoveSpeed);
                    targetPos += move;
                    changed = changed || move.sqrMagnitude > 0.000001f;
                }

                var scroll = Mouse.current.scroll.ReadValue().y;
                targetPos += Vector3.up * (scroll * depthMoveSpeed);
                changed = changed || Mathf.Abs(scroll) > 0.001f;
            }

            if (Keyboard.current != null)
            {
                var move = ReadKeyboardMove();
                if (move.sqrMagnitude > 0.000001f)
                {
                    targetPos += move * Time.deltaTime;
                    changed = true;
                }
            }

            if (changed) ClampTargetToWorkspace();
        }

        private Vector3 ReadKeyboardMove()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null) return Vector3.zero;

            var right = Vector3.ProjectOnPlane(sourceCamera.transform.right, Vector3.up);
            var forward = Vector3.ProjectOnPlane(sourceCamera.transform.forward, Vector3.up);
            if (right.sqrMagnitude < 0.000001f) right = transform.right;
            if (forward.sqrMagnitude < 0.000001f) forward = transform.forward;
            right.Normalize();
            forward.Normalize();

            var move = Vector3.zero;
            if (keyboard.aKey.isPressed) move -= right;
            if (keyboard.dKey.isPressed) move += right;
            if (keyboard.sKey.isPressed) move -= forward;
            if (keyboard.fKey.isPressed) move += forward;
            if (keyboard.spaceKey.isPressed) move += Vector3.up * (keyboardLiftSpeed / Mathf.Max(keyboardMoveSpeed, 0.001f));

            if (move.sqrMagnitude > 1f) move.Normalize();
            return move * keyboardMoveSpeed;
        }

        private void ClampTargetToWorkspace()
        {
            targetPos = GetWorkspaceClampedWorldPosition(targetPos);
            targetPos = ResolveCollisionLimitedPosition(targetPos);
        }

        public Vector3 GetWorkspaceClampedWorldPosition(Vector3 worldPosition)
        {
            var local = transform.InverseTransformPoint(worldPosition);
            local = ClampLocalToWorkspace(local);
            return transform.TransformPoint(local);
        }

        private Vector3 ClampLocalToWorkspace(Vector3 local)
        {
            float sideSign = handSide == HandSide.Left ? 1f : -1f;
            float signedLateral = local.x * sideSign;
            signedLateral = Mathf.Clamp(signedLateral, handSideLateralRange.x, handSideLateralRange.y);
            local.x = signedLateral * sideSign;
            local.y = Mathf.Clamp(local.y, handVerticalRange.x, handVerticalRange.y);
            local.z = Mathf.Clamp(local.z, handForwardRange.x, handForwardRange.y);

            if (local.magnitude > workspaceRadius)
            {
                local = local.normalized * workspaceRadius;
                signedLateral = Mathf.Clamp(local.x * sideSign, handSideLateralRange.x, handSideLateralRange.y);
                local.x = signedLateral * sideSign;
                local.y = Mathf.Clamp(local.y, handVerticalRange.x, handVerticalRange.y);
                local.z = Mathf.Clamp(local.z, handForwardRange.x, handForwardRange.y);
            }

            local = ClampTargetToShoulderHalfSpace(local);
            return local;
        }

        private Vector3 ClampTargetToShoulderHalfSpace(Vector3 local)
        {
            if (!constrainTargetFromShoulder || rig == null || rig.UpperArm == null) return local;

            var shoulderLocal = transform.InverseTransformPoint(rig.UpperArm.position);
            var shoulderToTarget = local - shoulderLocal;
            float sideSign = handSide == HandSide.Left ? -1f : 1f;
            float outward = shoulderToTarget.x * sideSign;
            outward = Mathf.Clamp(outward, shoulderOutwardRange.x, shoulderOutwardRange.y);
            shoulderToTarget.x = outward * sideSign;
            return shoulderLocal + shoulderToTarget;
        }

        private Vector3 ResolveCollisionLimitedPosition(Vector3 desiredWorldPosition)
        {
            if (!enableCollisionBlocking || collisionRadius <= 0f) return desiredWorldPosition;
            if (HandTarget == null) return ResolveProbeCollisionLimitedTargetPosition(desiredWorldPosition, transform.position, Vector3.zero, collisionRadius);

            var resolved = desiredWorldPosition;
            resolved = ResolveCollisionProbe(resolved, palm);

            if (rig != null)
            {
                resolved = ResolveCollisionProbe(resolved, rig.Hand);
                resolved = ResolveFingerCollisionProbes(resolved, HandFinger.Thumb);
                resolved = ResolveFingerCollisionProbes(resolved, HandFinger.Index);
                resolved = ResolveFingerCollisionProbes(resolved, HandFinger.Middle);
                resolved = ResolveFingerCollisionProbes(resolved, HandFinger.Ring);
                resolved = ResolveFingerCollisionProbes(resolved, HandFinger.Pinky);
            }

            return resolved;
        }

        private Vector3 ResolveCollisionProbe(Vector3 desiredTargetPosition, Transform probe)
        {
            if (probe == null || HandTarget == null) return desiredTargetPosition;
            return ResolveProbeCollisionLimitedTargetPosition(desiredTargetPosition, probe.position, probe.position - HandTarget.position, Mathf.Max(collisionRadius, handShellCollisionRadius));
        }

        private Vector3 ResolveCollisionProbe(Vector3 desiredTargetPosition, Transform probe, float probeRadius)
        {
            if (probe == null || HandTarget == null) return desiredTargetPosition;
            return ResolveProbeCollisionLimitedTargetPosition(desiredTargetPosition, probe.position, probe.position - HandTarget.position, probeRadius);
        }

        private Vector3 ResolveFingerCollisionProbes(Vector3 desiredTargetPosition, HandFinger finger)
        {
            if (rig == null) return desiredTargetPosition;
            var bones = rig.GetFingerBones(finger);
            foreach (var bone in bones)
            {
                desiredTargetPosition = ResolveCollisionProbe(desiredTargetPosition, bone, Mathf.Max(0.001f, fingerCollisionRadius));
            }

            return desiredTargetPosition;
        }

        private Vector3 ResolveProbeCollisionLimitedTargetPosition(Vector3 desiredTargetPosition, Vector3 currentProbePosition, Vector3 targetToProbeOffset, float probeRadius)
        {
            var desiredProbePosition = desiredTargetPosition + targetToProbeOffset;
            desiredProbePosition = ResolveProbeSweep(currentProbePosition, desiredProbePosition, probeRadius);
            desiredProbePosition = PushProbeOutOfOverlaps(desiredProbePosition, probeRadius);
            return desiredProbePosition - targetToProbeOffset;
        }

        private Vector3 ResolveProbeSweep(Vector3 currentProbePosition, Vector3 desiredProbePosition, float probeRadius)
        {
            var move = desiredProbePosition - currentProbePosition;
            float distance = move.magnitude;
            if (distance <= 0.0001f) return desiredProbePosition;

            var direction = move / distance;
            var hits = Physics.SphereCastAll(currentProbePosition, probeRadius, direction, distance + collisionSkin, collisionMask, QueryTriggerInteraction.Ignore);
            float nearest = float.PositiveInfinity;
            foreach (var hit in hits)
            {
                if (hit.collider == null || IsOwnCollider(hit.collider)) continue;
                if (ShouldIgnoreBlockingCollider(hit.collider)) continue;
                if (hit.distance < nearest) nearest = hit.distance;
            }

            if (nearest < float.PositiveInfinity)
            {
                desiredProbePosition = currentProbePosition + direction * Mathf.Max(0f, nearest - collisionSkin);
            }

            return desiredProbePosition;
        }

        private Vector3 PushProbeOutOfOverlaps(Vector3 probePosition, float probeRadius)
        {
            int iterations = Mathf.Max(8, collisionIterations);
            for (int i = 0; i < iterations; i++)
            {
                var overlaps = Physics.OverlapSphere(probePosition, probeRadius, collisionMask, QueryTriggerInteraction.Ignore);
                bool adjusted = false;
                foreach (var overlap in overlaps)
                {
                    if (overlap == null || IsOwnCollider(overlap)) continue;
                    if (ShouldIgnoreBlockingCollider(overlap)) continue;

                    var closest = GetClosestPointForProbe(overlap, probePosition);
                    var away = probePosition - closest;
                    if (away.sqrMagnitude < 0.000001f)
                    {
                        away = probePosition - overlap.bounds.center;
                        if (away.sqrMagnitude < 0.000001f) away = Vector3.up;
                        probePosition += away.normalized * (probeRadius + collisionSkin);
                        adjusted = true;
                        continue;
                    }

                    float penetration = probeRadius - away.magnitude + collisionSkin;
                    if (penetration <= 0f) continue;
                    probePosition += away.normalized * penetration;
                    adjusted = true;
                }

                if (!adjusted) break;
            }

            return probePosition;
        }

        private bool IsOwnCollider(Collider other)
        {
            return other != null && (other.transform == transform || other.transform.IsChildOf(transform));
        }

        private bool ShouldIgnoreBlockingCollider(Collider other)
        {
            if (!ignoreOversizedConcaveMeshBlockers) return false;
            if (other is not MeshCollider meshCollider || meshCollider.convex) return false;

            var size = meshCollider.bounds.size;
            float maxExtent = Mathf.Max(size.x, Mathf.Max(size.y, size.z));
            return maxExtent >= oversizedConcaveMeshBlockerMaxExtent;
        }

        private static Vector3 GetClosestPointForProbe(Collider collider, Vector3 probePosition)
        {
            return collider.ClosestPoint(probePosition);
        }

        private void HandleGrabInput()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (held != null) ReleaseHeld();
                else TryGrab();
            }
        }

        private void TryGrab()
        {
            if (held != null) return;
            var hits = Physics.OverlapSphere(Palm.position, grabRadius, grabbableMask, QueryTriggerInteraction.Collide);
            float best = float.PositiveInfinity;
            GrabbablePhysicalTool target = null;
            foreach (var h in hits)
            {
                var tool = h.GetComponentInParent<GrabbablePhysicalTool>();
                if (tool == null || tool.IsHeld) continue;
                float d = (tool.transform.position - Palm.position).sqrMagnitude;
                if (d < best) { best = d; target = tool; }
            }
            if (target != null)
            {
                target.AttachToHand(Body);
                held = target;
                HandActionEvents.RaiseGrabbed(target, Palm.position);
            }
        }

        private void ReleaseHeld()
        {
            if (held == null) return;
            var released = held;
            held.DetachFromHand();
            held = null;
            HandActionEvents.RaiseReleased(released, Palm.position);
        }

        private void UpdateRubSignal()
        {
            var delta = Mouse.current.delta.ReadValue();
            float pxPerSec = delta.magnitude / Mathf.Max(Time.deltaTime, 1e-5f);
            if (pxPerSec >= rubPixelsPerSec)
            {
                HandActionEvents.RaiseRubbed(Time.deltaTime);
            }
        }

        private void OnDrawGizmosSelected()
        {
            var p = palm != null ? palm.position : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(p, grabRadius);
        }

        private static void ProcessGlobalHandInput()
        {
            if (Keyboard.current == null) return;
            if (inputFrame == Time.frameCount) return;
            inputFrame = Time.frameCount;

            if (Keyboard.current.leftShiftKey.wasPressedThisFrame || Keyboard.current.rightShiftKey.wasPressedThisFrame || Keyboard.current.tabKey.wasPressedThisFrame)
            {
                SetActiveSide(Other(activeSide));
            }

            if (Keyboard.current.digit1Key.wasPressedThisFrame) SetActiveSide(HandSide.Left);
            if (Keyboard.current.digit2Key.wasPressedThisFrame) SetActiveSide(HandSide.Right);
            if (Keyboard.current.leftCtrlKey.wasPressedThisFrame || Keyboard.current.rightCtrlKey.wasPressedThisFrame) SetSeated(true);
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                SetSeated(false);
                ActiveInputHand?.ResetTargetPose();
            }
        }

        private static void SetActiveSide(HandSide side)
        {
            activeSide = side;
        }

        private static void SetSeated(bool seated)
        {
            var rig = Object.FindFirstObjectByType<FirstPersonHandCameraRig>();
            if (rig != null) rig.SetSeated(seated);
        }

        private static HandSide Other(HandSide side)
        {
            return side == HandSide.Left ? HandSide.Right : HandSide.Left;
        }

        private static Hand3DController GetHand(HandSide side)
        {
            return side == HandSide.Left ? leftHand : rightHand;
        }

        private static void RegisterHand(Hand3DController hand)
        {
            if (hand.handSide == HandSide.Left) leftHand = hand;
            else rightHand = hand;
        }

        private static void UnregisterHand(Hand3DController hand)
        {
            if (leftHand == hand) leftHand = null;
            if (rightHand == hand) rightHand = null;
        }

        private void ToggleLocked()
        {
            isLocked = !isLocked;
            targetPos = HandTarget.position;
            followVelocity = Vector3.zero;
        }

        private void ResolveRigReferences()
        {
            if (rig == null) rig = GetComponent<ArmRigReferences>();
            if (rig != null)
            {
                if (!rig.HasArmChain) rig.AutoBind(handSide);
                handTarget = handTarget != null ? handTarget : rig.HandTarget;
                palm = palm != null ? palm : rig.Palm;
            }

            if (palm != null && rig != null && rig.Hand != null && palm != rig.Hand && !palm.IsChildOf(rig.Hand))
            {
                palm.SetParent(rig.Hand, true);
            }

            if (handTarget != null)
            {
                handTargetBody = handTargetBody != null ? handTargetBody : handTarget.GetComponent<Rigidbody>();
                if (handTargetBody == null) handTargetBody = handTarget.gameObject.AddComponent<Rigidbody>();
                handTargetBody.useGravity = false;
                handTargetBody.isKinematic = true;
            }
        }
    }
}
