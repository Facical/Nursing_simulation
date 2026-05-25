using UnityEngine;
using UnityEngine.Rendering;

namespace NursingSim.Gameplay.Hand3D
{
    [DisallowMultipleComponent]
    public class FaucetWaterInteraction : MonoBehaviour
    {
        [SerializeField] private Transform streamStart;
        [SerializeField] private Transform streamEnd;
        [SerializeField] private float activationRadius = 0.32f;
        [SerializeField] private float contactIntervalSec = 0.1f;
        [SerializeField] private Color waterColor = new Color(0.35f, 0.78f, 1f, 0.8f);
        [SerializeField] private bool interactionEnabled;

        private LineRenderer waterLine;
        private ParticleSystem droplets;
        private float lastContactTime = -10f;
        private bool flowing;

        private void Awake()
        {
            EnsureAnchors();
            EnsureEffects();
            SetFlowing(false);
        }

        private void Update()
        {
            EnsureAnchors();
            UpdateEffectPositions();
            if (!interactionEnabled)
            {
                SetFlowing(false);
                return;
            }

            var hand = Hand3DController.ActiveInputHand;
            bool hasContact = hand != null && IsPalmUnderFaucet(hand.Palm.position);
            SetFlowing(hasContact);
            if (!hasContact) return;
            if (Time.time - lastContactTime < contactIntervalSec) return;

            lastContactTime = Time.time;
            HandActionEvents.RaiseWaterContacted(transform, ClosestPointOnStream(hand.Palm.position));
        }

        public void SetInteractionEnabled(bool enabled)
        {
            interactionEnabled = enabled;
            if (!interactionEnabled) SetFlowing(false);
        }

        public void Configure(Vector3 startPosition, Vector3 endPosition)
        {
            streamStart = EnsureChild("WaterStreamStart");
            streamEnd = EnsureChild("WaterStreamEnd");
            streamStart.position = startPosition;
            streamEnd.position = endPosition;
            EnsureEffects();
            UpdateEffectPositions();
        }

        public void RefreshVisualFromAnchors()
        {
            EnsureAnchors();
            EnsureEffects();
            UpdateEffectPositions();
            if (!interactionEnabled) SetFlowing(false);
        }

        private bool IsPalmUnderFaucet(Vector3 palmPosition)
        {
            var closest = ClosestPointOnStream(palmPosition);
            return Vector3.Distance(palmPosition, closest) <= activationRadius;
        }

        private Vector3 ClosestPointOnStream(Vector3 point)
        {
            if (streamStart == null || streamEnd == null) return transform.position;
            var start = streamStart.position;
            var end = streamEnd.position;
            var segment = end - start;
            float lengthSq = segment.sqrMagnitude;
            if (lengthSq < 0.0001f) return start;
            float t = Vector3.Dot(point - start, segment) / lengthSq;
            return Vector3.Lerp(start, end, Mathf.Clamp01(t));
        }

        private void SetFlowing(bool value)
        {
            if (flowing == value)
            {
                if (value) UpdateEffectPositions();
                else
                {
                    if (waterLine != null) waterLine.enabled = false;
                    if (droplets != null && droplets.isPlaying)
                    {
                        droplets.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }
                return;
            }

            flowing = value;
            if (waterLine != null) waterLine.enabled = value;
            if (droplets != null)
            {
                if (value) droplets.Play();
                else droplets.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            if (value) UpdateEffectPositions();
        }

        private void EnsureAnchors()
        {
            if (streamStart != null && streamEnd != null) return;
            var start = transform.position + Vector3.up * 0.14f;
            var end = transform.position + Vector3.down * 0.34f;
            Configure(start, end);
        }

        private void EnsureEffects()
        {
            if (waterLine == null)
            {
                var existingLine = transform.Find("WaterStreamVisual");
                var lineGo = existingLine != null ? existingLine.gameObject : new GameObject("WaterStreamVisual");
                if (existingLine == null) lineGo.transform.SetParent(transform, false);
                waterLine = lineGo.GetComponent<LineRenderer>();
                if (waterLine == null) waterLine = lineGo.AddComponent<LineRenderer>();
                waterLine.positionCount = 2;
                waterLine.startWidth = 0.012f;
                waterLine.endWidth = 0.007f;
                waterLine.useWorldSpace = true;
                waterLine.numCapVertices = 4;
                waterLine.numCornerVertices = 2;
                waterLine.textureMode = LineTextureMode.Stretch;
                waterLine.shadowCastingMode = ShadowCastingMode.Off;
                waterLine.receiveShadows = false;
                waterLine.sharedMaterial = CreateWaterMaterial();
                waterLine.colorGradient = CreateWaterGradient();
            }

            if (droplets == null)
            {
                var existingParticles = transform.Find("WaterDroplets");
                var particleGo = existingParticles != null ? existingParticles.gameObject : new GameObject("WaterDroplets");
                if (existingParticles == null) particleGo.transform.SetParent(transform, false);
                droplets = particleGo.GetComponent<ParticleSystem>();
                if (droplets == null) droplets = particleGo.AddComponent<ParticleSystem>();
                var main = droplets.main;
                main.startLifetime = 0.28f;
                main.startSpeed = 0.42f;
                main.startSize = 0.007f;
                main.startColor = waterColor;
                main.gravityModifier = 0.18f;
                var emission = droplets.emission;
                emission.rateOverTime = 28f;
                var shape = droplets.shape;
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = 5f;
                shape.radius = 0.006f;
                var renderer = particleGo.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.receiveShadows = false;
                    renderer.sharedMaterial = CreateWaterMaterial();
                }
            }

            UpdateEffectPositions();
        }

        private void UpdateEffectPositions()
        {
            if (streamStart == null || streamEnd == null) return;
            if (waterLine != null)
            {
                waterLine.SetPosition(0, streamStart.position);
                waterLine.SetPosition(1, streamEnd.position);
            }

            if (droplets != null)
            {
                droplets.transform.position = streamStart.position;
                var direction = streamEnd.position - streamStart.position;
                if (direction.sqrMagnitude > 0.0001f)
                {
                    droplets.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
                }
            }
        }

        private Transform EnsureChild(string childName)
        {
            var child = transform.Find(childName);
            if (child != null) return child;
            child = new GameObject(childName).transform;
            child.SetParent(transform, false);
            return child;
        }

        private Material CreateWaterMaterial()
        {
            var shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            var material = new Material(shader);
            if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", waterColor);
            else material.color = waterColor;
            return material;
        }

        private Gradient CreateWaterGradient()
        {
            var start = waterColor;
            var end = waterColor;
            start.a = 0.78f;
            end.a = 0.18f;

            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(start, 0f),
                    new GradientColorKey(end, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(start.a, 0f),
                    new GradientAlphaKey(end.a, 1f)
                });
            return gradient;
        }

        private void OnDrawGizmosSelected()
        {
            if (streamStart == null || streamEnd == null) return;
            Gizmos.color = waterColor;
            Gizmos.DrawLine(streamStart.position, streamEnd.position);
            Gizmos.DrawWireSphere(ClosestPointOnStream(streamStart.position), activationRadius);
        }
    }
}
