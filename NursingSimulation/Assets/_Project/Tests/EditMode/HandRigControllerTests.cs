using NursingSim.Gameplay.Hand3D;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace NursingSim.Tests
{
    public class HandRigControllerTests
    {
        [Test]
        public void TwoBoneArmIK_DoesNotMoveShoulderRootAndClampsReach()
        {
            var root = BuildArmChain(HandSide.Right);
            var rig = root.AddComponent<ArmRigReferences>();
            rig.AutoBind(HandSide.Right);

            var ik = root.AddComponent<TwoBoneArmIK>();
            ik.Initialize();
            var rootBefore = root.transform.position;
            var shoulderBefore = rig.Shoulder.position;

            rig.HandTarget.position = rig.UpperArm.position + Vector3.right * 5f;
            var clamped = ik.GetClampedTargetPosition();
            ik.SolveImmediate();

            Assert.That(root.transform.position, Is.EqualTo(rootBefore));
            Assert.That(rig.Shoulder.position, Is.EqualTo(shoulderBefore));
            Assert.That(Vector3.Distance(rig.UpperArm.position, clamped), Is.LessThanOrEqualTo(ik.MaxReach + 0.001f));
            Object.DestroyImmediate(root);
        }

        [Test]
        public void FingerPoseController_AppliesCurlOnlyToRequestedFinger()
        {
            var root = BuildArmChain(HandSide.Right);
            var rig = root.AddComponent<ArmRigReferences>();
            rig.AutoBind(HandSide.Right);

            var fingers = root.AddComponent<FingerPoseController>();
            fingers.Initialize();

            var index = rig.GetFingerBones(HandFinger.Index)[1];
            var middle = rig.GetFingerBones(HandFinger.Middle)[1];
            var indexBefore = index.localRotation;
            var middleBefore = middle.localRotation;

            fingers.SetCurl(HandFinger.Index, 1f);
            fingers.ApplyImmediate();

            Assert.That(Quaternion.Angle(indexBefore, index.localRotation), Is.GreaterThan(1f));
            Assert.That(Quaternion.Angle(middleBefore, middle.localRotation), Is.LessThan(0.01f));
            Object.DestroyImmediate(root);
        }

        [Test]
        public void FingerPoseController_PrefabFingersCurlInwardWithDistributedJoints()
        {
            foreach (var prefabPath in new[]
                     {
                         "Assets/_Project/Prefabs/Characters/PlayerHand_Left.prefab",
                         "Assets/_Project/Prefabs/Characters/PlayerHand_Right.prefab"
                     })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                Assert.That(prefab, Is.Not.Null, prefabPath);

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                try
                {
                    var rig = instance.GetComponent<ArmRigReferences>();
                    var controller = instance.GetComponent<FingerPoseController>();
                    controller.Initialize();

                    foreach (var finger in new[] { HandFinger.Index, HandFinger.Middle, HandFinger.Ring, HandFinger.Pinky })
                    {
                        controller.ResetPose();
                        controller.ApplyImmediate();

                        var bones = rig.GetFingerBones(finger);
                        var beforeRotations = new Quaternion[bones.Length];
                        for (int i = 0; i < bones.Length; i++) beforeRotations[i] = bones[i].localRotation;

                        var tip = FingerTip(bones);
                        var beforeDistance = Vector3.Distance(tip.position, rig.Hand.position);
                        controller.SetCurl(finger, 1f);
                        controller.ApplyImmediate();

                        var afterDistance = Vector3.Distance(tip.position, rig.Hand.position);
                        Assert.That(beforeDistance - afterDistance, Is.GreaterThan(0.005f), $"{prefabPath} {finger}");
                        Assert.That(Quaternion.Angle(beforeRotations[1], bones[1].localRotation), Is.GreaterThan(20f), $"{prefabPath} {finger} proximal");
                        Assert.That(Quaternion.Angle(beforeRotations[2], bones[2].localRotation), Is.GreaterThan(20f), $"{prefabPath} {finger} middle");
                        Assert.That(Quaternion.Angle(beforeRotations[3], bones[3].localRotation), Is.GreaterThan(10f), $"{prefabPath} {finger} distal");
                    }
                }
                finally
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }

        [Test]
        public void FingerPoseController_LeftAndRightCurlAsMirroredPairs()
        {
            var leftPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/PlayerHand_Left.prefab");
            var rightPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/PlayerHand_Right.prefab");
            Assert.That(leftPrefab, Is.Not.Null);
            Assert.That(rightPrefab, Is.Not.Null);

            var left = (GameObject)PrefabUtility.InstantiatePrefab(leftPrefab);
            var right = (GameObject)PrefabUtility.InstantiatePrefab(rightPrefab);
            try
            {
                var leftRig = left.GetComponent<ArmRigReferences>();
                var rightRig = right.GetComponent<ArmRigReferences>();
                var leftController = left.GetComponent<FingerPoseController>();
                var rightController = right.GetComponent<FingerPoseController>();
                leftController.Initialize();
                rightController.Initialize();

                foreach (var finger in new[] { HandFinger.Index, HandFinger.Middle, HandFinger.Ring, HandFinger.Pinky })
                {
                    leftController.ResetPose();
                    rightController.ResetPose();
                    leftController.ApplyImmediate();
                    rightController.ApplyImmediate();

                    var leftBones = leftRig.GetFingerBones(finger);
                    var rightBones = rightRig.GetFingerBones(finger);
                    var leftBefore = FingerTip(leftBones).position;
                    var rightBefore = FingerTip(rightBones).position;
                    var leftBeforeRotations = new Quaternion[leftBones.Length];
                    var rightBeforeRotations = new Quaternion[rightBones.Length];
                    for (int i = 0; i < leftBones.Length; i++) leftBeforeRotations[i] = leftBones[i].localRotation;
                    for (int i = 0; i < rightBones.Length; i++) rightBeforeRotations[i] = rightBones[i].localRotation;

                    leftController.SetCurl(finger, 1f);
                    rightController.SetCurl(finger, 1f);
                    leftController.ApplyImmediate();
                    rightController.ApplyImmediate();

                    var mirroredLeftMove = MirrorXAndY(FingerTip(leftBones).position - leftBefore);
                    var rightMove = FingerTip(rightBones).position - rightBefore;
                    Assert.That(Vector3.Distance(mirroredLeftMove, rightMove), Is.LessThan(0.012f), finger.ToString());

                    for (int i = 0; i < Mathf.Min(leftBones.Length, rightBones.Length); i++)
                    {
                        var leftDelta = Quaternion.Angle(leftBeforeRotations[i], leftBones[i].localRotation);
                        var rightDelta = Quaternion.Angle(rightBeforeRotations[i], rightBones[i].localRotation);
                        Assert.That(Mathf.Abs(leftDelta - rightDelta), Is.LessThan(0.1f), $"{finger} joint {i}");
                    }
                }
            }
            finally
            {
                Object.DestroyImmediate(left);
                Object.DestroyImmediate(right);
            }
        }

        [Test]
        public void FingerPoseController_PrefabFingersCurlTowardPalmWithoutSidewaysDrift()
        {
            foreach (var prefabPath in new[]
                     {
                         "Assets/_Project/Prefabs/Characters/PlayerHand_Left.prefab",
                         "Assets/_Project/Prefabs/Characters/PlayerHand_Right.prefab"
                     })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                Assert.That(prefab, Is.Not.Null, prefabPath);

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                try
                {
                    var rig = instance.GetComponent<ArmRigReferences>();
                    var controller = instance.GetComponent<FingerPoseController>();
                    controller.Initialize();

                    var palmNormal = PalmCurlNormal(rig);
                    var spreadAxis = FingerSpreadAxis(rig);
                    Assert.That(palmNormal.sqrMagnitude, Is.GreaterThan(0.9f), prefabPath);
                    Assert.That(spreadAxis.sqrMagnitude, Is.GreaterThan(0.9f), prefabPath);

                    foreach (var finger in new[] { HandFinger.Index, HandFinger.Middle, HandFinger.Ring, HandFinger.Pinky })
                    {
                        controller.ResetPose();
                        controller.ApplyImmediate();

                        var bones = rig.GetFingerBones(finger);
                        var before = FingerTip(bones).position;
                        controller.SetCurl(finger, 1f);
                        controller.ApplyImmediate();

                        var move = FingerTip(bones).position - before;
                        var inwardMove = Vector3.Dot(move, palmNormal);
                        var sidewaysMove = Mathf.Abs(Vector3.Dot(move, spreadAxis));

                        Assert.That(inwardMove, Is.GreaterThan(0.035f), $"{prefabPath} {finger} inward");
                        Assert.That(sidewaysMove, Is.LessThan(0.025f), $"{prefabPath} {finger} sideways");
                    }
                }
                finally
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }

        [Test]
        public void Hand3DController_RightWorkspaceIncludesSanitizerPressPoint()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Prefabs/Characters/PlayerHand_Right.prefab");
            Assert.That(prefab, Is.Not.Null);

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                var controller = instance.GetComponent<Hand3DController>();
                var sanitizerPressPointLocal = new Vector3(0.33f, 0.112f, 0.371f);
                var pressPointWorld = controller.transform.TransformPoint(sanitizerPressPointLocal);
                var clamped = controller.GetWorkspaceClampedWorldPosition(pressPointWorld);

                Assert.That(Vector3.Distance(clamped, pressPointWorld), Is.LessThan(0.01f));
            }
            finally
            {
                Object.DestroyImmediate(instance);
            }
        }

        [Test]
        public void Hand3DController_WorkspacePreventsCrossShoulderTargets()
        {
            foreach (var prefabPath in new[]
                     {
                         "Assets/_Project/Prefabs/Characters/PlayerHand_Left.prefab",
                         "Assets/_Project/Prefabs/Characters/PlayerHand_Right.prefab"
                     })
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                Assert.That(prefab, Is.Not.Null, prefabPath);

                var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
                try
                {
                    var controller = instance.GetComponent<Hand3DController>();
                    var rig = instance.GetComponent<ArmRigReferences>();
                    var shoulderLocal = instance.transform.InverseTransformPoint(rig.UpperArm.position);
                    float sideSign = rig.Side == HandSide.Left ? -1f : 1f;
                    var crossedLocal = shoulderLocal + Vector3.right * (-sideSign * 0.5f);

                    var clampedWorld = controller.GetWorkspaceClampedWorldPosition(instance.transform.TransformPoint(crossedLocal));
                    var clampedLocal = instance.transform.InverseTransformPoint(clampedWorld);
                    float outward = (clampedLocal.x - shoulderLocal.x) * sideSign;

                    Assert.That(outward, Is.GreaterThanOrEqualTo(0.219f), prefabPath);
                }
                finally
                {
                    Object.DestroyImmediate(instance);
                }
            }
        }

        [Test]
        public void TwoBoneArmIK_KeepsElbowOnAnatomicalPoleSide()
        {
            var root = BuildArmChain(HandSide.Right);
            var rig = root.AddComponent<ArmRigReferences>();
            rig.AutoBind(HandSide.Right);

            var ik = root.AddComponent<TwoBoneArmIK>();
            ik.Initialize();
            var shoulder = rig.UpperArm.position;
            rig.HandTarget.position = shoulder + Vector3.right * 0.42f + Vector3.forward * 0.05f;

            ik.SolveImmediate();

            var reachDirection = (rig.Hand.position - shoulder).normalized;
            var elbowPole = Vector3.ProjectOnPlane(rig.ForeArm.position - shoulder, reachDirection).normalized;
            var naturalPole = Vector3.ProjectOnPlane(Vector3.down, reachDirection).normalized;
            Assert.That(Vector3.Dot(elbowPole, naturalPole), Is.GreaterThan(0.8f));
            Object.DestroyImmediate(root);
        }

        [Test]
        public void TwoBoneArmIK_LimitsWristRotationFromForearm()
        {
            var root = BuildArmChain(HandSide.Right);
            var rig = root.AddComponent<ArmRigReferences>();
            rig.AutoBind(HandSide.Right);

            var ik = root.AddComponent<TwoBoneArmIK>();
            ik.Initialize();
            rig.ElbowHint.position = rig.UpperArm.position + Vector3.up;
            rig.HandTarget.position = rig.UpperArm.position + Vector3.right * 0.42f + Vector3.forward * 0.04f;
            rig.HandTarget.rotation = Quaternion.Euler(0f, 180f, 0f);

            ik.SolveImmediate();

            Assert.That(Quaternion.Angle(rig.ForeArm.rotation, rig.Hand.rotation), Is.LessThanOrEqualTo(ik.MaxWristSwingDegrees + 0.5f));
            Object.DestroyImmediate(root);
        }

        private static GameObject BuildArmChain(HandSide side)
        {
            var root = new GameObject("TestArmRoot");
            string prefix = side == HandSide.Right ? "mixamorig:Right" : "mixamorig:Left";
            var shoulder = Child(root.transform, $"{prefix}Shoulder", Vector3.zero);
            var upper = Child(shoulder, $"{prefix}Arm", Vector3.zero);
            var fore = Child(upper, $"{prefix}ForeArm", Vector3.right * 0.3f);
            var hand = Child(fore, $"{prefix}Hand", Vector3.right * 0.3f);

            AddFinger(hand, prefix, "Thumb");
            AddFinger(hand, prefix, "Index");
            AddFinger(hand, prefix, "Middle");
            AddFinger(hand, prefix, "Ring");
            AddFinger(hand, prefix, "Pinky");
            return root;
        }

        private static void AddFinger(Transform parent, string prefix, string finger)
        {
            var last = parent;
            for (int i = 1; i <= 4; i++)
            {
                last = Child(last, $"{prefix}Hand{finger}{i}", Vector3.right * 0.03f);
            }
        }

        private static Transform Child(Transform parent, string name, Vector3 localPosition)
        {
            var child = new GameObject(name).transform;
            child.SetParent(parent, false);
            child.localPosition = localPosition;
            return child;
        }

        private static Transform FingerTip(Transform[] bones)
        {
            var tip = bones[bones.Length - 1];
            return tip.childCount > 0 ? tip.GetChild(0) : tip;
        }

        private static Vector3 MirrorXAndY(Vector3 value)
        {
            return new Vector3(-value.x, -value.y, value.z);
        }

        private static Vector3 PalmCurlNormal(ArmRigReferences rig)
        {
            var index = rig.GetFingerBones(HandFinger.Index);
            var middle = rig.GetFingerBones(HandFinger.Middle);
            var ring = rig.GetFingerBones(HandFinger.Ring);
            var pinky = rig.GetFingerBones(HandFinger.Pinky);
            var forward = FingerForward(index) + FingerForward(middle) + FingerForward(ring) + FingerForward(pinky);
            forward *= 0.25f;

            var across = FingerSpreadAxis(rig);
            forward.Normalize();
            var normal = Vector3.Cross(forward, across);
            normal.Normalize();
            return MirroredPalmNormal(normal, middle, rig);
        }

        private static Vector3 FingerSpreadAxis(ArmRigReferences rig)
        {
            var index = rig.GetFingerBones(HandFinger.Index);
            var pinky = rig.GetFingerBones(HandFinger.Pinky);
            var across = pinky[0].position - index[0].position;
            across.Normalize();
            return across;
        }

        private static Vector3 FingerForward(Transform[] bones)
        {
            return FingerTip(bones).position - bones[0].position;
        }

        private static Vector3 MirroredPalmNormal(Vector3 normal, Transform[] middleBones, ArmRigReferences rig)
        {
            return ChoosePalmSide(normal, middleBones, rig);
        }

        private static Vector3 ChoosePalmSide(Vector3 normal, Transform[] middleBones, ArmRigReferences rig)
        {
            var positiveScore = ScorePalmNormal(normal, middleBones, rig);
            var negativeScore = ScorePalmNormal(-normal, middleBones, rig);
            return positiveScore >= negativeScore ? normal : -normal;
        }

        private static float ScorePalmNormal(Vector3 normal, Transform[] bones, ArmRigReferences rig)
        {
            var origin = bones[1].position;
            var tip = FingerTip(bones).position;
            var segment = tip - origin;
            segment.Normalize();

            var axis = Vector3.Cross(segment, normal);
            axis.Normalize();

            var curledTip = origin + Quaternion.AngleAxis(18f, axis) * (tip - origin);
            return Vector3.Distance(tip, rig.Hand.position) - Vector3.Distance(curledTip, rig.Hand.position);
        }
    }
}
