using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Serbull.GameAssets.Editor
{
    /// <summary>
    /// Shared evaluation logic for <see cref="ShowIfAttribute"/> so it can be honored
    /// both by <see cref="ShowIfDrawer"/> and by other PropertyDrawers that already own
    /// the field (Unity applies only a single PropertyDrawer per field).
    /// </summary>
    public static class ShowIfEvaluator
    {
        /// <summary>
        /// Returns true if the field has no <see cref="ShowIfAttribute"/>, or if its
        /// condition currently evaluates to true.
        /// </summary>
        public static bool ShouldShow(SerializedProperty property, FieldInfo fieldInfo)
        {
            ShowIfAttribute showIf = fieldInfo != null
                ? fieldInfo.GetCustomAttribute<ShowIfAttribute>()
                : null;

            return showIf == null || Evaluate(property, showIf);
        }

        public static bool Evaluate(SerializedProperty property, ShowIfAttribute showIf)
        {
            return EvaluateCondition(property, showIf) != showIf.Inverted;
        }

        private static bool EvaluateCondition(SerializedProperty property, ShowIfAttribute showIf)
        {
            // Resolve the condition relative to the property's parent so it also
            // works for fields nested inside serializable classes / array elements.
            SerializedProperty conditionProperty = FindSiblingProperty(property, showIf.ConditionName);

            if (conditionProperty != null)
            {
                return conditionProperty.boolValue;
            }

            Object target = property.serializedObject.targetObject;
            MethodInfo method = target.GetType().GetMethod(showIf.ConditionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method != null && method.ReturnType == typeof(bool))
            {
                return (bool)method.Invoke(target, null);
            }

            Debug.LogWarning($"[ShowIf] Condition \"{showIf.ConditionName}\" not found for {property.propertyPath} in {target.GetType().Name}");
            // Return the value that, after inversion, results in the field being shown,
            // so a misconfigured condition never silently hides the field.
            return showIf.Inverted;
        }

        private static SerializedProperty FindSiblingProperty(SerializedProperty property, string siblingName)
        {
            int dot = property.propertyPath.LastIndexOf('.');
            if (dot < 0)
            {
                return property.serializedObject.FindProperty(siblingName);
            }

            string parentPath = property.propertyPath.Substring(0, dot);
            return property.serializedObject.FindProperty($"{parentPath}.{siblingName}");
        }
    }
}
