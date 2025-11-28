using UnityEngine;

namespace Serbull.GameAssets.Samples
{
    public partial class SceneContext : MonoBehaviour
    {
        [SerializeField] private SGAInstaller _sgaInstaller;

        private void Awake()
        {
            _sgaInstaller.Init();
        }
    }
}
