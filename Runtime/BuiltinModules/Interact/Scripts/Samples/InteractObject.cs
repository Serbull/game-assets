using System.Collections;
using UnityEngine;

namespace Serbull.GameAssets.Samples
{
    public class InteractObject : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(ActivateInteractButton());
        }

        private void OnInteract()
        {
            Debug.Log("Interacted with " + gameObject.name);
            StartCoroutine(ActivateInteractButton());
        }

        private IEnumerator ActivateInteractButton()
        {
            yield return new WaitForSeconds(1f);
            Services.UI.InteractButton.Show(transform, Vector3.up * 1.5f, OnInteract, "Test interact", 0.5f);
        }
    }
}
