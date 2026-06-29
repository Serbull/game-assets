using UnityEditor;
using UnityEngine;

namespace Serbull.GameAssets.Editor
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (ShowIfEvaluator.Evaluate(property, (ShowIfAttribute)attribute))
            {
                EditorGUI.PropertyField(position, property, label, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return ShowIfEvaluator.Evaluate(property, (ShowIfAttribute)attribute)
                ? EditorGUI.GetPropertyHeight(property, label, true)
                : 0;
        }
    }
}
