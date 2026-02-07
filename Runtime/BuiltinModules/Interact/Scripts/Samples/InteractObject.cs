using System.Collections;
using UnityEngine;

namespace Serbull.GameAssets
{
    public class InteractObject : MonoBehaviour
    {
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(1f);
            Services.UI.InteractButton.Show(transform, Vector3.up * 1.5f, "Interact", OnInteract, 0f);
        }

        private void OnInteract()
        {
            Debug.Log("Interacted with " + gameObject.name);
        }
    }
}
