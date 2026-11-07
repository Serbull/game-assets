using Serbull.ResponsiveUI;
using UnityEditor;
using UnityEngine;

namespace Serbull.ResponsiveUI.EditorTools
{
    /// <summary>
    /// Ловит любые правки компонентов в Edit Mode и пишет их в слот активного
    /// режима. Благодаря этому художник правит настоящий компонент штатным
    /// инспектором, а хелпер сам запоминает значения — отдельного UI для ввода
    /// координат не требуется.
    ///
    /// Хук Undo.postprocessModifications срабатывает на изменения из инспектора,
    /// из Scene view (перетаскивание, ресайз) и из любого кода, вызвавшего
    /// RecordObject — то есть покрывает все обычные способы правки.
    /// </summary>
    [InitializeOnLoad]
    public static class ResponsiveAutoCapture
    {
        /// <summary>
        /// Взводится, пока мы сами накатываем снапшот. Без него применение
        /// варианта тут же было бы записано обратно как "правка пользователя",
        /// и второй слот затёрся бы первым.
        /// </summary>
        private static bool _suppressed;

        static ResponsiveAutoCapture()
        {
            Undo.postprocessModifications += OnModifications;
            ScreenLayoutMode.Changed += OnModeChanged;
        }

        private static void OnModeChanged(LayoutMode mode)
        {
            // Применение варианта идёт синхронно внутри события, поэтому глушим
            // автозахват на время текущей операции и снимаем на следующем такте.
            _suppressed = true;
            EditorApplication.delayCall += () => _suppressed = false;
        }

        private static UndoPropertyModification[] OnModifications(UndoPropertyModification[] modifications)
        {
            if (_suppressed || EditorApplication.isPlayingOrWillChangePlaymode)
                return modifications;

            var mode = ResponsiveEditorMode.Current;

            for (int i = 0; i < modifications.Length; i++)
            {
                var target = modifications[i].currentValue?.target as Component;
                if (target == null)
                    continue;

                CaptureIfVaried(target, mode);
            }

            return modifications;
        }

        private static void CaptureIfVaried(Component target, LayoutMode mode)
        {
            if (target is RectTransform rect)
            {
                var rectVariant = rect.GetComponent<RectTransformVariant>();
                if (rectVariant == null || !rectVariant.VariantEnabled)
                    return;

                // SetDirty только при реальном изменении снапшота — иначе каждый
                // автозахват диртит хелпер и плодит prefab overrides на инстансах.
                if (rectVariant.Capture(mode))
                    EditorUtility.SetDirty(rectVariant);
                return;
            }

            var variant = ResponsiveComponentHeader.FindVariantFor(target);
            if (variant == null || !variant.VariantEnabled)
                return;

            if (variant.Capture(mode))
                EditorUtility.SetDirty(variant);
        }
    }
}
