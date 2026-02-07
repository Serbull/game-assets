using UnityEngine;

namespace Serbull.GameAssets.Interact
{
    public interface IInteractable
    {
        Transform GetInteractPoint();
        string GetInteractText();
        void Interact();
    }
}
