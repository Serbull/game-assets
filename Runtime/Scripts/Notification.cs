using TMPro;
using UnityEngine;

namespace Serbull.GameAssets
{
    public class Notification : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;

        private void OnEnable()
        {
            Invoke(nameof(Hide), 2f);
        }

        public void Show(string message, Color color)
        {
            CancelInvoke();
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            _text.text = message;
            _text.color = color;
        }

        public void Show(string message) => Show(message, Color.white);
        public void ShowGreen(string message) => Show(message, Color.green);
        public void ShowRed(string message) => Show(message, Color.red);

        private void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
