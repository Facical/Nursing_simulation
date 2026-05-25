using UnityEngine;

namespace NursingSim.Gameplay.Hand3D
{
    [DisallowMultipleComponent]
    public class FirstPersonHandCameraRig : MonoBehaviour
    {
        [SerializeField] private Transform cameraPose;
        [SerializeField] private Transform leftShoulderAnchor;
        [SerializeField] private Transform rightShoulderAnchor;
        [SerializeField] private Transform lookTarget;
        [SerializeField] private bool applyToMainCameraOnStart;
        [SerializeField] private Vector3 leftShoulderLocalPosition = new Vector3(-0.34f, -0.26f, 0.62f);
        [SerializeField] private Vector3 rightShoulderLocalPosition = new Vector3(0.34f, -0.26f, 0.62f);
        [SerializeField] private Vector3 seatedCameraLocalOffset = new Vector3(0f, -0.32f, 0f);

        private bool seated;

        public Transform CameraPose => cameraPose;
        public Transform LeftShoulderAnchor => leftShoulderAnchor;
        public Transform RightShoulderAnchor => rightShoulderAnchor;
        public Transform LookTarget => lookTarget;
        public bool IsSeated => seated;

        private void Start()
        {
            if (applyToMainCameraOnStart) ApplyToMainCamera();
        }

        public void Configure(Vector3 cameraPosition, Vector3 lookAtPosition)
        {
            cameraPose = EnsureChild("CameraPose");
            lookTarget = EnsureChild("LookTarget");
            leftShoulderAnchor = EnsureChild("LeftShoulderAnchor");
            rightShoulderAnchor = EnsureChild("RightShoulderAnchor");

            transform.position = cameraPosition;
            transform.rotation = Quaternion.LookRotation((lookAtPosition - cameraPosition).normalized, Vector3.up);
            cameraPose.localPosition = Vector3.zero;
            cameraPose.localRotation = Quaternion.identity;
            lookTarget.position = lookAtPosition;
            seated = false;
            ApplyShoulderAnchorOffsets();
        }

        public void ApplyShoulderAnchorOffsets()
        {
            if (leftShoulderAnchor != null) leftShoulderAnchor.localPosition = leftShoulderLocalPosition;
            if (rightShoulderAnchor != null) rightShoulderAnchor.localPosition = rightShoulderLocalPosition;
        }

        public void ApplyToMainCamera()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null || cameraPose == null) return;

            ApplyCameraPoseOffset();
            mainCamera.transform.position = cameraPose.position;
            mainCamera.transform.rotation = cameraPose.rotation;
        }

        public void SetSeated(bool value)
        {
            seated = value;
            ApplyCameraPoseOffset();
        }

        public void ToggleSeated()
        {
            SetSeated(!seated);
        }

        private void ApplyCameraPoseOffset()
        {
            if (cameraPose != null) cameraPose.localPosition = seated ? seatedCameraLocalOffset : Vector3.zero;
        }

        private Transform EnsureChild(string childName)
        {
            var child = transform.Find(childName);
            if (child != null) return child;

            child = new GameObject(childName).transform;
            child.SetParent(transform, false);
            return child;
        }
    }
}
