namespace NursingSim.Data
{
    public enum ScenarioLevel
    {
        Easy,
        Normal,
        Hard
    }

    public enum StepCategory
    {
        Checklist,
        ToolInteraction,
        Dialogue,
        Selection,
        LandmarkPick,
        Sequence,
        ToggleGroup
    }

    public enum FeedbackTiming
    {
        Instant,
        Deferred
    }

    public enum InteractionKind
    {
        Click,
        DragPath,
        Pour
    }

    public enum FeedbackKind
    {
        Info,
        Warning,
        Error,
        Success
    }
}
