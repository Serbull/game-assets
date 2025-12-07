using UnityEditor;
using UnityEngine;
using System.Reflection;

namespace Serbull.GameAssets.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Object), true)]
    public class ButtonDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            DrawButtons(target);
        }

        private void DrawButtons(Object targetObj)
        {
            if (targetObj == null) return;

            var methods = targetObj
                .GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (var method in methods)
            {
                var button = method.GetCustomAttribute<ButtonAttribute>();
                if (button == null)
                    continue;

                EditorGUILayout.Space(button.Space);

                string label = string.IsNullOrEmpty(button.Label)
                    ? method.Name
                    : button.Label;

                if (GUILayout.Button(label))
                    method.Invoke(targetObj, null);
            }
        }
    }
}
