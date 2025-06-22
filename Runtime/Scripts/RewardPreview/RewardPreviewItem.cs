using UnityEngine;

namespace Serbull.GameAssets
{
    public struct RewardPreviewItem
    {
        public int Count;
        public string Name;
        public string Description;
        public Sprite Icon;
        public bool ShowGlow;
        public Color NameColor;
        public Color DescriptionColor;
        public Color GlowColor;

        public RewardPreviewItem(string name, string description, Sprite icon, int count, bool showGlow, Color nameColor, Color descriptionColor, Color glowColor)
        {
            Name = name;
            Description = description;
            Count = count;
            Icon = icon;
            ShowGlow = showGlow;
            NameColor = nameColor;
            DescriptionColor = descriptionColor;
            GlowColor = glowColor;
        }
    }
}
