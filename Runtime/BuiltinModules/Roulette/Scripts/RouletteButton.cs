using UnityEngine;
using UnityEngine.UI;

namespace Serbull.GameAssets.Roulette
{
    public class RouletteButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _ntf;

        private void Awake()
        {
            _button.onClick.AddListener(Button_OnClick);
        }

        private void OnEnable()
        {
            if (Services.Roulette != null)
            {
                Services.Roulette.OnSpinCountChanged += OnSpinCountChanged;
                OnSpinCountChanged(Services.Roulette.SaveData.SpinCount);
            }
        }

        private void OnDisable()
        {
            if (Services.Roulette != null)
            {
                Services.Roulette.OnSpinCountChanged -= OnSpinCountChanged;
            }
        }

        private void OnSpinCountChanged(int amount)
        {
            _ntf.SetActive(amount > 0);
        }

        private void Button_OnClick()
        {
            Services.UI.RoulettePopup.gameObject.SetActive(true);
        }
    }
}
