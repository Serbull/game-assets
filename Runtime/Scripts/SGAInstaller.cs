using UnityEngine;

namespace Serbull.GameAssets
{
    public class SGAInstaller : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private RewardPreviewPopup _rewardPreviewPopup;
        [SerializeField] private Notification _notification;

        public void Init()
        {
            SGAManager.Init(_rewardPreviewPopup, _notification);
        }
    }
}
