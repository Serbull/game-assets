using Serbull.ResponsiveUI;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace Serbull.ResponsiveUI.EditorTools
{
    /// <summary>
    /// Тумблер режима прямо в Scene view. Включается через меню оверлеев
    /// Scene view (правый клик по панели оверлеев -> Responsive Layout).
    /// </summary>
    [Overlay(typeof(SceneView), "Responsive Layout")]
    public class ResponsiveSceneOverlay : Overlay
    {
        private Button _button;
        private Label _status;

        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();
            root.style.minWidth = 220f;

            // Статусная строка против человеческого фактора: всегда видно,
            // в каком режиме сейчас экран, даже без выбранного объекта.
            _status = new Label();
            _status.style.unityFontStyleAndWeight = FontStyle.Bold;
            _status.style.whiteSpace = WhiteSpace.Normal;
            _status.style.paddingLeft = 4f;
            _status.style.paddingRight = 4f;
            _status.style.paddingTop = 2f;
            _status.style.paddingBottom = 2f;
            root.Add(_status);

            _button = new Button(OnClick);
            root.Add(_button);

            Refresh();
            ScreenLayoutMode.Changed += OnModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;

            return root;
        }

        public override void OnWillBeDestroyed()
        {
            ScreenLayoutMode.Changed -= OnModeChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            base.OnWillBeDestroyed();
        }

        private void OnModeChanged(LayoutMode mode) => Refresh();

        private void OnPlayModeChanged(PlayModeStateChange state) => Refresh();

        private void OnClick()
        {
            ResponsiveEditorMode.Toggle();
            Refresh();
        }

        private void Refresh()
        {
            if (_button == null)
                return;

            bool playDriven = ResponsiveEditorMode.IsPlayDriven;
            bool portrait = ResponsiveEditorMode.EffectiveMode == LayoutMode.Portrait;

            // В Play Mode режим диктует aspect ratio Game view — тумблер гасим.
            _button.SetEnabled(!playDriven);
            _button.text = playDriven
                ? (portrait ? "▯ Portrait  (auto)" : "▭ Landscape  (auto)")
                : (portrait ? "▯ Portrait  (F9)" : "▭ Landscape  (F9)");
            _button.style.backgroundColor = portrait
                ? new Color(0.24f, 0.42f, 0.55f)
                : new Color(0.45f, 0.38f, 0.22f);

            if (_status != null)
            {
                if (playDriven)
                {
                    _status.text = portrait
                        ? "PLAY MODE — screen in VERTICAL (Game view aspect)"
                        : "PLAY MODE — screen in HORIZONTAL (Game view aspect)";
                }
                else
                {
                    _status.text = portrait
                        ? "Now screen working in VERTICAL mode"
                        : "Now screen working in HORIZONTAL mode";
                }

                _status.style.color = portrait
                    ? new Color(0.4f, 0.75f, 1f)
                    : new Color(1f, 0.75f, 0.3f);
            }
        }
    }
}
