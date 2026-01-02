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

            _sgaInstaller.Init(resourceGiver, saveData.RouletteData, "en");
        }
    }
}
