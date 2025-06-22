using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Serbull.GameAssets
{
    public class RewardPreviewSlot : MonoBehaviour
    {
        [SerializeField] private Image _glow;
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _countText;

        public void Init(RewardPreviewItem item)
        {
            _icon.sprite = item.Icon;

            _nameText.gameObject.SetActive(!string.IsNullOrEmpty(item.Name));
            _nameText.text = item.Name;
            _nameText.color = item.NameColor;

            _descriptionText.gameObject.SetActive(!string.IsNullOrEmpty(item.Description));
            _descriptionText.text = item.Description;
            _descriptionText.color = item.DescriptionColor;

            _countText.text = item.Count.ToShortValue();
            _countText.gameObject.SetActive(item.Count > 1);

            _glow.gameObject.SetActive(item.ShowGlow);
            _glow.color = item.GlowColor;
        }
    }
}
