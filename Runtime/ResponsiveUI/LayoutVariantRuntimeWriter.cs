using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Serbull.ResponsiveUI
{
    /// <summary>
    /// Применяет снапшот в билде, где SerializedObject недоступен.
    ///
    /// Пути снапшота — это SerializedProperty-пути ("m_AnchorMin", "m_Padding.m_Left"),
    /// которые соответствуют приватным сериализованным полям. Ищем их рефлексией
    /// по иерархии типов, результат кэшируем — в рантайме смена ориентации редкая,
    /// но при перевороте применяются сотни компонентов сразу.
    ///
    /// RectTransform обрабатывается отдельно: его свойства нативные, полей под
    /// этими именами в managed-типе нет.
    /// </summary>
    internal static class LayoutVariantRuntimeWriter
    {
        private static readonly Dictionary<Type, Dictionary<string, FieldInfo>> FieldCache =
            new Dictionary<Type, Dictionary<string, FieldInfo>>();

        public static void Apply(Component target, IEnumerable<KeyValuePair<string, string>> pairs)
        {
            if (target == null)
                return;

            if (target is RectTransform rect)
            {
                ApplyRect(rect, pairs);
                return;
            }

            var type = target.GetType();

            foreach (var pair in pairs)
            {
                var field = ResolveField(type, pair.Key);
                if (field == null)
                    continue;

                object value = Convert(field.FieldType, pair.Value);
                if (value != null)
                {
                    // The fitter's setters recalculate its driven RectTransform.
                    // Writing backing fields leaves the old geometry until re-enable.
                    if (target is UnityEngine.UI.AspectRatioFitter fitter && pair.Key == "m_AspectRatio")
                        fitter.aspectRatio = (float)value;
                    else if (target is UnityEngine.UI.AspectRatioFitter modeFitter && pair.Key == "m_AspectMode")
                        modeFitter.aspectMode = (UnityEngine.UI.AspectRatioFitter.AspectMode)value;
                    else
                        field.SetValue(target, value);
                }
            }
        }

        private static void ApplyRect(RectTransform rect, IEnumerable<KeyValuePair<string, string>> pairs)
        {
            foreach (var pair in pairs)
            {
                switch (pair.Key)
                {
                    case "m_AnchorMin":
                        if (LayoutVariantSerialization.TryParse(pair.Value, 2, out var min))
                            rect.anchorMin = new Vector2(min[0], min[1]);
                        break;
                    case "m_AnchorMax":
                        if (LayoutVariantSerialization.TryParse(pair.Value, 2, out var max))
                            rect.anchorMax = new Vector2(max[0], max[1]);
                        break;
                    case "m_Pivot":
                        if (LayoutVariantSerialization.TryParse(pair.Value, 2, out var pivot))
                            rect.pivot = new Vector2(pivot[0], pivot[1]);
                        break;
                    case "m_AnchoredPosition":
                        if (LayoutVariantSerialization.TryParse(pair.Value, 2, out var pos))
                            rect.anchoredPosition = new Vector2(pos[0], pos[1]);
                        break;
                    case "m_SizeDelta":
                        if (LayoutVariantSerialization.TryParse(pair.Value, 2, out var size))
                            rect.sizeDelta = new Vector2(size[0], size[1]);
                        break;
                    case "m_LocalScale":
                        if (LayoutVariantSerialization.TryParse(pair.Value, 3, out var scale))
                            rect.localScale = new Vector3(scale[0], scale[1], scale[2]);
                        break;
                    case "m_LocalRotation":
                        if (LayoutVariantSerialization.TryParse(pair.Value, 4, out var rot))
                            rect.localRotation = new Quaternion(rot[0], rot[1], rot[2], rot[3]);
                        break;
                }
            }
        }

        private static FieldInfo ResolveField(Type type, string path)
        {
            if (!FieldCache.TryGetValue(type, out var map))
            {
                map = new Dictionary<string, FieldInfo>();
                FieldCache[type] = map;
            }

            if (map.TryGetValue(path, out var cached))
                return cached;

            FieldInfo found = null;

            // Вложенные пути ("m_Padding.m_Left") рантайм не поддерживает:
            // в них лежат структуры Unity, которые пришлось бы боксить целиком.
            if (path.IndexOf('.') < 0)
            {
                for (var t = type; t != null && t != typeof(object); t = t.BaseType)
                {
                    found = t.GetField(path, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (found != null)
                        break;
                }
            }

            map[path] = found;
            return found;
        }

        private static object Convert(Type type, string value)
        {
            var culture = System.Globalization.CultureInfo.InvariantCulture;

            if (type == typeof(float))
                return float.TryParse(value, System.Globalization.NumberStyles.Float, culture, out float f) ? f : (object)null;

            if (type == typeof(int))
                return int.TryParse(value, System.Globalization.NumberStyles.Integer, culture, out int i) ? i : (object)null;

            if (type == typeof(bool))
                return value == "1";

            if (type == typeof(string))
                return value;

            if (type.IsEnum)
            {
                if (!int.TryParse(value, System.Globalization.NumberStyles.Integer, culture, out int e))
                    return null;

                var values = Enum.GetValues(type);
                return e >= 0 && e < values.Length ? values.GetValue(e) : null;
            }

            if (type == typeof(Vector2))
                return LayoutVariantSerialization.TryParse(value, 2, out var v2) ? new Vector2(v2[0], v2[1]) : (object)null;

            if (type == typeof(Vector3))
                return LayoutVariantSerialization.TryParse(value, 3, out var v3) ? new Vector3(v3[0], v3[1], v3[2]) : (object)null;

            if (type == typeof(Vector4))
                return LayoutVariantSerialization.TryParse(value, 4, out var v4) ? new Vector4(v4[0], v4[1], v4[2], v4[3]) : (object)null;

            if (type == typeof(Quaternion))
                return LayoutVariantSerialization.TryParse(value, 4, out var q) ? new Quaternion(q[0], q[1], q[2], q[3]) : (object)null;

            if (type == typeof(Color))
                return LayoutVariantSerialization.TryParse(value, 4, out var c) ? new Color(c[0], c[1], c[2], c[3]) : (object)null;

            if (type == typeof(Rect))
                return LayoutVariantSerialization.TryParse(value, 4, out var r) ? new Rect(r[0], r[1], r[2], r[3]) : (object)null;

            if (type == typeof(Vector2Int))
                return LayoutVariantSerialization.TryParse(value, 2, out var v2i) ? new Vector2Int((int)v2i[0], (int)v2i[1]) : (object)null;

            if (type == typeof(Vector3Int))
                return LayoutVariantSerialization.TryParse(value, 3, out var v3i) ? new Vector3Int((int)v3i[0], (int)v3i[1], (int)v3i[2]) : (object)null;

            return null;
        }
    }
}
