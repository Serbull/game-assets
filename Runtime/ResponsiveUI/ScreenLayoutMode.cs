using System;
using UnityEngine;

namespace Serbull.ResponsiveUI
{
    public enum LayoutMode
    {
        Portrait = 0,
        Landscape = 1
    }

    /// <summary>
    /// Глобальный источник правды о текущем режиме экрана.
    /// В рантайме режим считает <see cref="ScreenLayoutWatcher"/> по aspect ratio,
    /// в Editor вне Play Mode его задаёт ручной тумблер (окно Responsive UI).
    /// </summary>
    public static class ScreenLayoutMode
    {
        public const string EditorModeKey = "Serbull.ResponsiveUI.EditorMode";

        private static LayoutMode _current = LayoutMode.Portrait;
        private static bool _initialized;

        /// <summary>Вызывается ПОСЛЕ смены режима. Подписчики применяют свой вариант.</summary>
        public static event Action<LayoutMode> Changed;

        /// <summary>
        /// Вызывается при любом фактическом изменении размера экрана, включая resize
        /// внутри одного режима (WebGL-окно). Стреляет после <see cref="Changed"/>.
        /// </summary>
        public static event Action ResolutionChanged;

        internal static void NotifyResolutionChanged()
        {
            ResolutionChanged?.Invoke();
        }

        public static LayoutMode Current
        {
            get
            {
                if (!_initialized)
                {
                    _initialized = true;
                    _current = DetectFromScreen();
                }

                return _current;
            }
        }

        public static bool IsPortrait => Current == LayoutMode.Portrait;
        public static bool IsLandscape => Current == LayoutMode.Landscape;

        /// <summary>
        /// Применяет режим и уведомляет подписчиков. Повторная установка того же
        /// значения не создаёт событие, поэтому вызов безопасно дёргать каждый кадр.
        /// </summary>
        public static void Set(LayoutMode mode, bool force = false)
        {
            _initialized = true;

            if (_current == mode && !force)
                return;

            _current = mode;
            Changed?.Invoke(mode);
        }

        public static LayoutMode DetectFromScreen()
        {
            return FromAspect(Screen.width, Screen.height);
        }

        public static LayoutMode FromAspect(float width, float height)
        {
            if (height <= 0f)
                return LayoutMode.Portrait;

            return width > height ? LayoutMode.Landscape : LayoutMode.Portrait;
        }
    }
}
