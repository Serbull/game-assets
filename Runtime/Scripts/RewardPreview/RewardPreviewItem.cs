using UnityEngine;

namespace Serbull.GameAssets
{
    [System.Serializable]
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


        public RewardPreviewItem(Sprite icon, int count)
        {
            Name = "";
            Description = "";
            Count = count;
            Icon = icon;
            ShowGlow = true;
            NameColor = Color.white;
            DescriptionColor = Color.white;
            GlowColor = Color.white;
        }

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
