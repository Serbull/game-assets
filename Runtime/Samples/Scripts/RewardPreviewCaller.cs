using UnityEngine;

namespace Serbull.GameAssets.Samples
{
    public class RewardPreviewCaller : MonoBehaviour
    {
        [SerializeField] private RewardPreviewItem[] _items;

        [Button("Show preview popup")]
        private void ShowPreviewPopup()
        {
            if (!Application.isPlaying) return;
            Services.UI.RewardPreviewPopup.Show(_items);
        }
    }
}
