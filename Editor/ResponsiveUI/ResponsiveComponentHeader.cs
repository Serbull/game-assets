using System.Collections.Generic;
using System.Linq;
using Serbull.ResponsiveUI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Serbull.ResponsiveUI.EditorTools
{
    /// <summary>
    /// UI управления альтернативным layout в инспекторе.
    ///
    /// ГОТЧА: UnityEditor.Editor.finishedDefaultHeaderGUI стреляет только для большой шапки
    /// инспектора (GameObject/asset) — тайтлбары компонентов рисуются через
    /// EditorGUILayout.InspectorTitlebar БЕЗ события, официального хука в них нет.
    /// Поэтому блок кнопок рисуется под шапкой GameObject (как Addressables),
    /// а добавление с конкретного компонента продублировано в его ⋮-меню
    /// через MenuItem("CONTEXT/Component/...").
    ///
    /// Multi-select поддержан: при выборе нескольких GameObject блок агрегирует
    /// варианты по типу компонента, "Add" назначает всем выбранным, capture и
    /// удаление действуют на всю группу. Правки полей через штатный multi-edit
    /// инспектора ловит ResponsiveAutoCapture по каждому объекту отдельно.
    /// </summary>
    [InitializeOnLoad]
    public static class ResponsiveComponentHeader
    {
        private static readonly List<ComponentLayoutVariant> Buffer = new List<ComponentLayoutVariant>();
        private static readonly List<GameObject> Selected = new List<GameObject>();
        private static readonly List<RectTransformVariant> RectVariants = new List<RectTransformVariant>();
        private static readonly Dictionary<string, List<ComponentLayoutVariant>> Groups =
            new Dictionary<string, List<ComponentLayoutVariant>>();

        private static readonly GUIStyle StatusStyle = new GUIStyle();
        private static bool _statusStyleReady;

        static ResponsiveComponentHeader()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnHeaderGUI;
        }

        private static void OnHeaderGUI(UnityEditor.Editor editor)
        {
            if (editor.targets == null || editor.targets.Length == 0)
                return;

            Selected.Clear();

            foreach (var target in editor.targets)
            {
                var go = target as GameObject;
                if (go == null)
                    return; // Смешанный/не-GameObject инспектор — не наш случай.

                // Только сцена или prefab stage — ассеты в Project не трогаем.
                if (!go.scene.IsValid() && PrefabStageUtility.GetCurrentPrefabStage() == null)
                    return;

                // Не-UI объекты (без RectTransform) пропускаем молча.
                if (go.transform is RectTransform)
                    Selected.Add(go);
            }

            if (Selected.Count == 0)
                return;

            DrawBlock(Selected);
        }

        private static void DrawBlock(List<GameObject> gos)
        {
            DrawModeStatusLine();

            int total = gos.Count;

            bool playDriven = ResponsiveEditorMode.IsPlayDriven;

            using (new EditorGUILayout.HorizontalScope())
            {
                var mode = ResponsiveEditorMode.EffectiveMode;
                bool portrait = mode == LayoutMode.Portrait;

                Color prev = GUI.backgroundColor;
                GUI.backgroundColor = portrait
                    ? new Color(0.55f, 0.85f, 1f)
                    : new Color(1f, 0.85f, 0.45f);

                string modeLabel;
                string modeTooltip;
                if (playDriven)
                {
                    modeLabel = portrait ? "▯ Portrait  (auto)" : "▭ Landscape  (auto)";
                    modeTooltip = "Play Mode: режим определяется aspect ratio Game view, ручной тумблер отключён.";
                }
                else
                {
                    modeLabel = portrait ? "▯ Portrait  (F9)" : "▭ Landscape  (F9)";
                    modeTooltip = "Глобальный режим. Правки компонентов пишутся в активный слот.";
                }

                using (new EditorGUI.DisabledScope(playDriven))
                {
                    if (GUILayout.Button(new GUIContent(modeLabel, modeTooltip),
                            EditorStyles.miniButton, GUILayout.Width(130f)))
                        ResponsiveEditorMode.Toggle();
                }

                GUI.backgroundColor = prev;

                string addLabel = total > 1
                    ? "＋ Add alternative layout (" + total + " objects)"
                    : "＋ Add alternative layout";

                // В Play Mode добавление/захват бессмысленны: изменения компонентов
                // откатятся при выходе из Play Mode и слоты будут потеряны.
                using (new EditorGUI.DisabledScope(playDriven))
                {
                    if (GUILayout.Button(new GUIContent(addLabel,
                            playDriven ? "Недоступно в Play Mode — изменения не переживут выход из Play." : ""),
                            EditorStyles.miniButton))
                        ShowAddMenu(gos.ToArray());
                }
            }

            using (new EditorGUI.DisabledScope(playDriven))
            {
            // === Группа RectTransform ===
            RectVariants.Clear();
            foreach (var go in gos)
            {
                var v = go.GetComponent<RectTransformVariant>();
                if (v != null)
                    RectVariants.Add(v);
            }

            if (RectVariants.Count > 0)
            {
                var arr = RectVariants.ToArray();
                bool allPortrait = true, allLandscape = true, anyEditable = false;
                foreach (var v in arr)
                {
                    allPortrait &= v.HasPortrait;
                    allLandscape &= v.HasLandscape;
                    anyEditable |= !v.IsExternallyOwned;
                }

                string label = total > 1
                    ? "Rect Transform (" + arr.Length + "/" + total + ")"
                    : "Rect Transform";

                if (anyEditable)
                {
                    DrawRow(label, allPortrait, allLandscape,
                        m => { foreach (var v in arr) if (!v.IsExternallyOwned) CaptureRect(v, m); },
                        () => { foreach (var v in arr) if (!v.IsExternallyOwned) Undo.DestroyObjectImmediate(v); });
                }
                else
                {
                    DrawExternalRow(label, arr);
                }
            }

            // === Группы ComponentLayoutVariant по типу целевого компонента ===
            Groups.Clear();
            foreach (var go in gos)
            {
                foreach (var v in go.GetComponents<ComponentLayoutVariant>())
                {
                    if (v == null)
                        continue;

                    string key = v.Target != null
                        ? ObjectNames.NicifyVariableName(v.Target.GetType().Name)
                        : "<missing target>";

                    if (!Groups.TryGetValue(key, out var list))
                        Groups[key] = list = new List<ComponentLayoutVariant>();

                    list.Add(v);
                }
            }

            foreach (var key in Groups.Keys.OrderBy(k => k))
            {
                var arr = Groups[key].ToArray();
                bool allPortrait = true, allLandscape = true, anyEditable = false;
                foreach (var v in arr)
                {
                    allPortrait &= v.HasPortrait;
                    allLandscape &= v.HasLandscape;
                    anyEditable |= !v.IsExternallyOwned;
                }

                string label = total > 1 ? key + " (" + arr.Length + "/" + total + ")" : key;

                if (anyEditable)
                {
                    DrawRow(label, allPortrait, allLandscape,
                        m => { foreach (var v in arr) if (!v.IsExternallyOwned) CaptureComponent(v, m); },
                        () => { foreach (var v in arr) if (!v.IsExternallyOwned) Undo.DestroyObjectImmediate(v); });
                }
                else
                {
                    DrawExternalRow(label, arr);
                }
            }
            } // DisabledScope(playDriven)
        }

        /// <summary>
        /// Явная статусная строка против человеческого фактора: всегда видно,
        /// в каком режиме сейчас экран и куда сохраняются правки.
        /// </summary>
        private static void DrawModeStatusLine()
        {
            if (!_statusStyleReady)
            {
                StatusStyle.fontStyle = FontStyle.Bold;
                StatusStyle.fontSize = 11;
                StatusStyle.padding = new RectOffset(4, 4, 2, 2);
                _statusStyleReady = true;
            }

            bool portrait = ResponsiveEditorMode.EffectiveMode == LayoutMode.Portrait;

            StatusStyle.normal.textColor = portrait
                ? new Color(0.4f, 0.75f, 1f)
                : new Color(1f, 0.75f, 0.3f);

            string text;
            if (ResponsiveEditorMode.IsPlayDriven)
            {
                text = portrait
                    ? "PLAY MODE — screen working in VERTICAL mode (from Game view aspect ratio)"
                    : "PLAY MODE — screen working in HORIZONTAL mode (from Game view aspect ratio)";
            }
            else
            {
                text = portrait
                    ? "Now screen working in VERTICAL (Portrait) mode — edits are saved to the Portrait layout"
                    : "Now screen working in HORIZONTAL (Landscape) mode — edits are saved to the Landscape layout";
            }

            EditorGUILayout.LabelField(text, StatusStyle);
        }

        /// <summary>
        /// Строка хелпера, принадлежащего префабу (смотрим снаружи). Снапшоты
        /// здесь read-only; единственное действие — сбросить локальные оверрайды
        /// полей хелпера, накопленные до появления гейта: они экранируют
        /// снапшоты префаба, и инстанс продолжает применять устаревшие значения
        /// ("в префабе включено, в сцене выключается и не ревертится").
        /// </summary>
        private static void DrawExternalRow(string label, Component[] variants)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(8f);
                GUILayout.Label(label + "  — Prefab Mode only", EditorStyles.miniLabel, GUILayout.Width(230f));

                if (GUILayout.Button(new GUIContent("⟲ Use prefab values",
                        "Сбросить локальные оверрайды снапшотов этого хелпера и заново применить вариант из префаба."),
                        EditorStyles.miniButton))
                {
                    var mode = ResponsiveEditorMode.EffectiveMode;
                    foreach (var v in variants)
                    {
                        if (v == null)
                            continue;

                        PrefabUtility.RevertObjectOverride(v, InteractionMode.UserAction);

                        if (v is RectTransformVariant rectVariant)
                            rectVariant.Apply(mode);
                        else if (v is ComponentLayoutVariant componentVariant)
                            componentVariant.Apply(mode);
                    }
                }
            }
        }

        private static void DrawRow(string label, bool hasPortrait, bool hasLandscape,
            System.Action<LayoutMode> onCapture, System.Action onRemove)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(8f);
                GUILayout.Label(label, EditorStyles.miniLabel, GUILayout.Width(170f));

                DrawSlotButton("Portrait", LayoutMode.Portrait, hasPortrait, onCapture);
                DrawSlotButton("Landscape", LayoutMode.Landscape, hasLandscape, onCapture);

                if (GUILayout.Button(new GUIContent("✕", "Убрать альтернативный layout со всех выбранных объектов"),
                        EditorStyles.miniButton, GUILayout.Width(22f)))
                    onRemove();
            }
        }

        private static void DrawSlotButton(string label, LayoutMode slot, bool captured, System.Action<LayoutMode> onCapture)
        {
            var active = ResponsiveEditorMode.EffectiveMode;
            Color prev = GUI.backgroundColor;

            if (slot == active)
                GUI.backgroundColor = new Color(0.55f, 0.85f, 1f);
            else if (!captured)
                GUI.backgroundColor = new Color(1f, 0.6f, 0.5f);

            string text = slot == active ? "● " + label : label;
            string tooltip = slot == active
                ? "Активный слот: правки компонентов сохраняются сюда автоматически."
                : "Записать ТЕКУЩЕЕ состояние компонентов всех выбранных объектов в слот " + label + ".";

            if (GUILayout.Button(new GUIContent(text, tooltip), EditorStyles.miniButton))
                onCapture(slot);

            GUI.backgroundColor = prev;
        }

        /// <summary>
        /// true для объекта, принадлежащего prefab-ассету, на который мы смотрим
        /// снаружи. Добавлять/менять варианты такого объекта можно только в
        /// Prefab Mode его собственного префаба.
        /// </summary>
        private static bool IsExternallyOwnedGo(GameObject go)
        {
            if (PrefabUtility.GetCorrespondingObjectFromSource(go) == null)
                return false;

            // Корень инстанса — сценовые данные, там варианты добавляются
            // и правятся свободно; заблокированы только внутренности префаба.
            return !PrefabUtility.IsOutermostPrefabInstanceRoot(go);
        }

        private static void ShowAddMenu(GameObject[] gos)
        {
            var menu = new GenericMenu();
            int total = gos.Length;
            bool prefabBlocked = false;

            // Rect Transform — сколько выбранных ещё без варианта.
            int rectMissing = 0;
            foreach (var go in gos)
            {
                if (IsExternallyOwnedGo(go))
                {
                    prefabBlocked = true;
                    continue;
                }

                if (go.transform is RectTransform && go.GetComponent<RectTransformVariant>() == null)
                    rectMissing++;
            }

            if (rectMissing > 0)
            {
                string label = total > 1
                    ? "Rect Transform (" + rectMissing + " of " + total + ")"
                    : "Rect Transform";

                menu.AddItem(new GUIContent(label), false, () =>
                {
                    foreach (var go in gos)
                    {
                        if (IsExternallyOwnedGo(go))
                            continue;

                        if (go.transform is RectTransform rect && go.GetComponent<RectTransformVariant>() == null)
                            AddRectVariant(rect);
                    }
                });
            }

            // Остальные типы компонентов по всему выбору: тип -> сколько объектов
            // ещё имеют этот компонент без варианта.
            var addable = new Dictionary<System.Type, int>();
            foreach (var go in gos)
            {
                if (IsExternallyOwnedGo(go))
                    continue;

                var existing = go.GetComponents<ComponentLayoutVariant>();
                foreach (var component in go.GetComponents<Component>())
                {
                    if (component == null || component is Transform)
                        continue;
                    if (component is RectTransformVariant || component is ComponentLayoutVariant)
                        continue;
                    if (HasVariant(existing, component))
                        continue;

                    addable.TryGetValue(component.GetType(), out int n);
                    addable[component.GetType()] = n + 1;
                }
            }

            foreach (var pair in addable.OrderBy(p => p.Key.Name))
            {
                var type = pair.Key;
                string nice = ObjectNames.NicifyVariableName(type.Name);
                string label = total > 1 ? nice + " (" + pair.Value + " of " + total + ")" : nice;

                menu.AddItem(new GUIContent(label), false, () => AddComponentVariantToAll(gos, type));
            }

            if (prefabBlocked)
                menu.AddDisabledItem(new GUIContent("Объекты префаба — откройте его в Prefab Mode"));

            if (menu.GetItemCount() == 0)
                menu.AddDisabledItem(new GUIContent("Все компоненты уже добавлены"));

            menu.ShowAsContext();
        }

        // === Добавление через ⋮-меню конкретного компонента ===
        // При multi-select применяется ко всем выбранным объектам с тем же типом
        // компонента. Идемпотентно: повторный вызов по уже покрытому объекту
        // ничего не дублирует (Unity может дёргать CONTEXT-пункт по каждой цели).

        [MenuItem("CONTEXT/Component/Add Alternative Layout")]
        private static void AddFromContext(MenuCommand cmd)
        {
            var component = cmd.context as Component;
            if (component == null)
                return;

            var selection = Selection.gameObjects;
            bool multi = selection != null && selection.Length > 1
                && System.Array.IndexOf(selection, component.gameObject) >= 0;

            if (multi)
            {
                var type = component.GetType();
                foreach (var go in selection)
                    AddVariantForType(go, type);
                return;
            }

            AddVariantForType(component.gameObject, component.GetType());
        }

        [MenuItem("CONTEXT/Component/Add Alternative Layout", true)]
        private static bool AddFromContextValidate(MenuCommand cmd)
        {
            var component = cmd.context as Component;
            if (component == null)
                return false;
            if (component is ComponentLayoutVariant || component is RectTransformVariant)
                return false;
            // Объект чужого префаба: варианты добавляются только в его Prefab Mode.
            if (IsExternallyOwnedGo(component.gameObject))
                return false;
            if (component is RectTransform rect)
                return rect.GetComponent<RectTransformVariant>() == null;
            if (component is Transform)
                return false;

            return FindVariantFor(component) == null;
        }

        // === Общие операции ===

        private static void AddVariantForType(GameObject go, System.Type type)
        {
            if (IsExternallyOwnedGo(go))
                return;

            if (type == typeof(RectTransform))
            {
                if (go.transform is RectTransform rect && go.GetComponent<RectTransformVariant>() == null)
                    AddRectVariant(rect);
                return;
            }

            var existing = go.GetComponents<ComponentLayoutVariant>();
            foreach (var component in go.GetComponents(type))
            {
                var c = component as Component;
                if (c == null || c is Transform || c is RectTransformVariant || c is ComponentLayoutVariant)
                    continue;
                if (!HasVariant(existing, c))
                    AddComponentVariant(c);
            }
        }

        private static void AddComponentVariantToAll(GameObject[] gos, System.Type type)
        {
            foreach (var go in gos)
                AddVariantForType(go, type);
        }

        private static void AddRectVariant(RectTransform rect)
        {
            var added = Undo.AddComponent<RectTransformVariant>(rect.gameObject);
            // Текущее состояние становится стартовой точкой обоих слотов.
            added.Capture(LayoutMode.Portrait);
            added.Capture(LayoutMode.Landscape);
            EditorUtility.SetDirty(added);
        }

        private static void AddComponentVariant(Component target)
        {
            var added = Undo.AddComponent<ComponentLayoutVariant>(target.gameObject);
            added.Target = target;
            added.Capture(LayoutMode.Portrait);
            added.Capture(LayoutMode.Landscape);
            EditorUtility.SetDirty(added);
        }

        // SetDirty только при фактическом изменении снапшота: RecordObject без
        // последующего диффа Unity отбрасывает сам, а вот безусловный SetDirty
        // диртил хелпер и штамповал prefab overrides на каждый чих.

        private static void CaptureRect(RectTransformVariant variant, LayoutMode mode)
        {
            Undo.RecordObject(variant, "Capture Layout Variant");
            if (variant.Capture(mode))
                EditorUtility.SetDirty(variant);
        }

        private static void CaptureComponent(ComponentLayoutVariant variant, LayoutMode mode)
        {
            Undo.RecordObject(variant, "Capture Layout Variant");
            if (variant.Capture(mode))
                EditorUtility.SetDirty(variant);
        }

        private static bool HasVariant(ComponentLayoutVariant[] variants, Component component)
        {
            for (int i = 0; i < variants.Length; i++)
            {
                if (variants[i] != null && variants[i].Target == component)
                    return true;
            }

            return false;
        }

        /// <summary>Используется ResponsiveAutoCapture — не менять сигнатуру.</summary>
        internal static ComponentLayoutVariant FindVariantFor(Component component)
        {
            if (component == null)
                return null;

            Buffer.Clear();
            component.GetComponents(Buffer);

            for (int i = 0; i < Buffer.Count; i++)
            {
                if (Buffer[i].Target == component)
                    return Buffer[i];
            }

            return null;
        }
    }
}
