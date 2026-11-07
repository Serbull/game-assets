using Serbull.ResponsiveUI;
using UnityEditor;
using UnityEngine;

namespace Serbull.ResponsiveUI.EditorTools
{
    /// <summary>
    /// Ручной тумблер режима для Edit Mode. Выбранный режим хранится в EditorPrefs,
    /// поэтому переживает перекомпиляцию и перезапуск редактора.
    ///
    /// В Play Mode тумблер не вмешивается: там режим считает ScreenLayoutWatcher
    /// по фактическому aspect ratio.
    /// </summary>
    [InitializeOnLoad]
    public static class ResponsiveEditorMode
    {
        private static readonly string PrefKey = ScreenLayoutMode.EditorModeKey + "." + Application.dataPath;

        static ResponsiveEditorMode()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        public static LayoutMode Current
        {
            get => (LayoutMode)EditorPrefs.GetInt(PrefKey, (int)LayoutMode.Portrait);
            set
            {
                if (Current == value)
                    return;

                EditorPrefs.SetInt(PrefKey, (int)value);
                PushToRuntime();
            }
        }

        /// <summary>true, когда режим диктуется реальным aspect ratio (Play Mode).</summary>
        public static bool IsPlayDriven => EditorApplication.isPlayingOrWillChangePlaymode;

        /// <summary>
        /// Фактический режим для UI: в Play Mode — от aspect ratio Game view
        /// (его держит ScreenLayoutWatcher), в Edit Mode — ручной тумблер.
        /// </summary>
        public static LayoutMode EffectiveMode => IsPlayDriven ? ScreenLayoutMode.Current : Current;

        public static void Toggle()
        {
            if (IsPlayDriven)
            {
                Debug.Log("[Responsive] В Play Mode режим определяется aspect ratio Game view; " +
                          "ручной тумблер работает только в Edit Mode.");
                return;
            }

            Current = Current == LayoutMode.Portrait ? LayoutMode.Landscape : LayoutMode.Portrait;
        }

        /// <summary>
        /// Применяет режим ко всем хелперам сцены. В Edit Mode это реально меняет
        /// значения компонентов — по решению «компонент источник правды».
        /// </summary>
        public static void PushToRuntime()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            ScreenLayoutMode.Set(Current, force: true);
            SceneView.RepaintAll();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            // Вход/выход из Play Mode не должен переписывать сцену. Unity сама
            // возвращает сохранённые Edit Mode значения, а следующий явный F9
            // снова применит выбранный автором вариант.
            if (state == PlayModeStateChange.EnteredEditMode)
                SceneView.RepaintAll();
        }

        [MenuItem("Tools/Responsive UI/Toggle Portrait-Landscape _F9")]
        private static void ToggleMenu() => Toggle();

        [MenuItem("Tools/Responsive UI/Set Portrait")]
        private static void SetPortrait() => SetManual(LayoutMode.Portrait);

        [MenuItem("Tools/Responsive UI/Set Landscape")]
        private static void SetLandscape() => SetManual(LayoutMode.Landscape);

        private static void SetManual(LayoutMode mode)
        {
            if (IsPlayDriven)
            {
                Debug.Log("[Responsive] В Play Mode режим определяется aspect ratio Game view; " +
                          "ручной выбор работает только в Edit Mode.");
                return;
            }

            Current = mode;
        }
    }
}
