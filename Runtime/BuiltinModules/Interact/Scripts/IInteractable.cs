using UnityEngine;

namespace Serbull.GameAssets.Interact
{
    public interface IInteractable
    {
        InteractData GetInteractData();
        void Interact();
    }
}
