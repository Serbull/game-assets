using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Serbull.GameAssets.Rarity;

namespace Serbull.GameAssets.Editor
{
    [CustomPropertyDrawer(typeof(RarityDropdownAttribute))]
    public class RarityDropdownDrawer : PropertyDrawer
    {
        private RarityData[] _rares;
        private string[] _rareHeaders;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!ShowIfEvaluator.ShouldShow(property, fieldInfo))
            {
                return;
            }

            if (_rares == null)
            {
                CacheRares();
            }

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "Use [RareDropdown] with a string.");
                return;
            }

            if (_rares == null || _rares.Length == 0)
            {
                EditorGUI.LabelField(position, label.text, "No rare in config");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);

            bool hasMixed = property.hasMultipleDifferentValues;
            EditorGUI.showMixedValue = hasMixed;

            int currentIndex = Array.IndexOf(_rareHeaders, property.stringValue);
            if (currentIndex < 0) currentIndex = 0;

            var oldColor = GUI.contentColor;

            if (!hasMixed && currentIndex >= 0 && currentIndex < _rares.Length)
                GUI.contentColor = _rares[currentIndex].Color;

            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUI.Popup(position, label.text, currentIndex, _rareHeaders);
            if (EditorGUI.EndChangeCheck())
            {
                property.stringValue = _rareHeaders[newIndex];
            }

            GUI.contentColor = oldColor;
            EditorGUI.showMixedValue = false;

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ShowIfEvaluator.ShouldShow(property, fieldInfo)
                ? base.GetPropertyHeight(property, label)
                : 0;
        }

        private void CacheRares()
        {
            var config = ConfigProvider.LoadConfig<Rarity.RarityConfig>(ConfigProvider.ConfigType.Rarity);
            if (config != null && config.Rarities != null)
            {
                _rareHeaders = config.Rarities.Select((i) => i.Id).ToArray();
                _rares = config.Rarities;
            }
        }
    }
}
