using UnityEngine;

namespace Serbull.GameAssets.Interact
{
    public interface IInteractable
    {
        Transform GetInteractObject(out Vector3 interactOffset);
        string GetInteractText();
        void Interact();
    }
}
