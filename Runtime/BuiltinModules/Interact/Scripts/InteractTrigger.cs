using UnityEngine;

namespace Serbull.GameAssets.Interact
{
    public class InteractTrigger : MonoBehaviour
    {
        private bool _isPlyerInside;

        public IInteractable Interactable;

        private void OnDisable()
        {
            Services.InteractService.RemoveInteractTrigger(this);
        }

        private void OnDestroy()
        {
            Services.InteractService.RemoveInteractTrigger(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlyerInside = true;
                if (Interactable != null && Interactable.CanInteract)
                {
                    Services.InteractService.AddInteractTrigger(this);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlyerInside = false;
                Services.InteractService.RemoveInteractTrigger(this);
            }
        }

        public void SetInteractable(IInteractable interactable)
        {
            Interactable = interactable;
        }

        public void UpdateTrigger()
        {
            Services.InteractService.UpdateInteracts(true);
        }
    }
}
