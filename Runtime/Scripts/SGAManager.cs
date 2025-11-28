using UnityEngine;

namespace Serbull.GameAssets
{
    public class SGAManager
    {
        private static RewardPreviewPopup _rewardPreviewPopup;
        private static Notification _notification;

        public static void Init(RewardPreviewPopup rewardPreviewPopup, Notification notification)
        {
            _rewardPreviewPopup = rewardPreviewPopup;
            _notification = notification;
        }

        public static RewardPreviewPopup RewardPreviewPopup
        {
            get
            {
                if (_rewardPreviewPopup == null)
                {
                    Debug.LogError("Use SGAInstaller.Init() to initialize or check links in SGAInstaller.");
                    return null;
                }

                return _rewardPreviewPopup;
            }
        }

        public static Notification Notification
        {
            get
            {
                if (_notification == null)
                {
                    Debug.LogError("Use SGAInstaller.Init() to initialize or check links in SGAInstaller.");
                    return null;
                }

                return _notification;
            }
        }
    }
}
