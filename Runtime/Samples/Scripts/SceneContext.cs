using UnityEngine;

namespace Serbull.GameAssets.Samples
{
    public partial class SceneContext : MonoBehaviour
    {
        [SerializeField] private SGAInstaller _sgaInstaller;

        private void Awake()
        {
            var saveData = new SaveData();
            var resourceGiver = new ResourceGiver();
            var purchaseService = new PurchaseService();

            _sgaInstaller.Init(resourceGiver, purchaseService, saveData.Roulette, saveData.DailyReward, false, "en");
        }
    }
}
