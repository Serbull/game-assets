using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Serbull.GameAssets.Roulette
{
    public class RouletteSlot : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _countTxt;
        [SerializeField] private TextMeshProUGUI _percentTxt;

        public void Init(RouletteConfig.PartData partData)
        {
            _icon.sprite = partData.reward.icon;
            _countTxt.text = partData.reward.count.ToShortValue();
            _percentTxt.text = partData.weight + "%";
        }
    }
}
