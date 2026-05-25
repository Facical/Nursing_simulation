using System.Collections.Generic;
using NursingSim.Data;

namespace NursingSim.Gameplay
{
    public static class SequenceScoring
    {
        // KABONE 13/14 핵심항목 위반에 해당하는 감점 사유.
        // 본 시뮬레이터의 SequenceStep이 isCriticalGate=true일 때 이 중 하나라도
        // 누적되면 해당 step은 critical fail로 처리된다.
        public static readonly HashSet<DeductionReason> CriticalEligibleReasons = new HashSet<DeductionReason> {
            DeductionReason.BloodSeenButContinued,
            DeductionReason.AspirationSkipped,
            DeductionReason.AngleOutOfRange,
            DeductionReason.InjectionTooFast,
            DeductionReason.InjectionTooSlow,
        };

        public static bool ShouldCriticalFail(IReadOnlyList<DeductionReason> reasons, bool isCriticalGate, bool explicitFail)
        {
            if (explicitFail) return true;
            if (!isCriticalGate || reasons == null) return false;
            for (int i = 0; i < reasons.Count; i++) {
                if (CriticalEligibleReasons.Contains(reasons[i])) return true;
            }
            return false;
        }
    }
}
