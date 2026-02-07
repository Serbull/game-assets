using UnityEngine;

namespace Serbull.GameAssets.Interact
{
    public interface IInteractable
    {
        Transform InteractObject { get; }
        Vector3 InteractOffset { get; }
        string GetInteractText();
        void Interact();
    }
}
