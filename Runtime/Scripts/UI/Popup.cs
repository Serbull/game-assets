using System;
using UnityEngine;

namespace Serbull.GameAssets
{
    public class Popup : MonoBehaviour
    {
        public event Action OnShown;
        public event Action OnHidden;
        public void Show()
        {
            gameObject.SetActive(true);
            OnShown?.Invoke();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            OnHidden?.Invoke();
        }
    }
}
