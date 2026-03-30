using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Serbull.GameAssets.Interact
{
    public class InteractService
    {
        private readonly HashSet<InteractTrigger> _interactTriggers = new();
        private readonly Transform _cameraTransform;

        private IInteractable _activeInteractable;

        public InteractService(Transform cameraTransform)
        {
            _cameraTransform = cameraTransform;
        }

        public void AddInteractTrigger(InteractTrigger interactTrigger)
        {
            if (!_interactTriggers.Contains(interactTrigger))
                _interactTriggers.Add(interactTrigger);

            UpdateInteracts(true);
        }

        public void RemoveInteractTrigger(InteractTrigger interactTrigger)
        {
            if (_interactTriggers.Contains(interactTrigger))
                _interactTriggers.Remove(interactTrigger);

            UpdateInteracts(true);
        }

        public void Update()
        {
            if (_interactTriggers.Count == 0)
                return;

            UpdateInteracts(false);
        }

        public void UpdateInteracts(bool force = false)
        {
            if (force)
            {
                _activeInteractable = null;
            }

            if (_interactTriggers.Count == 0)
            {
                _activeInteractable = null;
                Services.UI.InteractButton.Hide();
                return;
            }

            InteractTrigger interactTrigger = null;

            if (_interactTriggers.Count == 1)
            {
                interactTrigger = _interactTriggers.First();
            }
            else
            {
                float bestDot = -1f;

                foreach (var trigger in _interactTriggers)
                {
                    Vector3 directionToTrigger = (trigger.transform.position - _cameraTransform.position).normalized;
                    float dot = Vector3.Dot(_cameraTransform.forward, directionToTrigger);

                    if (dot > bestDot)
                    {
                        bestDot = dot;
                        interactTrigger = trigger;
                    }
                }
            }

            if (_activeInteractable == interactTrigger.Interactable)
                return;

            if (interactTrigger.Interactable == null)
            {
                Debug.LogError("Interactable is null in InteractTrigger");
                return;
            }

            _activeInteractable = interactTrigger.Interactable;
            Services.UI.InteractButton.Show(_activeInteractable.GetInteractData());
        }
    }
}
