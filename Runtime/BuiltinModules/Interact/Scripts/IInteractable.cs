
namespace Serbull.GameAssets.Interact
{
    public interface IInteractable
    {
        bool CanInteract { get; }
        TriggerInteractData GetInteractData();
        void Interact();
    }
}
