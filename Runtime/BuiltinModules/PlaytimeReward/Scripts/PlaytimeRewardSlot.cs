using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Serbull.GameAssets.PlaytimeReward
{
    public class PlaytimeRewardSlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _timeTxt;
        [SerializeField] private TextMeshProUGUI _countTxt;
        [SerializeField] private Button _button;
        [SerializeField] private GameObject _doneMark;
        [Space]
        [SerializeField] private Color _grayColor;
        [SerializeField] private Color _greenColor;

        private int _id;
        private bool _isDone;
        private float _timeLeft;

        private void Awake()
        {
            _button.onClick.AddListener(Button_OnClick);
        }

        public void Init(RewardConfig.Entry rewardData, int id, bool isDone)
        {
            _id = id;
            _isDone = isDone;
            var totalTime = rewardData.time; /// (VipValues.IsBought() ? 2 : 1);
            _timeLeft = totalTime - Time.time;
            _icon.sprite = rewardData.reward.icon;
            _countTxt.text = "x" + rewardData.reward.count.ToShortValue();
            _doneMark.SetActive(isDone);
            var bgColor = Services.Rarity.GetRarityData(rewardData.color).Color;
            GetComponent<Image>().color = bgColor;
            UpdateTakeButton();
            StartCoroutine(Timer());
        }

        private IEnumerator Timer()
        {
            while (!_isDone && _timeLeft > 0)
            {
                UpdateTimer();
                yield return new WaitForSecondsRealtime(1);
                _timeLeft -= 1;
            }

            UpdateTakeButton();
        }

        private void Button_OnClick()
        {
            if (_timeLeft <= 0)
            {
                _isDone = true;
                Services.PlaytimeReward.ClaimReward(_id);
                UpdateTakeButton();
                _doneMark.SetActive(true);
            }
        }

        private void UpdateTimer()
        {
            int minutes = Mathf.FloorToInt(_timeLeft / 60);
            int seconds = Mathf.FloorToInt(_timeLeft % 60);

            _timeTxt.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        private void UpdateTakeButton()
        {
            _button.gameObject.SetActive(!_isDone);

            if (!_isDone)
            {
                _button.GetComponent<Image>().color = _timeLeft <= 0 ? _greenColor : _grayColor;

                if (_timeLeft <= 0)
                {
                    _timeTxt.text = Services.Localization.GetText("claim");
                }
            }
        }
    }
}
