
namespace Serbull.GameAssets.Interact
{
    public interface IInteractable
    {
        bool CanInteract { get; }
        InteractData GetInteractData();
    }
}
