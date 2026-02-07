using UnityEngine;

namespace Serbull.GameAssets.Interact
{
    public interface IInteractable
    {
        public bool CanInteract { get; }
        InteractData GetInteractData();
        void Interact();
    }
}
