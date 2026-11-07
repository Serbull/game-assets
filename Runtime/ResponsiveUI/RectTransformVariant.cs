using UnityEngine;

namespace Serbull.ResponsiveUI
{
    /// <summary>
    /// Альтернативный layout для RectTransform: якоря, pivot, позиция, размер,
    /// поворот и масштаб. Тип растягивания задаётся парой anchorMin/anchorMax,
    /// поэтому он тоже полностью варьируется.
    ///
    /// Хранит типизированный снапшот, а не JSON: у RectTransform фиксированный
    /// набор полей, и явная структура читаема в инспекторе и в диффе префаба.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("Responsive UI/Rect Transform Variant")]
    public class RectTransformVariant : MonoBehaviour
    {
        [System.Serializable]
        public struct RectSnapshot
        {
            public Vector2 anchorMin;
            public Vector2 anchorMax;
            public Vector2 pivot;
            public Vector2 anchoredPosition;
            public Vector2 sizeDelta;
            public Vector3 localScale;
            public Vector3 localEulerAngles;

            public static RectSnapshot Capture(RectTransform rt) => new RectSnapshot
            {
                anchorMin = rt.anchorMin,
                anchorMax = rt.anchorMax,
                pivot = rt.pivot,
                anchoredPosition = rt.anchoredPosition,
                sizeDelta = rt.sizeDelta,
                localScale = rt.localScale,
                localEulerAngles = rt.localEulerAngles
            };

            public void Apply(RectTransform rt)
            {
                rt.anchorMin = anchorMin;
                rt.anchorMax = anchorMax;
                rt.pivot = pivot;
                rt.anchoredPosition = anchoredPosition;
                rt.sizeDelta = sizeDelta;
                rt.localScale = localScale;
                rt.localEulerAngles = localEulerAngles;
            }

            /// <summary>
            /// Векторный == с допуском Unity, поэтому round-trip захваченных
            /// значений всегда матчится и повторный apply становится no-op.
            /// </summary>
            public bool Matches(RectTransform rt)
            {
                return anchorMin == rt.anchorMin
                    && anchorMax == rt.anchorMax
                    && pivot == rt.pivot
                    && anchoredPosition == rt.anchoredPosition
                    && sizeDelta == rt.sizeDelta
                    && localScale == rt.localScale
                    && localEulerAngles == rt.localEulerAngles;
            }
        }

        [SerializeField] private RectSnapshot _portrait;
        [SerializeField] private RectSnapshot _landscape;
        [SerializeField, HideInInspector] private bool _portraitCaptured;
        [SerializeField, HideInInspector] private bool _landscapeCaptured;
        [SerializeField] private bool _variantEnabled = true;

        private RectTransform _rect;
        private bool _subscribed;

        public RectTransform Rect => _rect != null ? _rect : (_rect = (RectTransform)transform);
        public bool HasPortrait => _portraitCaptured;
        public bool HasLandscape => _landscapeCaptured;
        public bool VariantEnabled => _variantEnabled;

        /// <summary>
        /// true, когда хелпер принадлежит prefab-ассету, а мы смотрим на него
        /// снаружи (инстанс/вложенный префаб). Гейтится ТОЛЬКО Capture —
        /// снапшоты правятся в Prefab Mode собственного префаба; Apply работает
        /// всегда, иначе инстансы не адаптируются при смене режима экрана.
        /// В Play Mode гейт не действует.
        /// </summary>
        public bool IsExternallyOwned
        {
            get
            {
#if UNITY_EDITOR
                if (Application.isPlaying)
                    return false;

                if (UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(this) == null)
                    return false;

                // Корень инстанса — сценовые данные (см. ComponentLayoutVariant):
                // без этого исключения префаб нельзя даже подвинуть в сцене.
                return !UnityEditor.PrefabUtility.IsOutermostPrefabInstanceRoot(gameObject);
#else
                return false;
#endif
            }
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            // Гард импорта: см. ComponentLayoutVariant.OnEnable.
            if (!Application.isPlaying && UnityEditor.EditorUtility.IsPersistent(this))
                return;
#endif
            if (!_subscribed)
            {
                ScreenLayoutMode.Changed += OnModeChanged;
                _subscribed = true;
            }

#if UNITY_EDITOR
            // Prefab Stage — редактор ассета, а не runtime-инстанс. Начальный
            // Apply при каждом входе в Prefab Mode менял RectTransform под
            // текущий глобальный режим и сразу показывал Save/Discard, хотя
            // пользователь ничего не редактировал. Осознанное переключение
            // режима через F9 по-прежнему приходит в OnModeChanged.
            if (IsInPrefabStage())
                return;

            // В Edit Mode OnEnable срабатывает при открытии сцены, импорте и
            // domain reload. Накат варианта в этот момент создаёт сотни prefab
            // overrides в сцене без действия пользователя. В редакторе layout
            // меняется только явным переключением режима (F9); в Play Mode
            // начальный Apply по фактическому aspect ratio остаётся обязательным.
            if (!Application.isPlaying)
                return;
#endif
            Apply(ScreenLayoutMode.Current);
        }

        private void OnDisable()
        {
            if (!_subscribed)
                return;

            ScreenLayoutMode.Changed -= OnModeChanged;
            _subscribed = false;
        }

        private void OnModeChanged(LayoutMode mode)
        {
#if UNITY_EDITOR
            // В Play Mode Prefab Stage остаётся редактором ассета и не должен
            // реагировать на aspect ratio Game View. В Edit Mode ручной F9
            // намеренно разрешён, чтобы автор мог править второй снапшот.
            if (Application.isPlaying && IsInPrefabStage())
                return;
#endif
            Apply(mode);
        }

#if UNITY_EDITOR
        private bool IsInPrefabStage()
        {
            // GetCurrentPrefabStage ещё может быть null во время OnEnable:
            // объект stage уже создан, но stage не успел стать current.
            return UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null;
        }
#endif

        public bool Apply(LayoutMode mode)
        {
            if (!_variantEnabled)
                return false;

            bool captured = mode == LayoutMode.Portrait ? _portraitCaptured : _landscapeCaptured;
            if (!captured)
                return false;

            var snapshot = mode == LayoutMode.Portrait ? _portrait : _landscape;

            // Не диртим сцену и не плодим prefab overrides, когда значения
            // уже совпадают: повторный apply обязан быть полным no-op.
            if (snapshot.Matches(Rect))
                return true;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                UnityEditor.Undo.RecordObject(Rect, "Apply Layout Variant");
#endif
            snapshot.Apply(Rect);
            return true;
        }

        /// <summary>
        /// Возвращает true, только если снапшот реально изменился. Неизменившийся
        /// захват — полный no-op, чтобы не диртить хелпер и не плодить prefab
        /// overrides на инстансах (см. ComponentLayoutVariant.Capture).
        /// </summary>
        public bool Capture(LayoutMode mode)
        {
            if (IsExternallyOwned)
                return false;

            if (mode == LayoutMode.Portrait)
            {
                if (_portraitCaptured && _portrait.Matches(Rect))
                    return false;

                _portrait = RectSnapshot.Capture(Rect);
                _portraitCaptured = true;
            }
            else
            {
                if (_landscapeCaptured && _landscape.Matches(Rect))
                    return false;

                _landscape = RectSnapshot.Capture(Rect);
                _landscapeCaptured = true;
            }

            return true;
        }

        public void ClearVariants()
        {
            _portraitCaptured = false;
            _landscapeCaptured = false;
        }
    }
}
