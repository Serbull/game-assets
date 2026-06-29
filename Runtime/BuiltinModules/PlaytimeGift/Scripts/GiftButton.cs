using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Serbull.GameAssets.PlaytimeReward
{
    public class GiftButton : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private GameObject _ntf;

        private float _lastUpdateTime = float.MinValue;
        private float _timeLeft;

        private GiftConfig _config;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Button_OnClick);
        }

        private void Start()
        {
            _config = Services.PlaytimeGift.Config;

            UpdateTimeLeft();
        }

        private void Button_OnClick()
        {
            Services.UI.PlaytimeGiftPopup.gameObject.SetActive(true);
        }

        private void Update()
        {
            if (Time.time - _lastUpdateTime >= 1f)
            {
                _lastUpdateTime = Time.time;

                UpdateTimeLeft();

                _ntf.SetActive(_timeLeft <= 0);

                if (_timeLeft > 0)
                {
                    int minutes = Mathf.FloorToInt(_timeLeft / 60);
                    int seconds = Mathf.FloorToInt(_timeLeft % 60);
                    _text.text = string.Format("{0:00}:{1:00}", minutes, seconds);
                }
                else
                {
                    _text.text = Services.Localization.GetText("ready");
                }
            }
        }

        private void UpdateTimeLeft()
        {
            var nearestGift = Services.PlaytimeGift.GetNearestGiftId();
            if (nearestGift >= 0)
            {
                var totalTime = _config.GetReward(nearestGift).Time;
                _timeLeft = totalTime - Time.time;
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}
