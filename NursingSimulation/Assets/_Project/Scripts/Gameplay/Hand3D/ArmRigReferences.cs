using System;
using UnityEngine;

namespace NursingSim.Gameplay.Hand3D
{
    public enum HandFinger
    {
        Thumb,
        Index,
        Middle,
        Ring,
        Pinky
    }

    [DisallowMultipleComponent]
    public class ArmRigReferences : MonoBehaviour
    {
        [SerializeField] private HandSide handSide = HandSide.Right;
        [SerializeField] private Transform shoulder;
        [SerializeField] private Transform upperArm;
        [SerializeField] private Transform foreArm;
        [SerializeField] private Transform hand;
        [SerializeField] private Transform shoulderAnchor;
        [SerializeField] private Transform handTarget;
        [SerializeField] private Transform elbowHint;
        [SerializeField] private Transform palm;
        [SerializeField] private Transform gripSocket;
        [SerializeField] private Transform indexTip;
        [SerializeField] private Transform thumbTip;

        [SerializeField] private Transform[] thumbBones = Array.Empty<Transform>();
        [SerializeField] private Transform[] indexBones = Array.Empty<Transform>();
        [SerializeField] private Transform[] middleBones = Array.Empty<Transform>();
        [SerializeField] private Transform[] ringBones = Array.Empty<Transform>();
        [SerializeField] private Transform[] pinkyBones = Array.Empty<Transform>();

        public HandSide Side => handSide;
        public Transform Shoulder => shoulder;
        public Transform UpperArm => upperArm;
        public Transform ForeArm => foreArm;
        public Transform Hand => hand;
        public Transform ShoulderAnchor => shoulderAnchor;
        public Transform HandTarget => handTarget;
        public Transform ElbowHint => elbowHint;
        public Transform Palm => palm != null ? palm : handTarget;
        public Transform GripSocket => gripSocket;
        public Transform IndexTip => indexTip;
        public Transform ThumbTip => thumbTip;

        public bool HasArmChain => upperArm != null && foreArm != null && hand != null && handTarget != null;

        public void AutoBind(HandSide side)
        {
            handSide = side;
            string prefix = side == HandSide.Right ? "mixamorig:Right" : "mixamorig:Left";
            shoulder = FindChildRecursive(transform, $"{prefix}Shoulder");
            upperArm = FindChildRecursive(transform, $"{prefix}Arm");
            foreArm = FindChildRecursive(transform, $"{prefix}ForeArm");
            hand = FindChildRecursive(transform, $"{prefix}Hand");

            shoulderAnchor = EnsureAnchor("ShoulderAnchor", Vector3.zero);
            handTarget = EnsureAnchor("HandTarget", LocalFromWorld(hand != null ? hand.position : transform.position + transform.forward * 0.35f));
            elbowHint = EnsureAnchor("ElbowHint", LocalFromWorld(CalculateElbowHintWorld()));
            palm = EnsureAnchor("Palm", LocalFromWorld(hand != null ? hand.position : transform.position));
            gripSocket = EnsureAnchor("GripSocket", LocalFromWorld((hand != null ? hand.position : transform.position) + transform.forward * 0.03f));
            indexTip = EnsureAnchor("IndexTip", LocalFromWorld((hand != null ? hand.position : transform.position) + transform.forward * 0.07f));
            thumbTip = EnsureAnchor("ThumbTip", LocalFromWorld((hand != null ? hand.position : transform.position) + transform.right * (side == HandSide.Right ? -0.03f : 0.03f)));

            thumbBones = FindFingerBones(prefix, "Thumb");
            indexBones = FindFingerBones(prefix, "Index");
            middleBones = FindFingerBones(prefix, "Middle");
            ringBones = FindFingerBones(prefix, "Ring");
            pinkyBones = FindFingerBones(prefix, "Pinky");
        }

        public Transform[] GetFingerBones(HandFinger finger)
        {
            return finger switch
            {
                HandFinger.Thumb => thumbBones,
                HandFinger.Index => indexBones,
                HandFinger.Middle => middleBones,
                HandFinger.Ring => ringBones,
                HandFinger.Pinky => pinkyBones,
                _ => Array.Empty<Transform>()
            };
        }

        private Transform EnsureAnchor(string anchorName, Vector3 localPosition)
        {
            var found = transform.Find(anchorName);
            if (found == null)
            {
                found = new GameObject(anchorName).transform;
                found.SetParent(transform, false);
            }

            found.localPosition = localPosition;
            return found;
        }

        private Vector3 CalculateElbowHintWorld()
        {
            if (upperArm == null || foreArm == null || hand == null)
            {
                return transform.position + transform.forward * 0.2f + Vector3.down * 0.18f;
            }

            var elbow = foreArm.position;
            var rootToHand = (hand.position - upperArm.position).normalized;
            var sideSign = handSide == HandSide.Right ? 1f : -1f;
            var side = Vector3.Cross(Vector3.up, rootToHand).normalized * sideSign;
            if (side.sqrMagnitude < 0.0001f) side = transform.right * sideSign;
            return elbow + side * 0.22f + Vector3.down * 0.08f;
        }

        private Vector3 LocalFromWorld(Vector3 worldPosition)
        {
            return transform.InverseTransformPoint(worldPosition);
        }

        private Transform[] FindFingerBones(string sidePrefix, string fingerName)
        {
            var bones = new Transform[4];
            int count = 0;
            for (int i = 1; i <= 4; i++)
            {
                var bone = FindChildRecursive(transform, $"{sidePrefix}Hand{fingerName}{i}");
                if (bone == null) continue;
                bones[count++] = bone;
            }

            if (count == bones.Length) return bones;
            Array.Resize(ref bones, count);
            return bones;
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
