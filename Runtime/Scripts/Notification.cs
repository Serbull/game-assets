using TMPro;
using UnityEngine;

namespace Serbull.GameAssets
{
    public class Notification : Singleton<Notification>
    {
        [SerializeField] private TextMeshProUGUI _text;

        private void OnEnable()
        {
            Invoke(nameof(Hide), 2f);
        }

        public void ShowGreen(string message)
        {
            CancelInvoke();
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            _text.text = message;
            _text.color = Color.green;
        }

        public void ShowRed(string message)
        {
            CancelInvoke();
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            _text.text = message;
            _text.color = Color.red;
        }

        private void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
