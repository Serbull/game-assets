using UnityEngine;

namespace Serbull.ResponsiveUI
{
    /// <summary>
    /// Следит за фактическим размером экрана и переключает <see cref="ScreenLayoutMode"/>.
    /// Создаётся автоматически, отдельный объект в сцене класть не нужно.
    /// В Editor вне Play Mode не работает: там режим задаёт ручной тумблер.
    /// </summary>
    [DisallowMultipleComponent]
    public class ScreenLayoutWatcher : MonoBehaviour
    {
        private int _lastWidth;
        private int _lastHeight;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            var go = new GameObject(nameof(ScreenLayoutWatcher));
            go.hideFlags = HideFlags.HideInHierarchy;
            DontDestroyOnLoad(go);
            go.AddComponent<ScreenLayoutWatcher>();
        }

        private void Awake()
        {
            _lastWidth = Screen.width;
            _lastHeight = Screen.height;

            // force: подписчики уже могли проснуться до нас и применить дефолт,
            // поэтому первый прогон обязан разослать событие даже без смены режима.
            ScreenLayoutMode.Set(ScreenLayoutMode.FromAspect(_lastWidth, _lastHeight), force: true);
            ScreenLayoutMode.NotifyResolutionChanged();
        }

        private void Update()
        {
            int width = Screen.width;
            int height = Screen.height;

            if (width == _lastWidth && height == _lastHeight)
                return;

            _lastWidth = width;
            _lastHeight = height;

            ScreenLayoutMode.Set(ScreenLayoutMode.FromAspect(width, height));
            ScreenLayoutMode.NotifyResolutionChanged();
        }
    }
}
