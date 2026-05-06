namespace NursingSim.Data
{
    // Source of truth: docs/05-data-model.md §4
    // Grounded in KABONE 핵심기본간호술 평가항목 프로토콜 제4.1판 항목 #3 (근육주사)
    public enum DeductionReason
    {
        HandHygieneSkipped,
        HandHygieneTooShort,
        RequiredItemMissing,
        DistractorItemSelected,
        DoseOutOfTolerance,
        AsepticBreach,
        ClosedQuestionOnly,
        OneIdentifierOnly,
        RegistrationNumberNotChecked,
        PrivacyNotSecured,
        LandmarkOrderWrong,
        DisinfectionPathWrong,
        DisinfectionDryTimeShort,
        AngleOutOfRange,
        AspirationSkipped,
        BloodSeenButContinued,
        InjectionTooFast,
        InjectionTooSlow,
        WithdrawalAngleDiff,
        NeedleRecapped,
        SharpsBinMissed,
        RecordFieldMissing
    }
}
