using UnityEngine;
using UnityEngine.UI;

namespace Serbull.GameAssets.PlaytimeGift
{
    public class GiftPopup : MonoBehaviour
    {
        [SerializeField] private GiftSlot _giftSlotPrefab;
        [SerializeField] private Transform _content;
        [SerializeField] private Button _closeButton;

        private void Awake()
        {
            _closeButton.onClick.AddListener(CloseButton_OnClick);
        }

        private void OnEnable()
        {
            var config = Services.PlaytimeGift.Config;

            foreach (Transform child in _content)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < config.Datas.Length; i++)
            {
                var slot = Instantiate(_giftSlotPrefab, _content);
                slot.Init(config.GetReward(i), i, GiftManager.ClaimedList.Contains(i));
            }
        }

        private void CloseButton_OnClick()
        {
            gameObject.SetActive(false);
        }
    }
}
