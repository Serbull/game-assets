#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Serbull.GameAssets.Localization
{
    [CustomEditor(typeof(LocalizationConfig))]
    public class LocalizationConfigEditor : UnityEditor.Editor
    {
        private LocalizationConfig _source;
        private bool _overwriteExisting;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Merge", EditorStyles.boldLabel);

            _source = (LocalizationConfig)EditorGUILayout.ObjectField(
                "Source config", _source, typeof(LocalizationConfig), false);

            _overwriteExisting = EditorGUILayout.ToggleLeft(
                "Overwrite existing ids (otherwise only add missing)", _overwriteExisting);

            using (new EditorGUI.DisabledScope(_source == null || _source == target))
            {
                if (GUILayout.Button("Merge missing localizations"))
                {
                    Merge((LocalizationConfig)target, _source, _overwriteExisting);
                }
            }
        }

        private static void Merge(LocalizationConfig target, LocalizationConfig source, bool overwriteExisting)
        {
            var list = new List<LocalizationData>(target.Localizations ?? new LocalizationData[0]);
            var byId = list
                .Where(d => !string.IsNullOrEmpty(d.Id))
                .GroupBy(d => d.Id)
                .ToDictionary(g => g.Key, g => g.First());

            int added = 0;
            int updated = 0;

            foreach (var src in source.Localizations ?? new LocalizationData[0])
            {
                if (string.IsNullOrEmpty(src.Id))
                    continue;

                if (byId.TryGetValue(src.Id, out var existing))
                {
                    if (overwriteExisting)
                    {
                        existing.En = src.En;
                        existing.Ru = src.Ru;
                        updated++;
                    }
                }
                else
                {
                    var copy = new LocalizationData { Id = src.Id, En = src.En, Ru = src.Ru };
                    list.Add(copy);
                    byId[copy.Id] = copy;
                    added++;
                }
            }

            target.Localizations = list.ToArray();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();

            Debug.Log($"[Localization merge] Added {added}, updated {updated}, total {target.Localizations.Length}.");
        }
    }
}
#endif
