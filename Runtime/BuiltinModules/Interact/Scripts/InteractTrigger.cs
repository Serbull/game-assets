using UnityEngine;

namespace Serbull.GameAssets.Interact
{
    public class InteractTrigger : MonoBehaviour
    {
        private bool _isPlyerInside;

        public IInteractable Interactable;

        private void OnDisable()
        {
            Services.Interact.RemoveInteractTrigger(this);
        }

        private void OnDestroy()
        {
            Services.Interact.RemoveInteractTrigger(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlyerInside = true;
                if (Interactable != null && Interactable.CanInteract)
                {
                    Services.Interact.AddInteractTrigger(this);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                _isPlyerInside = false;
                Services.Interact.RemoveInteractTrigger(this);
            }
        }

        public void SetInteractable(IInteractable interactable)
        {
            Interactable = interactable;
        }

        public void UpdateTrigger()
        {
            if (_isPlyerInside)
            {
                if (Interactable.CanInteract)
                {
                    if (Services.Interact.HasInteractTrigger(this))
                    {
                        Services.Interact.UpdateInteracts(true);
                    }
                    else
                    {
                        Services.Interact.AddInteractTrigger(this);
                    }
                }
                else
                {
                    Services.Interact.RemoveInteractTrigger(this);
                }
            }
        }
    }
}
