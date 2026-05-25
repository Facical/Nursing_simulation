using UnityEngine;

namespace NursingSim.Gameplay.Hand3D
{
    public enum ToolKind
    {
        None,
        Pump,
        Syringe,
        Ampoule,
        AlcoholSwab,
        Gauze,
        SharpsContainer,
    }

    [RequireComponent(typeof(Rigidbody))]
    public class GrabbablePhysicalTool : MonoBehaviour
    {
        [SerializeField] private ToolKind kind = ToolKind.None;
        [SerializeField] private Transform attachPoint;
        [Tooltip("Optional: when grabbed, snap rotation so attachPoint matches the hand's palm orientation.")]
        [SerializeField] private bool snapOrientationOnGrab = true;

        private Rigidbody body;
        private Transform originalParent;
        private FixedJoint joint;

        public ToolKind Kind => kind;
        public bool IsHeld { get; private set; }
        public Transform AttachPoint => attachPoint != null ? attachPoint : transform;

        private void Awake()
        {
            body = GetComponent<Rigidbody>();
            originalParent = transform.parent;
        }

        public void AttachToHand(Rigidbody handBody)
        {
            if (IsHeld || handBody == null) return;
            if (snapOrientationOnGrab && attachPoint != null)
            {
                var rotOffset = attachPoint.localRotation;
                transform.rotation = handBody.transform.rotation * Quaternion.Inverse(rotOffset);
                transform.position = handBody.transform.position - (transform.rotation * attachPoint.localPosition);
            }
            joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = handBody;
            joint.breakForce = Mathf.Infinity;
            joint.breakTorque = Mathf.Infinity;
            IsHeld = true;
        }

        public void DetachFromHand()
        {
            if (!IsHeld) return;
            if (joint != null) Destroy(joint);
            joint = null;
            IsHeld = false;
        }
    }
}
