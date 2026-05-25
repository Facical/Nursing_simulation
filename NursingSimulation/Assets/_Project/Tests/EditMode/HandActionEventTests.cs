using NursingSim.Data;
using NursingSim.Gameplay;
using NursingSim.Gameplay.Hand3D;
using NUnit.Framework;
using UnityEngine;

namespace NursingSim.Tests
{
    public class HandActionEventTests
    {
        [Test]
        public void HandRubAction_ForwardsToHandActionEvents()
        {
            float rubbed = 0f;
            void OnRubbed(float deltaSec) => rubbed += deltaSec;

            HandActionEvents.Rubbed += OnRubbed;
            try
            {
                HandRubAction.RaiseRubbed(0.25f);
            }
            finally
            {
                HandActionEvents.Rubbed -= OnRubbed;
            }

            Assert.That(rubbed, Is.EqualTo(0.25f).Within(0.0001f));
        }

        [Test]
        public void ToolInteraction3D_CompletesPourOnlyAfterPumpThresholdAndRubDuration()
        {
            var step = ScriptableObject.CreateInstance<ToolInteractionStep>();
            step.stepId = "TEST_HAND_HYGIENE_3D";
            step.title = "손위생";
            step.instruction = "손소독제를 누르고 비비기";
            step.weight = 5;
            step.kind = InteractionKind.Pour;
            step.thresholds.minPumps = 2;
            step.thresholds.minDurationSec = 0.3f;

            var host = new GameObject("ToolInteraction3DStepController_Test");
            var controller = host.AddComponent<ToolInteraction3DStepController>();
            StepResult result = null;
            controller.Completed += r => result = r;

            try
            {
                controller.Begin(step, null);

                HandActionEvents.RaiseRubbed(0.3f);
                Assert.That(result, Is.Null);

                HandActionEvents.RaisePumpPressed(null, Vector3.zero);
                HandActionEvents.RaiseRubbed(0.3f);
                Assert.That(result, Is.Null);

                HandActionEvents.RaisePumpPressed(null, Vector3.zero);
                HandActionEvents.RaiseRubbed(0.1f);
                Assert.That(result, Is.Null);

                HandActionEvents.RaiseRubbed(0.2f);
                Assert.That(result, Is.Not.Null);
                Assert.That(result.stepId, Is.EqualTo("TEST_HAND_HYGIENE_3D"));
                Assert.That(result.earned, Is.EqualTo(5));
                Assert.That(result.criticalFail, Is.False);
            }
            finally
            {
                controller.Abort();
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(step);
            }
        }

        [Test]
        public void ToolInteraction3D_WaterStepRequiresFaucetContactBeforeRubCounts()
        {
            var step = ScriptableObject.CreateInstance<ToolInteractionStep>();
            step.stepId = "TEST_WATER_HAND_HYGIENE_3D";
            step.title = "손위생 #1";
            step.instruction = "물과 비누로 손위생";
            step.weight = 5;
            step.kind = InteractionKind.Pour;
            step.thresholds.minPumps = 2;
            step.thresholds.minDurationSec = 0.3f;
            step.thresholds.requiresWaterContact = true;

            var host = new GameObject("ToolInteraction3DStepController_Water_Test");
            var controller = host.AddComponent<ToolInteraction3DStepController>();
            StepResult result = null;
            controller.Completed += r => result = r;

            try
            {
                controller.Begin(step, null);

                HandActionEvents.RaisePumpPressed(null, Vector3.zero);
                HandActionEvents.RaisePumpPressed(null, Vector3.zero);
                HandActionEvents.RaiseRubbed(0.4f);
                Assert.That(result, Is.Null);

                HandActionEvents.RaiseWaterContacted(null, Vector3.up);
                HandActionEvents.RaiseRubbed(0.3f);

                Assert.That(result, Is.Not.Null);
                Assert.That(result.stepId, Is.EqualTo("TEST_WATER_HAND_HYGIENE_3D"));
                Assert.That(result.earned, Is.EqualTo(5));
            }
            finally
            {
                controller.Abort();
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(step);
            }
        }
    }
}
