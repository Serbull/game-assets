using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Serbull.GameAssets.DailyReward
{
    [RequireComponent(typeof(Button))]
    public class RewardSlot : MonoBehaviour
    {
        [SerializeField] private Image _bgImage;
        [SerializeField] private Localization.LocalizationText _dayText;
        [SerializeField] private Image _mainImage;
        [SerializeField] private TextMeshProUGUI _countText;
        [SerializeField] private GameObject _claimObject;
        [SerializeField] private Image _claimedObject;

        public void Init(Sprite bgSprite, Color bgColor, int day, Sprite icon, int count, bool isAvailable, bool isClaimed, UnityAction callback)
        {
            if (bgSprite != null) _bgImage.sprite = bgSprite;
            _bgImage.color = bgColor;

            _dayText.SetArguments(day);

            _mainImage.sprite = icon;

            _countText.gameObject.SetActive(count > 1);
            _countText.text = $"x{count}";

            if (bgSprite != null) _claimedObject.sprite = bgSprite;
            _claimObject.SetActive(isAvailable && !isClaimed);
            _claimedObject.gameObject.SetActive(isClaimed);

            var btn = GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(callback);
        }
    }
}
