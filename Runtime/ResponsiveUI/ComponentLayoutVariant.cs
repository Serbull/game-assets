using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Serbull.ResponsiveUI
{
    /// <summary>
    /// Универсальный хелпер альтернативного layout для ЛЮБОГО компонента:
    /// GridLayoutGroup, LayoutElement, HorizontalLayoutGroup, ContentSizeFitter,
    /// Image, TMP_Text и т.д. Снапшот — плоский список SerializedProperty-пар,
    /// поэтому покрываются все сериализованные поля без ручного перечисления.
    ///
    /// На объекте может быть несколько таких хелперов — по одному на компонент,
    /// поэтому DisallowMultipleComponent здесь намеренно НЕ ставится.
    /// </summary>
    [ExecuteAlways]
    [AddComponentMenu("Responsive UI/Component Layout Variant")]
    public class ComponentLayoutVariant : MonoBehaviour
    {
        [Tooltip("Компонент, чьи поля варьируются между портретом и ландшафтом.")]
        [SerializeField] private Component _target;

        [SerializeField, HideInInspector] private string _portraitJson;
        [SerializeField, HideInInspector] private string _landscapeJson;
        [SerializeField, HideInInspector] private bool _portraitCaptured;
        [SerializeField, HideInInspector] private bool _landscapeCaptured;
        [SerializeField] private bool _variantEnabled = true;

        private bool _subscribed;

        // Кэш разобранных снапшотов: строка парсится один раз, а не на каждую
        // смену ориентации. Non-serialized — пересобирается после domain reload
        // и сбрасывается при каждом Capture.
        [System.NonSerialized] private KeyValuePair<string, string>[] _portraitPairs;
        [System.NonSerialized] private KeyValuePair<string, string>[] _landscapePairs;

        public Component Target
        {
            get => _target;
            set => _target = value;
        }

        public bool HasPortrait => _portraitCaptured;
        public bool HasLandscape => _landscapeCaptured;
        public bool VariantEnabled => _variantEnabled;

        /// <summary>
        /// true, когда хелпер принадлежит prefab-ассету, а мы смотрим на него
        /// снаружи: инстанс в сцене либо вложенный префаб в чужом Prefab Mode.
        /// Гейтится ТОЛЬКО Capture: снапшоты такого хелпера редактируются в
        /// Prefab Mode его собственного ассета, иначе автозахват штампует
        /// overrides на полях хелпера. Apply работает всегда — иначе инстансы
        /// префабов перестают адаптироваться при смене режима экрана.
        /// В Play Mode гейт не действует — там изменения не персистятся.
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

                // Корень инстанса — сценовые данные: его размещение и слоты
                // принадлежат сцене, поэтому редактирование и автозахват там
                // разрешены (пишутся оверрайдом хелпера этого инстанса).
                // Иначе префаб невозможно даже подвинуть — apply возвращает
                // его в снапшот. Заблокированы только внутренности префаба.
                return !UnityEditor.PrefabUtility.IsOutermostPrefabInstanceRoot(gameObject);
#else
                return false;
#endif
            }
        }

        private void OnEnable()
        {
#if UNITY_EDITOR
            // Во время импорта префаба OnEnable вызывается на persistent-объекте;
            // применение варианта там диртит импортируемый ассет и даёт
            // "Problem detected while importing the Prefab file".
            if (!Application.isPlaying && UnityEditor.EditorUtility.IsPersistent(this))
                return;
#endif
            if (!_subscribed)
            {
                ScreenLayoutMode.Changed += OnModeChanged;
                _subscribed = true;
            }

#if UNITY_EDITOR
            // Открытие/переключение Prefab Stage не является сменой layout:
            // автоматический Apply здесь создавал изменения ассета без действий
            // пользователя и диалог Save/Discard при переходе к другому prefab.
            if (IsInPrefabStage())
                return;

            // Обычный Edit Mode OnEnable (открытие сцены, импорт, domain reload)
            // не является авторским переключением layout. Иначе сериализованные
            // поля компонентов внутри prefab-инстансов записываются в сцену как
            // случайные overrides. F9 продолжает применять вариант через
            // OnModeChanged, а Play Mode — здесь по реальному aspect ratio.
            if (!Application.isPlaying)
                return;
#endif
            Apply(ScreenLayoutMode.Current);
        }

#if UNITY_EDITOR
        // Revert оверрайдов и prefab-propagation меняют сериализованные поля в
        // обход Capture — кэш разобранных пар обязан сброситься, иначе Apply
        // продолжит применять устаревший снапшот.
        private void OnValidate()
        {
            _portraitPairs = null;
            _landscapePairs = null;
        }
#endif

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
            if (Application.isPlaying && IsInPrefabStage())
                return;
#endif
            Apply(mode);
        }

#if UNITY_EDITOR
        private bool IsInPrefabStage()
        {
            return UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(gameObject) != null;
        }
#endif

        public bool Apply(LayoutMode mode)
        {
            if (!_variantEnabled || _target == null)
                return false;

            bool captured = mode == LayoutMode.Portrait ? _portraitCaptured : _landscapeCaptured;
            if (!captured)
                return false;

            KeyValuePair<string, string>[] pairs;
            if (mode == LayoutMode.Portrait)
            {
                pairs = _portraitPairs ??
                        (_portraitPairs = LayoutVariantSerialization.ParsePairs(_portraitJson));
            }
            else
            {
                pairs = _landscapePairs ??
                        (_landscapePairs = LayoutVariantSerialization.ParsePairs(_landscapeJson));
            }

            if (pairs.Length == 0)
                return false;

            LayoutVariantSerialization.Apply(_target, pairs);
            MarkLayoutDirty();
            return true;
        }

        /// <summary>
        /// Возвращает true, только если снапшот реально изменился. Неизменившийся
        /// захват обязан быть полным no-op: иначе каждый автозахват диртит хелпер,
        /// а для хелпера из вложенного префаба — штампует property override
        /// на внешнем инстансе.
        /// </summary>
        public bool Capture(LayoutMode mode)
        {
            if (_target == null || IsExternallyOwned)
                return false;

            string json = LayoutVariantSerialization.Capture(_target);
            if (string.IsNullOrEmpty(json))
                return false;

            if (mode == LayoutMode.Portrait)
            {
                if (_portraitCaptured && json == _portraitJson)
                    return false;

                _portraitJson = json;
                _portraitCaptured = true;
                _portraitPairs = null;
            }
            else
            {
                if (_landscapeCaptured && json == _landscapeJson)
                    return false;

                _landscapeJson = json;
                _landscapeCaptured = true;
                _landscapePairs = null;
            }

            return true;
        }

        public void ClearVariants()
        {
            _portraitJson = null;
            _landscapeJson = null;
            _portraitCaptured = false;
            _landscapeCaptured = false;
            _portraitPairs = null;
            _landscapePairs = null;
        }

        /// <summary>
        /// Layout- и graphic-компоненты не перестраиваются сами после прямой
        /// записи сериализованных полей, поэтому помечаем их на пересчёт явно.
        /// </summary>
        private void MarkLayoutDirty()
        {
            if (_target is Graphic graphic)
            {
                graphic.SetAllDirty();
                return;
            }

            if (_target is LayoutGroup || _target is LayoutElement || _target is ContentSizeFitter)
            {
                var rect = transform as RectTransform;
                if (rect != null)
                    LayoutRebuilder.MarkLayoutForRebuild(rect);
            }
        }
    }
}
