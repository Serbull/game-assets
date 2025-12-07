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
            EventBus.Subscribe<ChangeLuckySpinEvent>(OnLuckySpinChanged);
        }

        private void OnDisable()
        {
            EventBus.Unsubscribe<ChangeLuckySpinEvent>(OnLuckySpinChanged);
        }

        private void OnLuckySpinChanged(ChangeLuckySpinEvent e)
        {
            _ntf.SetActive(e.Amount > 0);
        }

        private void Button_OnClick()
        {
            Services.UI.RoulettePopup.gameObject.SetActive(true);
        }
    }
}
