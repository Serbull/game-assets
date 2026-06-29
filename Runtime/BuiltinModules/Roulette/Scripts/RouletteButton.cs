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
            if (Services.IsInitialized) OnServicesInitialized();
            else Services.OnInitialized += OnServicesInitialized;
        }

        private void OnDisable()
        {
            Services.OnInitialized -= OnServicesInitialized;

            if (Services.Roulette != null)
                Services.Roulette.OnSpinCountChanged -= UpdateNotification;
        }

        private void OnServicesInitialized()
        {
            Services.OnInitialized -= OnServicesInitialized;

            if (Services.Roulette != null)
                Services.Roulette.OnSpinCountChanged += UpdateNotification;

            UpdateNotification();
        }

        private void UpdateNotification()
        {
            var service = Services.Roulette;
            _ntf.SetActive(service != null && service.SpinCount > 0);
        }

        private void Button_OnClick()
        {
            Services.UI.RoulettePopup.Show();
        }
    }
}
