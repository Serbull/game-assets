using UnityEngine;

namespace Serbull.GameAssets.Interact
{
    public class InteractTrigger : MonoBehaviour
    {
        public IInteractable Interactable;

        private void OnDestroy()
        {
            Services.InteractService.RemoveInteractTrigger(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Services.InteractService.AddInteractTrigger(this);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                Services.InteractService.RemoveInteractTrigger(this);
            }
        }

        public void SetInteractable(IInteractable interactable)
        {
            Interactable = interactable;
        }
    }
}
