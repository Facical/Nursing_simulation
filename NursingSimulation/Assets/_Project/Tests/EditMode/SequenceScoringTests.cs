using System.Collections.Generic;
using NUnit.Framework;
using NursingSim.Data;
using NursingSim.Gameplay;

namespace NursingSim.Tests.EditMode
{
    public class SequenceScoringTests
    {
        [Test]
        public void EmptyReasons_NotCriticalFail()
        {
            var result = SequenceScoring.ShouldCriticalFail(new List<DeductionReason>(), isCriticalGate: true, explicitFail: false);
            Assert.IsFalse(result);
        }

        [Test]
        public void ExplicitFail_AlwaysCritical()
        {
            var result = SequenceScoring.ShouldCriticalFail(new List<DeductionReason>(), isCriticalGate: false, explicitFail: true);
            Assert.IsTrue(result);
        }

        [Test]
        public void CriticalGateOff_NeverCritical_EvenWithEligibleReason()
        {
            var reasons = new List<DeductionReason> { DeductionReason.BloodSeenButContinued };
            var result = SequenceScoring.ShouldCriticalFail(reasons, isCriticalGate: false, explicitFail: false);
            Assert.IsFalse(result);
        }

        [TestCase(DeductionReason.BloodSeenButContinued)]
        [TestCase(DeductionReason.AspirationSkipped)]
        [TestCase(DeductionReason.AngleOutOfRange)]
        [TestCase(DeductionReason.InjectionTooFast)]
        [TestCase(DeductionReason.InjectionTooSlow)]
        public void EligibleReason_TriggersCritical_WhenGateOn(DeductionReason eligible)
        {
            var reasons = new List<DeductionReason> { eligible };
            var result = SequenceScoring.ShouldCriticalFail(reasons, isCriticalGate: true, explicitFail: false);
            Assert.IsTrue(result, $"reason {eligible} should trigger critical fail when isCriticalGate=true");
        }

        [TestCase(DeductionReason.HandHygieneTooShort)]
        [TestCase(DeductionReason.LandmarkOrderWrong)]
        [TestCase(DeductionReason.RecordFieldMissing)]
        [TestCase(DeductionReason.PrivacyNotSecured)]
        public void NonSequenceReason_DoesNotTriggerCritical(DeductionReason nonEligible)
        {
            var reasons = new List<DeductionReason> { nonEligible };
            var result = SequenceScoring.ShouldCriticalFail(reasons, isCriticalGate: true, explicitFail: false);
            Assert.IsFalse(result, $"reason {nonEligible} belongs to other steps, not Sequence step");
        }

        [Test]
        public void MixedReasons_CriticalIfAnyEligible()
        {
            var reasons = new List<DeductionReason> {
                DeductionReason.HandHygieneTooShort,
                DeductionReason.InjectionTooFast,
                DeductionReason.LandmarkOrderWrong
            };
            var result = SequenceScoring.ShouldCriticalFail(reasons, isCriticalGate: true, explicitFail: false);
            Assert.IsTrue(result);
        }

        [Test]
        public void NullReasons_NotCritical()
        {
            var result = SequenceScoring.ShouldCriticalFail(null, isCriticalGate: true, explicitFail: false);
            Assert.IsFalse(result);
        }
    }
}
