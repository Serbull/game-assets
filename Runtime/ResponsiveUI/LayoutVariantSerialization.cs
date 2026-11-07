using System.Collections.Generic;
using UnityEngine;

namespace Serbull.ResponsiveUI
{
    /// <summary>
    /// Снимает и накатывает снапшоты компонентов.
    ///
    /// Формат снапшота — плоский список пар "propertyPath = значение", снятый
    /// через SerializedObject. Так покрываются все сериализованные поля любого
    /// компонента (GridLayoutGroup, LayoutElement, Image, TMP_Text...) без
    /// ручного перечисления, и при этом мы полностью контролируем, какие поля
    /// попадают в снапшот.
    ///
    /// ГОТЧА, ради которой не используется JsonUtility: у Transform/RectTransform
    /// он сериализует служебные поля иерархии (m_Children, m_Father, m_GameObject).
    /// Накат такого снапшота перевешивает родителя и рвёт иерархию. Здесь такие
    /// поля просто не попадают в снапшот — см. ForbiddenPaths.
    ///
    /// Захват возможен только в Editor. Применение работает и в билде: значения
    /// хранятся строками и раскладываются рефлексией по SerializedProperty-путям
    /// в Editor, а в рантайме — через кэш reflection-сеттеров.
    /// </summary>
    public static class LayoutVariantSerialization
    {
        private const char PairSeparator = '\n';
        private const char KeyValueSeparator = '\x01';

        private static readonly HashSet<string> ForbiddenPaths = new HashSet<string>
        {
            "m_Children",
            "m_Father",
            "m_GameObject",
            "m_ObjectHideFlags",
            "m_CorrespondingSourceObject",
            "m_PrefabInstance",
            "m_PrefabAsset",
            "m_Script",
            "m_Name",
            "m_EditorClassIdentifier",
            "m_EditorHideFlags",
            // Localized and runtime-generated TMP content is data, not layout.
            // Capturing it makes an orientation switch restore the prefab's
            // English placeholder over the current language.
            "m_text",
            "m_LocalPosition",
            "m_ConstrainProportionsScale"
        };

        public static string Capture(Component target)
        {
            if (target == null)
                return null;

#if UNITY_EDITOR
            var serialized = new UnityEditor.SerializedObject(target);
            var iterator = serialized.GetIterator();
            var builder = new System.Text.StringBuilder(512);

            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;

                string path = iterator.propertyPath;
                if (IsForbidden(path))
                    continue;

                if (!TryReadValue(iterator, out string value))
                    continue;

                if (builder.Length > 0)
                    builder.Append(PairSeparator);

                builder.Append(path).Append(KeyValueSeparator).Append(value);
            }

            return builder.ToString();
#else
            Debug.LogWarning($"[Responsive] Захват снапшота доступен только в Editor ({target.GetType().Name}).");
            return null;
#endif
        }

        public static void Apply(Component target, string snapshot)
        {
            if (target == null || string.IsNullOrEmpty(snapshot))
                return;

            Apply(target, ParsePairs(snapshot));
        }

        /// <summary>
        /// Разбирает снапшот в массив пар один раз. Вызывающий код обязан кэшировать
        /// результат: повторный парсинг строки на каждую смену ориентации — это
        /// лишние аллокации (Split + substrings) на ровном месте.
        /// </summary>
        public static KeyValuePair<string, string>[] ParsePairs(string snapshot)
        {
            if (string.IsNullOrEmpty(snapshot))
                return System.Array.Empty<KeyValuePair<string, string>>();

            var list = new List<KeyValuePair<string, string>>(16);
            foreach (var pair in EnumeratePairs(snapshot))
            {
                // Also filter old snapshots that were captured before a path
                // became forbidden, so existing prefabs are fixed immediately.
                if (!IsForbidden(pair.Key))
                    list.Add(pair);
            }

            return list.ToArray();
        }

        public static void Apply(Component target, IReadOnlyList<KeyValuePair<string, string>> pairs)
        {
            if (target == null || pairs == null || pairs.Count == 0)
                return;

#if UNITY_EDITOR
            var serialized = new UnityEditor.SerializedObject(target);
            int changed = 0;

            for (int i = 0; i < pairs.Count; i++)
            {
                var property = serialized.FindProperty(pairs[i].Key);
                if (property == null)
                    continue;

                // КРИТИЧНО для prefab instance: пишем только реально отличающиеся
                // значения. Безусловная запись всех полей на каждый apply создавала
                // property overrides заново — стереть оверрайд из сцены становилось
                // невозможно, он «воскресал» после любого переприменения.
                // Сравнение строковое: текущее значение форматируется тем же
                // TryReadValue, что использовался при захвате.
                if (TryReadValue(property, out string current) && current == pairs[i].Value)
                    continue;

                WriteValue(property, pairs[i].Value);
                changed++;
            }

            if (changed == 0)
                return;

            if (!Application.isPlaying)
            {
                // ApplyModifiedProperties сам регистрирует undo-запись.
                serialized.ApplyModifiedProperties();
                UnityEditor.EditorUtility.SetDirty(target);
            }
            else
            {
                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
#else
            LayoutVariantRuntimeWriter.Apply(target, pairs);
#endif
        }

        internal static IEnumerable<KeyValuePair<string, string>> EnumeratePairs(string snapshot)
        {
            if (string.IsNullOrEmpty(snapshot))
                yield break;

            string[] lines = snapshot.Split(PairSeparator);
            for (int i = 0; i < lines.Length; i++)
            {
                int split = lines[i].IndexOf(KeyValueSeparator);
                if (split <= 0)
                    continue;

                yield return new KeyValuePair<string, string>(
                    lines[i].Substring(0, split),
                    lines[i].Substring(split + 1));
            }
        }

        private static bool IsForbidden(string path)
        {
            if (ForbiddenPaths.Contains(path))
                return true;

            // Вложенные пути служебных полей (m_Children.Array.size и т.п.).
            int dot = path.IndexOf('.');
            return dot > 0 && ForbiddenPaths.Contains(path.Substring(0, dot));
        }

#if UNITY_EDITOR
        private static bool TryReadValue(UnityEditor.SerializedProperty property, out string value)
        {
            value = null;

            switch (property.propertyType)
            {
                case UnityEditor.SerializedPropertyType.Integer:
                    value = property.longValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    return true;
                case UnityEditor.SerializedPropertyType.Boolean:
                    value = property.boolValue ? "1" : "0";
                    return true;
                case UnityEditor.SerializedPropertyType.Float:
                    value = property.doubleValue.ToString("R", System.Globalization.CultureInfo.InvariantCulture);
                    return true;
                case UnityEditor.SerializedPropertyType.String:
                    value = property.stringValue;
                    return true;
                case UnityEditor.SerializedPropertyType.Enum:
                    value = property.enumValueIndex.ToString(System.Globalization.CultureInfo.InvariantCulture);
                    return true;
                case UnityEditor.SerializedPropertyType.Vector2:
                    value = Format(property.vector2Value.x, property.vector2Value.y);
                    return true;
                case UnityEditor.SerializedPropertyType.Vector3:
                    value = Format(property.vector3Value.x, property.vector3Value.y, property.vector3Value.z);
                    return true;
                case UnityEditor.SerializedPropertyType.Vector4:
                    var v4 = property.vector4Value;
                    value = Format(v4.x, v4.y, v4.z, v4.w);
                    return true;
                case UnityEditor.SerializedPropertyType.Quaternion:
                    var q = property.quaternionValue;
                    value = Format(q.x, q.y, q.z, q.w);
                    return true;
                case UnityEditor.SerializedPropertyType.Color:
                    var c = property.colorValue;
                    value = Format(c.r, c.g, c.b, c.a);
                    return true;
                case UnityEditor.SerializedPropertyType.Rect:
                    var r = property.rectValue;
                    value = Format(r.x, r.y, r.width, r.height);
                    return true;
                case UnityEditor.SerializedPropertyType.Vector2Int:
                    var v2i = property.vector2IntValue;
                    value = Format(v2i.x, v2i.y);
                    return true;
                case UnityEditor.SerializedPropertyType.Vector3Int:
                    var v3i = property.vector3IntValue;
                    value = Format(v3i.x, v3i.y, v3i.z);
                    return true;
                default:
                    // Ссылки на объекты, массивы, кастомные структуры в снапшот
                    // не идут: варьируется layout, а не состав данных.
                    return false;
            }
        }

        private static void WriteValue(UnityEditor.SerializedProperty property, string value)
        {
            var culture = System.Globalization.CultureInfo.InvariantCulture;

            switch (property.propertyType)
            {
                case UnityEditor.SerializedPropertyType.Integer:
                    if (long.TryParse(value, System.Globalization.NumberStyles.Integer, culture, out long l))
                        property.longValue = l;
                    break;
                case UnityEditor.SerializedPropertyType.Boolean:
                    property.boolValue = value == "1";
                    break;
                case UnityEditor.SerializedPropertyType.Float:
                    if (double.TryParse(value, System.Globalization.NumberStyles.Float, culture, out double d))
                        property.doubleValue = d;
                    break;
                case UnityEditor.SerializedPropertyType.String:
                    property.stringValue = value;
                    break;
                case UnityEditor.SerializedPropertyType.Enum:
                    if (int.TryParse(value, System.Globalization.NumberStyles.Integer, culture, out int e))
                        property.enumValueIndex = e;
                    break;
                case UnityEditor.SerializedPropertyType.Vector2:
                    if (TryParse(value, 2, out float[] p2))
                        property.vector2Value = new Vector2(p2[0], p2[1]);
                    break;
                case UnityEditor.SerializedPropertyType.Vector3:
                    if (TryParse(value, 3, out float[] p3))
                        property.vector3Value = new Vector3(p3[0], p3[1], p3[2]);
                    break;
                case UnityEditor.SerializedPropertyType.Vector4:
                    if (TryParse(value, 4, out float[] p4))
                        property.vector4Value = new Vector4(p4[0], p4[1], p4[2], p4[3]);
                    break;
                case UnityEditor.SerializedPropertyType.Quaternion:
                    if (TryParse(value, 4, out float[] pq))
                        property.quaternionValue = new Quaternion(pq[0], pq[1], pq[2], pq[3]);
                    break;
                case UnityEditor.SerializedPropertyType.Color:
                    if (TryParse(value, 4, out float[] pc))
                        property.colorValue = new Color(pc[0], pc[1], pc[2], pc[3]);
                    break;
                case UnityEditor.SerializedPropertyType.Rect:
                    if (TryParse(value, 4, out float[] pr))
                        property.rectValue = new Rect(pr[0], pr[1], pr[2], pr[3]);
                    break;
                case UnityEditor.SerializedPropertyType.Vector2Int:
                    if (TryParse(value, 2, out float[] pv2i))
                        property.vector2IntValue = new Vector2Int((int)pv2i[0], (int)pv2i[1]);
                    break;
                case UnityEditor.SerializedPropertyType.Vector3Int:
                    if (TryParse(value, 3, out float[] pv3i))
                        property.vector3IntValue = new Vector3Int((int)pv3i[0], (int)pv3i[1], (int)pv3i[2]);
                    break;
            }
        }
#endif

        private static string Format(params float[] values)
        {
            var culture = System.Globalization.CultureInfo.InvariantCulture;
            var parts = new string[values.Length];
            for (int i = 0; i < values.Length; i++)
                parts[i] = values[i].ToString("R", culture);

            return string.Join(",", parts);
        }

        internal static bool TryParse(string value, int count, out float[] result)
        {
            result = null;
            if (string.IsNullOrEmpty(value))
                return false;

            string[] parts = value.Split(',');
            if (parts.Length < count)
                return false;

            var culture = System.Globalization.CultureInfo.InvariantCulture;
            result = new float[count];

            for (int i = 0; i < count; i++)
            {
                if (!float.TryParse(parts[i], System.Globalization.NumberStyles.Float, culture, out result[i]))
                    return false;
            }

            return true;
        }
    }
}
