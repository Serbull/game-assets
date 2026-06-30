using System.Collections.Generic;
using UnityEngine;

namespace Serbull.GameAssets.Utils
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public class DynamicGridLayout : MonoBehaviour
    {
        [Header("Grid")]
        [Tooltip("Hard cap on columns. 0 = unlimited (only the container width decides).")]
        [SerializeField] private int _maxColumns = 0;
        [SerializeField] private Vector2 _cellSize = new Vector2(160f, 160f);
        [SerializeField] private Vector2 _spacing = new Vector2(20f, 20f);
        [SerializeField] private RectOffset _padding;

        [Header("Fit")]
        [Tooltip("Shrink cells & spacing so all rows fit the container height.")]
        [SerializeField] private bool _fitToHeight = true;
        [Tooltip("Shrink cells & spacing so the widest row fits the container width.")]
        [SerializeField] private bool _fitToWidth = true;
        [Tooltip("Smoothing speed of the fit factor (resize). 0 = instant.")]
        [SerializeField] private float _fitSmoothSpeed = 10f;

        [Header("Appear animation")]
        [Tooltip("If off, new children show instantly at full size with no grow animation.")]
        [SerializeField] private bool _animateAppear = true;
        [Tooltip("Seconds for a newly added child to grow from scale 0 to full. 0 = instant.")]
        [SerializeField, ShowIf(nameof(_animateAppear))] private float _revealDuration = 0.25f;
        [Tooltip("Maps reveal progress (0..1) to scale (0..1). Leave default for a soft ease.")]
        [SerializeField, ShowIf(nameof(_animateAppear))] private AnimationCurve _revealCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Header("Child control")]
        [SerializeField] private bool _controlChildAnchors = true;
        [SerializeField] private bool _controlChildSize = true;

        private RectTransform _rect;
        private float _displayedFit = 1f;

        // When true, the grid is fully at rest (fit reached, nothing revealing) and
        // LateUpdate skips the whole placement pass until something changes.
        private bool _settled;
        private Vector2 _lastRectSize = new Vector2(-1f, -1f);

        private readonly List<RectTransform> _children = new List<RectTransform>();
        private readonly List<RectTransform> _prevChildren = new List<RectTransform>();
        private readonly HashSet<RectTransform> _currentSet = new HashSet<RectTransform>();
        private readonly List<RectTransform> _removeBuffer = new List<RectTransform>();
        private readonly List<float> _easedBuffer = new List<float>();
        private readonly Dictionary<RectTransform, float> _reveal = new Dictionary<RectTransform, float>();

        private RectTransform Rect => _rect != null ? _rect : (_rect = (RectTransform)transform);

        private void OnEnable()
        {
            // Snap immediately when (re)enabled so we don't lerp from a stale state.
            UpdateLayout(0f, true);
        }

        private void LateUpdate()
        {
            UpdateLayout(Time.unscaledDeltaTime, false);
        }

        private void UpdateLayout(float dt, bool snap)
        {
            bool structureChanged = CollectChildren();

            int count = _children.Count;
            if (count == 0)
            {
                _displayedFit = 1f;
                _settled = false;
                return;
            }

            Vector2 rectSize = Rect.rect.size;
            bool rectChanged = rectSize != _lastRectSize;
            _lastRectSize = rectSize;

            // Nothing moved, nothing appearing, size unchanged → skip the whole pass.
            if (_settled && !structureChanged && !rectChanged && !snap)
                return;

            float availableWidth = rectSize.x - (_padding != null ? _padding.horizontal : 0);
            float availableHeight = rectSize.y - (_padding != null ? _padding.vertical : 0);

            // Columns, rows and the fit factor are solved together: shrinking cells to
            // fit the height frees up horizontal space, which lets more columns fit, so
            // they can't be computed independently.
            SolveLayout(availableWidth, availableHeight, count, out int columns, out int rows, out float targetFit);

            if (snap || _fitSmoothSpeed <= 0f)
                _displayedFit = targetFit;
            else
                _displayedFit = Mathf.Lerp(_displayedFit, targetFit, 1f - Mathf.Exp(-_fitSmoothSpeed * dt));

            // Step every child's reveal once (single curve eval), cache the eased value.
            bool anyRevealing = false;
            for (int i = 0; i < count; i++)
            {
                float p = StepReveal(_children[i], dt, snap);
                _easedBuffer[i] = Evaluate(p);
                if (p < 1f - 0.0001f)
                    anyRevealing = true;
            }

            float fit = _displayedFit;
            Vector2 cell = _cellSize * fit;
            Vector2 space = _spacing * fit;

            // --- vertical placement (whole block centered) ----------------------
            float gridHeight = rows * cell.y + (rows - 1) * space.y;
            float padOffsetY = _padding != null ? (_padding.bottom - _padding.top) * 0.5f : 0f;
            float padOffsetX = _padding != null ? (_padding.left - _padding.right) * 0.5f : 0f;
            float topRowY = gridHeight * 0.5f - cell.y * 0.5f + padOffsetY;

            for (int row = 0; row < rows; row++)
            {
                int start = row * columns;
                int end = Mathf.Min(start + columns, count);
                float rowY = topRowY - row * (cell.y + space.y);

                // First pass: total row width from cached eased values.
                float rowWidth = 0f;
                for (int i = start; i < end; i++)
                {
                    float eased = _easedBuffer[i];
                    if (i > start)
                        rowWidth += space.x * eased; // spacing also fades in with the child
                    rowWidth += cell.x * eased;
                }

                // Second pass: place children, centering the row.
                float cursor = -rowWidth * 0.5f + padOffsetX;
                for (int i = start; i < end; i++)
                {
                    float eased = _easedBuffer[i];
                    float w = cell.x * eased;

                    if (i > start)
                        cursor += space.x * eased;

                    float centerX = cursor + w * 0.5f;
                    cursor += w;

                    ApplyChild(_children[i], new Vector2(centerX, rowY), eased * fit);
                }
            }

            // At rest once the fit has converged and no child is still growing in.
            bool fitDone = Mathf.Abs(_displayedFit - targetFit) < 0.0005f;
            if (fitDone)
                _displayedFit = targetFit;
            _settled = fitDone && !anyRevealing && !structureChanged && !rectChanged;
        }

        /// <summary>
        /// Jointly picks the column count and the fit factor. Every possible column
        /// count is tried; the one allowing the largest fit (least shrinking) wins.
        /// This way, when cells must shrink to fit the height, the extra horizontal
        /// room is used to pack more columns instead of staying at the full-size count.
        /// </summary>
        private void SolveLayout(float availableWidth, float availableHeight, int count,
            out int columns, out int rows, out float fit)
        {
            int maxColumns = count;
            if (_maxColumns > 0)
                maxColumns = Mathf.Min(maxColumns, _maxColumns);
            maxColumns = Mathf.Max(1, maxColumns);

            columns = 1;
            fit = -1f;

            for (int c = 1; c <= maxColumns; c++)
            {
                float f = 1f;

                if (_fitToWidth && availableWidth > 0f)
                {
                    float neededWidth = c * _cellSize.x + (c - 1) * _spacing.x;
                    if (neededWidth > availableWidth)
                        f = Mathf.Min(f, availableWidth / neededWidth);
                }

                int rowsForC = Mathf.CeilToInt(count / (float)c);
                if (_fitToHeight && availableHeight > 0f)
                {
                    float neededHeight = rowsForC * _cellSize.y + (rowsForC - 1) * _spacing.y;
                    if (neededHeight > availableHeight)
                        f = Mathf.Min(f, availableHeight / neededHeight);
                }

                // Largest fit wins; on a tie prefer more columns (tighter width packing).
                if (f > fit + 0.0001f || (f >= fit - 0.0001f && c > columns))
                {
                    fit = f;
                    columns = c;
                }
            }

            fit = Mathf.Clamp01(fit);
            rows = Mathf.CeilToInt(count / (float)columns);
        }

        /// <summary>Rebuilds the active-children list; returns true if it changed since last frame.</summary>
        private bool CollectChildren()
        {
            _children.Clear();
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                Transform t = transform.GetChild(i);
                if (!t.gameObject.activeSelf)
                    continue;
                if (t is RectTransform rt)
                    _children.Add(rt);
            }

            int count = _children.Count;

            bool changed = count != _prevChildren.Count;
            if (!changed)
            {
                for (int i = 0; i < count; i++)
                {
                    if (_children[i] != _prevChildren[i])
                    {
                        changed = true;
                        break;
                    }
                }
            }

            if (changed)
            {
                // Sync the previous-frame snapshot and reveal dictionary (O(n), runs only on change).
                _prevChildren.Clear();
                _prevChildren.AddRange(_children);

                if (_reveal.Count > 0)
                {
                    _currentSet.Clear();
                    for (int i = 0; i < count; i++)
                        _currentSet.Add(_children[i]);

                    _removeBuffer.Clear();
                    foreach (var key in _reveal.Keys)
                    {
                        if (!_currentSet.Contains(key))
                            _removeBuffer.Add(key);
                    }
                    for (int i = 0; i < _removeBuffer.Count; i++)
                        _reveal.Remove(_removeBuffer[i]);
                }
            }

            while (_easedBuffer.Count < count)
                _easedBuffer.Add(0f);

            return changed;
        }

        private float StepReveal(RectTransform child, float dt, bool snap)
        {
            bool animate = _animateAppear && _revealDuration > 0f && !snap;

            if (!_reveal.TryGetValue(child, out float p))
            {
                // First time we see this child: apply its fixed transform setup once.
                SetupChild(child);
                // Start invisible only when animating, else full size.
                p = animate ? 0f : 1f;
            }
            else if (animate)
            {
                p = Mathf.MoveTowards(p, 1f, dt / _revealDuration);
            }
            else
            {
                p = 1f;
            }

            _reveal[child] = p;
            return p;
        }

        private float Evaluate(float p)
        {
            return _revealCurve != null ? _revealCurve.Evaluate(p) : p;
        }

        // Anchors and size never change per frame, so set them only when a child first appears.
        private void SetupChild(RectTransform child)
        {
            if (_controlChildAnchors)
                child.anchorMin = child.anchorMax = child.pivot = new Vector2(0.5f, 0.5f);
            if (_controlChildSize)
                child.sizeDelta = _cellSize;
        }

        private void ApplyChild(RectTransform child, Vector2 anchoredPos, float scale)
        {
            child.anchoredPosition = anchoredPos;
            child.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
