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

        private readonly List<RectTransform> _children = new List<RectTransform>();
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
            CollectChildren();

            int count = _children.Count;
            if (count == 0)
            {
                _displayedFit = 1f;
                return;
            }

            float availableWidth = Rect.rect.width - (_padding != null ? _padding.horizontal : 0);
            float availableHeight = Rect.rect.height - (_padding != null ? _padding.vertical : 0);

            // Columns, rows and the fit factor are solved together: shrinking cells to
            // fit the height frees up horizontal space, which lets more columns fit, so
            // they can't be computed independently.
            SolveLayout(availableWidth, availableHeight, count, out int columns, out int rows, out float targetFit);

            if (snap || _fitSmoothSpeed <= 0f)
                _displayedFit = targetFit;
            else
                _displayedFit = Mathf.Lerp(_displayedFit, targetFit, 1f - Mathf.Exp(-_fitSmoothSpeed * dt));

            float fit = _displayedFit;
            Vector2 cell = _cellSize * fit;
            Vector2 space = _spacing * fit;

            // --- vertical placement (whole block centered) ----------------------
            float gridHeight = rows * cell.y + (rows - 1) * space.y;
            float padOffsetY = _padding != null ? (_padding.bottom - _padding.top) * 0.5f : 0f;
            float padOffsetX = _padding != null ? (_padding.left - _padding.right) * 0.5f : 0f;
            float topRowY = gridHeight * 0.5f - cell.y * 0.5f + padOffsetY;

            // --- per child reveal + placement -----------------------------------
            for (int row = 0; row < rows; row++)
            {
                int start = row * columns;
                int end = Mathf.Min(start + columns, count);
                float rowY = topRowY - row * (cell.y + space.y);

                // First pass: compute eased reveal per child and total row width.
                float rowWidth = 0f;
                for (int i = start; i < end; i++)
                {
                    float eased = StepReveal(_children[i], dt, snap);
                    float w = cell.x * eased;
                    if (i > start)
                        rowWidth += space.x * eased; // spacing also fades in with the child
                    rowWidth += w;
                }

                // Second pass: place children, centering the row.
                float cursor = -rowWidth * 0.5f + padOffsetX;
                for (int i = start; i < end; i++)
                {
                    RectTransform child = _children[i];
                    float eased = _reveal.TryGetValue(child, out float p) ? Evaluate(p) : 1f;
                    float w = cell.x * eased;

                    if (i > start)
                        cursor += space.x * eased;

                    float centerX = cursor + w * 0.5f;
                    cursor += w;

                    ApplyChild(child, new Vector2(centerX, rowY), eased * fit);
                }
            }
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

        private void CollectChildren()
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

            // Drop reveal entries for children that no longer exist.
            if (_reveal.Count > 0)
            {
                _staleBuffer.Clear();
                foreach (var kvp in _reveal)
                {
                    if (!_children.Contains(kvp.Key))
                        _staleBuffer.Add(kvp.Key);
                }
                for (int i = 0; i < _staleBuffer.Count; i++)
                    _reveal.Remove(_staleBuffer[i]);
            }
        }

        private readonly List<RectTransform> _staleBuffer = new List<RectTransform>();

        private float StepReveal(RectTransform child, float dt, bool snap)
        {
            bool animate = _animateAppear && _revealDuration > 0f && !snap;

            if (!_reveal.TryGetValue(child, out float p))
            {
                // Newly seen child: start invisible only when animating, else full size.
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
            return Evaluate(p);
        }

        private float Evaluate(float p)
        {
            return _revealCurve != null ? _revealCurve.Evaluate(p) : p;
        }

        private void ApplyChild(RectTransform child, Vector2 anchoredPos, float scale)
        {
            if (_controlChildAnchors)
            {
                child.anchorMin = child.anchorMax = child.pivot = new Vector2(0.5f, 0.5f);
            }
            if (_controlChildSize)
            {
                child.sizeDelta = _cellSize;
            }

            child.anchoredPosition = anchoredPos;
            child.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
