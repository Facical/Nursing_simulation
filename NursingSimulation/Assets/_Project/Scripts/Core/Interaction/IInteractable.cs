namespace NursingSim.Core.Interaction
{
    public interface IInteractable
    {
        string Id { get; }
        string DisplayName { get; }
        void OnHoverEnter();
        void OnHoverExit();
        void OnClick();
    }
}
