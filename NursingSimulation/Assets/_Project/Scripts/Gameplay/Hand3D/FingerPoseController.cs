using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace NursingSim.Gameplay.Hand3D
{
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(30)]
    public class FingerPoseController : MonoBehaviour
    {
        [SerializeField] private ArmRigReferences rig;
        [SerializeField] private Hand3DController handController;
        [SerializeField] private Vector3 fingerCurlEuler = new Vector3(-32f, 0f, 0f);
        [SerializeField] private Vector3 thumbCurlEuler = new Vector3(-12f, 7f, 0f);
        [SerializeField] private bool useAnatomicalCurlAxes = true;
        [SerializeField] private float fingerCurlDegrees = 64f;
        [SerializeField] private float thumbCurlDegrees = 34f;
        [SerializeField] private float maxKeyboardCurl = 0.68f;
        [SerializeField] private float maxGripCurl = 0.2f;
        [SerializeField] private float curlSpeed = 9f;

        private readonly float[] targetCurls = new float[5];
        private readonly float[] currentCurls = new float[5];
        private readonly Quaternion[][] restRotations = new Quaternion[5][];
        private readonly Vector3[][] curlAxes = new Vector3[5][];
        private Vector3 palmCurlNormalWorld;
        private bool palmCurlNormalResolved;
        private bool initialized;
        private bool inputEnabled = true;

        public float GripAmount { get; private set; }

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            if (!inputEnabled || handController == null) return;
            if (!handController.IsActiveInputHand)
            {
                if (handController.Held == null) ResetPose();
                else SetAllCurls(maxGripCurl);
                return;
            }

            ProcessFingerInput();
        }

        private void LateUpdate()
        {
            if (!initialized) Initialize();
            for (int i = 0; i < currentCurls.Length; i++)
            {
                currentCurls[i] = Mathf.MoveTowards(currentCurls[i], Mathf.Clamp01(targetCurls[i]), Time.deltaTime * curlSpeed);
            }

            ApplyPose();
        }

        public void Initialize()
        {
            if (rig == null) rig = GetComponent<ArmRigReferences>();
            if (handController == null) handController = GetComponent<Hand3DController>();
            if (rig == null) return;

            if (!rig.HasArmChain)
            {
                rig.AutoBind(handController != null ? handController.Side : HandSide.Right);
            }

            palmCurlNormalResolved = false;
            CacheRestRotations(HandFinger.Thumb);
            CacheRestRotations(HandFinger.Index);
            CacheRestRotations(HandFinger.Middle);
            CacheRestRotations(HandFinger.Ring);
            CacheRestRotations(HandFinger.Pinky);
            initialized = true;
        }

        public void SetInputEnabled(bool enabled)
        {
            inputEnabled = enabled;
            if (!enabled) ResetPose();
        }

        public void SetCurl(HandFinger finger, float amount)
        {
            targetCurls[(int)finger] = Mathf.Clamp01(amount);
        }

        public void SetAllCurls(float amount)
        {
            float clamped = Mathf.Clamp01(amount);
            for (int i = 0; i < targetCurls.Length; i++) targetCurls[i] = clamped;
        }

        public void SetClinicalPose(float thumb, float index, float middle, float ring, float pinky)
        {
            targetCurls[(int)HandFinger.Thumb] = Mathf.Clamp01(thumb);
            targetCurls[(int)HandFinger.Index] = Mathf.Clamp01(index);
            targetCurls[(int)HandFinger.Middle] = Mathf.Clamp01(middle);
            targetCurls[(int)HandFinger.Ring] = Mathf.Clamp01(ring);
            targetCurls[(int)HandFinger.Pinky] = Mathf.Clamp01(pinky);
        }

        public void ResetPose()
        {
            GripAmount = 0f;
            Array.Clear(targetCurls, 0, targetCurls.Length);
        }

        public void ApplyImmediate()
        {
            if (!initialized) Initialize();
            for (int i = 0; i < currentCurls.Length; i++) currentCurls[i] = Mathf.Clamp01(targetCurls[i]);
            ApplyPose();
        }

        private void ProcessFingerInput()
        {
            if (Keyboard.current == null && Mouse.current == null) return;

            GripAmount = handController.Held != null ? maxGripCurl : 0f;
            SetCurl(HandFinger.Thumb, Mathf.Max(GripAmount, KeyAmount(Keyboard.current?.zKey) * maxKeyboardCurl));
            SetCurl(HandFinger.Index, Mathf.Max(GripAmount, KeyAmount(Keyboard.current?.xKey) * maxKeyboardCurl));
            SetCurl(HandFinger.Middle, Mathf.Max(GripAmount, KeyAmount(Keyboard.current?.cKey) * maxKeyboardCurl));
            SetCurl(HandFinger.Ring, Mathf.Max(GripAmount, KeyAmount(Keyboard.current?.vKey) * maxKeyboardCurl));
            SetCurl(HandFinger.Pinky, Mathf.Max(GripAmount, KeyAmount(Keyboard.current?.bKey) * maxKeyboardCurl));

            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                ResetPose();
            }
        }

        private static float KeyAmount(KeyControl key)
        {
            return key != null && key.isPressed ? 1f : 0f;
        }

        private void CacheRestRotations(HandFinger finger)
        {
            var bones = rig.GetFingerBones(finger);
            var rotations = new Quaternion[bones.Length];
            var axes = new Vector3[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                rotations[i] = bones[i] != null ? bones[i].localRotation : Quaternion.identity;
                axes[i] = ResolveCurlAxis(finger, bones, i, rotations[i]);
            }

            restRotations[(int)finger] = rotations;
            curlAxes[(int)finger] = axes;
        }

        private void ApplyPose()
        {
            ApplyFinger(HandFinger.Thumb, thumbCurlEuler);
            ApplyFinger(HandFinger.Index, fingerCurlEuler);
            ApplyFinger(HandFinger.Middle, fingerCurlEuler);
            ApplyFinger(HandFinger.Ring, fingerCurlEuler);
            ApplyFinger(HandFinger.Pinky, fingerCurlEuler);
        }

        private void ApplyFinger(HandFinger finger, Vector3 curlEuler)
        {
            var bones = rig.GetFingerBones(finger);
            var rests = restRotations[(int)finger];
            var axes = curlAxes[(int)finger];
            if (rests == null) return;

            float curl = currentCurls[(int)finger];
            float maxDegrees = finger == HandFinger.Thumb ? thumbCurlDegrees : fingerCurlDegrees;
            for (int i = 0; i < bones.Length && i < rests.Length; i++)
            {
                if (bones[i] == null) continue;
                float weight = FingerJointWeight(finger, i);
                if (useAnatomicalCurlAxes && axes != null && i < axes.Length && axes[i].sqrMagnitude > 0.0001f)
                {
                    bones[i].localRotation = rests[i] * Quaternion.AngleAxis(maxDegrees * curl * weight, axes[i]);
                }
                else
                {
                    bones[i].localRotation = rests[i] * Quaternion.Euler(curlEuler * (curl * weight));
                }
            }
        }

        private Vector3 ResolveCurlAxis(HandFinger finger, Transform[] bones, int index, Quaternion restRotation)
        {
            if (finger != HandFinger.Thumb)
            {
                var palmCurlDirection = ResolvePalmCurlNormalWorld();
                if (palmCurlDirection.sqrMagnitude > 0.0001f)
                {
                    return ResolveAxisTowardDirection(bones, index, restRotation, palmCurlDirection);
                }
            }

            return ResolveAxisTowardPalm(bones, index, restRotation);
        }

        private Vector3 ResolveAxisTowardPalm(Transform[] bones, int index, Quaternion restRotation)
        {
            var bone = bones[index];
            if (bone == null) return Vector3.zero;

            var end = ResolveSegmentEnd(bones, index);
            var segment = end != null ? end.position - bone.position : bone.forward;
            if (segment.sqrMagnitude < 0.000001f) return Vector3.zero;
            segment.Normalize();

            var palmCenter = rig != null && rig.Hand != null ? rig.Hand.position : transform.position;
            var towardPalm = palmCenter - (end != null ? end.position : bone.position);
            towardPalm = Vector3.ProjectOnPlane(towardPalm, segment);
            if (towardPalm.sqrMagnitude < 0.000001f) return Vector3.zero;
            towardPalm.Normalize();

            return ResolveAxisTowardDirection(bones, index, restRotation, towardPalm);
        }

        private Vector3 ResolveAxisTowardDirection(Transform[] bones, int index, Quaternion restRotation, Vector3 curlDirectionWorld)
        {
            var bone = bones[index];
            if (bone == null) return Vector3.zero;

            var end = ResolveSegmentEnd(bones, index);
            var segment = end != null ? end.position - bone.position : bone.forward;
            if (segment.sqrMagnitude < 0.000001f) return Vector3.zero;
            segment.Normalize();

            var curlDirection = Vector3.ProjectOnPlane(curlDirectionWorld, segment);
            if (curlDirection.sqrMagnitude < 0.000001f) return Vector3.zero;
            curlDirection.Normalize();

            var worldAxis = Vector3.Cross(segment, curlDirection);
            if (worldAxis.sqrMagnitude < 0.000001f) return Vector3.zero;
            worldAxis.Normalize();

            var parent = bone.parent;
            var axisInParent = parent != null ? parent.InverseTransformDirection(worldAxis) : worldAxis;
            return (Quaternion.Inverse(restRotation) * axisInParent).normalized;
        }

        private Vector3 ResolvePalmCurlNormalWorld()
        {
            if (palmCurlNormalResolved) return palmCurlNormalWorld;
            palmCurlNormalResolved = true;

            var index = rig.GetFingerBones(HandFinger.Index);
            var middle = rig.GetFingerBones(HandFinger.Middle);
            var ring = rig.GetFingerBones(HandFinger.Ring);
            var pinky = rig.GetFingerBones(HandFinger.Pinky);
            if (index.Length == 0 || middle.Length == 0 || pinky.Length == 0)
            {
                palmCurlNormalWorld = Vector3.zero;
                return palmCurlNormalWorld;
            }

            var across = pinky[0].position - index[0].position;
            var forward = ResolveFingerTip(middle).position - middle[0].position;
            if (ring.Length > 0)
            {
                forward += ResolveFingerTip(index).position - index[0].position;
                forward += ResolveFingerTip(ring).position - ring[0].position;
                forward += ResolveFingerTip(pinky).position - pinky[0].position;
                forward *= 0.25f;
            }

            if (across.sqrMagnitude < 0.000001f || forward.sqrMagnitude < 0.000001f)
            {
                palmCurlNormalWorld = Vector3.zero;
                return palmCurlNormalWorld;
            }

            across.Normalize();
            forward.Normalize();
            var normal = Vector3.Cross(forward, across);
            if (normal.sqrMagnitude < 0.000001f)
            {
                palmCurlNormalWorld = Vector3.zero;
                return palmCurlNormalWorld;
            }

            normal.Normalize();
            palmCurlNormalWorld = ResolveHandMirroredPalmNormal(normal, middle);
            return palmCurlNormalWorld;
        }

        private Vector3 ResolveHandMirroredPalmNormal(Vector3 normal, Transform[] middleBones)
        {
            return ChoosePalmSide(normal, middleBones);
        }

        private Vector3 ChoosePalmSide(Vector3 normal, Transform[] middleBones)
        {
            var positiveScore = ScorePalmNormal(normal, middleBones);
            var negativeScore = ScorePalmNormal(-normal, middleBones);
            return positiveScore >= negativeScore ? normal : -normal;
        }

        private float ScorePalmNormal(Vector3 normal, Transform[] bones)
        {
            if (bones == null || bones.Length < 2 || rig == null || rig.Hand == null) return 0f;

            var origin = bones[1].position;
            var tip = ResolveFingerTip(bones).position;
            var segment = tip - origin;
            if (segment.sqrMagnitude < 0.000001f) return 0f;
            segment.Normalize();

            var axis = Vector3.Cross(segment, normal);
            if (axis.sqrMagnitude < 0.000001f) return 0f;
            axis.Normalize();

            var curledTip = origin + Quaternion.AngleAxis(18f, axis) * (tip - origin);
            var handPosition = rig.Hand.position;
            return Vector3.Distance(tip, handPosition) - Vector3.Distance(curledTip, handPosition);
        }

        private static Transform ResolveFingerTip(Transform[] bones)
        {
            var tip = bones[bones.Length - 1];
            if (tip != null && tip.childCount > 0) return tip.GetChild(0);
            return tip;
        }

        private static Transform ResolveSegmentEnd(Transform[] bones, int index)
        {
            if (index + 1 < bones.Length && bones[index + 1] != null) return bones[index + 1];
            var bone = bones[index];
            if (bone != null && bone.childCount > 0) return bone.GetChild(0);
            return null;
        }

        private static float FingerJointWeight(HandFinger finger, int jointIndex)
        {
            if (finger == HandFinger.Thumb)
            {
                return jointIndex switch
                {
                    0 => 0.35f,
                    1 => 0.7f,
                    2 => 0.52f,
                    _ => 0.24f
                };
            }

            return jointIndex switch
            {
                0 => 0.22f,
                1 => 0.82f,
                2 => 0.68f,
                _ => 0.38f
            };
        }
    }
}
