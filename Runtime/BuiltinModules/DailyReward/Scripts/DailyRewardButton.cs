using UnityEngine;
using UnityEngine.UI;

namespace Serbull.GameAssets.DailyReward
{
    [RequireComponent(typeof(Button))]
    public class RewardButton : MonoBehaviour
    {
        [SerializeField] private GameObject _notification;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(Button_OnClick);
        }

        private void OnEnable()
        {
            if (Services.IsInitialized) OnServicesInitialized();
            else Services.OnInitialized += OnServicesInitialized;
        }

        private void OnDisable()
        {
            Services.OnInitialized -= OnServicesInitialized;

            if (Services.DailyRewardService != null)
                Services.DailyRewardService.OnUpdated -= UpdateNotification;
        }

        private void Button_OnClick()
        {
            Services.UI.DailyRewardPopup.Show();
        }

        private void OnServicesInitialized()
        {
            Services.OnInitialized -= OnServicesInitialized;

            if (Services.DailyRewardService != null)
                Services.DailyRewardService.OnUpdated += UpdateNotification;

            UpdateNotification();
        }

        private void UpdateNotification()
        {
            var service = Services.DailyRewardService;
            _notification.SetActive(service != null && service.HasRewardToClaim());
        }
    }
}
