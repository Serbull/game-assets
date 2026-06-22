#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using Serbull.GameAssets.Localization;

namespace Serbull.GameAssets.Editor
{
    [CustomEditor(typeof(LocalizationText))]
    public class LocalizationTextEditor : UnityEditor.Editor
    {
        private SerializedProperty _idProp;

        private LocalizationConfig _config;
        private SerializedObject _configSerialized;
        private SerializedProperty _localizationsProp;

        // Cached lookup so we only scan the config when the id actually changes.
        private string _cachedId;
        private int _entryIndex = -1;

        private GUIStyle _textAreaStyle;
        private GUIStyle TextAreaStyle => _textAreaStyle ??= new GUIStyle(EditorStyles.textArea)
        {
            wordWrap = true
        };

        private void OnEnable()
        {
            _idProp = serializedObject.FindProperty("_id");

            _config = ConfigProvider.LoadConfig<LocalizationConfig>(ConfigProvider.ConfigType.Localization);
            if (_config != null)
            {
                _configSerialized = new SerializedObject(_config);
                _localizationsProp = _configSerialized.FindProperty(nameof(LocalizationConfig.Localizations));
            }

            ResolveEntry(_idProp.stringValue);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(_idProp);
            serializedObject.ApplyModifiedProperties();

            var id = _idProp.stringValue;

            // Re-scan only when the id changed (also catches undo / external edits).
            if (id != _cachedId)
                ResolveEntry(id);

            EditorGUILayout.Space();

            if (_config == null)
            {
                EditorGUILayout.HelpBox("LocalizationConfig not found.", MessageType.Error);
                return;
            }

            if (string.IsNullOrEmpty(id))
            {
                EditorGUILayout.HelpBox("Set a localization id.", MessageType.Warning);
                return;
            }

            if (_entryIndex < 0)
            {
                EditorGUILayout.HelpBox($"No localization found with id \"{id}\".", MessageType.Warning);
                if (GUILayout.Button("Create New"))
                    CreateEntry(id);
                return;
            }

            _configSerialized.Update();
            var element = _localizationsProp.GetArrayElementAtIndex(_entryIndex);
            var enProp = element.FindPropertyRelative(nameof(LocalizationData.En));
            var ruProp = element.FindPropertyRelative(nameof(LocalizationData.Ru));
            DrawLangRow("En", enProp);
            DrawLangRow("Ru", ruProp);
            _configSerialized.ApplyModifiedProperties();
        }

        private void DrawLangRow(string label, SerializedProperty prop)
        {
            float rowHeight = EditorGUIUtility.singleLineHeight * 2f;
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(28f), GUILayout.Height(rowHeight));
                prop.stringValue = EditorGUILayout.TextArea(prop.stringValue, TextAreaStyle,
                    GUILayout.Height(rowHeight), GUILayout.ExpandWidth(true));
                if (GUILayout.Button("Try", GUILayout.Width(40), GUILayout.Height(rowHeight)))
                    ApplyToText(prop.stringValue);
            }
        }

        private void ApplyToText(string value)
        {
            var text = ((LocalizationText)target).GetComponent<TMP_Text>();
            if (text == null)
                return;

            Undo.RecordObject(text, "Apply Localization Text");
            text.text = value;
            EditorUtility.SetDirty(text);
        }

        private void ResolveEntry(string id)
        {
            _cachedId = id;
            _entryIndex = -1;

            if (_config == null || string.IsNullOrEmpty(id))
                return;

            var list = _config.Localizations;
            for (int i = 0; i < list.Length; i++)
            {
                if (list[i] != null && list[i].Id == id)
                {
                    _entryIndex = i;
                    return;
                }
            }
        }

        private void CreateEntry(string id)
        {
            _configSerialized.Update();

            int newIndex = _localizationsProp.arraySize;
            _localizationsProp.arraySize = newIndex + 1;

            var element = _localizationsProp.GetArrayElementAtIndex(newIndex);
            element.FindPropertyRelative(nameof(LocalizationData.Id)).stringValue = id;
            element.FindPropertyRelative(nameof(LocalizationData.En)).stringValue = string.Empty;
            element.FindPropertyRelative(nameof(LocalizationData.Ru)).stringValue = string.Empty;

            _configSerialized.ApplyModifiedProperties();
            _entryIndex = newIndex;
        }
    }
}
#endif
