using UnityEngine;
using UnityEngine.UI;

namespace Serbull.GameAssets.PlaytimeReward
{
    public class PlaytimeRewardPopup : Popup
    {
        [SerializeField] private PlaytimeRewardSlot _slotPrefab;
        [SerializeField] private Transform _content;
        [SerializeField] private Button _closeButton;

        private void Awake()
        {
            _closeButton.onClick.AddListener(CloseButton_OnClick);
        }

        private void OnEnable()
        {
            foreach (Transform child in _content)
            {
                Destroy(child.gameObject);
            }

            var service = Services.PlaytimeReward;
            if (service == null) return;

            var config = service.Config;

            for (int i = 0; i < config.entries.Length; i++)
            {
                var slot = Instantiate(_slotPrefab, _content);
                slot.Init(config.GetEntry(i), i, service.RewardIsClaimed(i));
            }
        }

        private void CloseButton_OnClick()
        {
            gameObject.SetActive(false);
        }
    }
}
