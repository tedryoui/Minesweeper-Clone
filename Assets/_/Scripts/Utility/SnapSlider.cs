using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _.Scripts.UI
{
    /// <summary>
    /// Слайдер с целочисленным значением [MinValue..MaxValue].
    /// Визуально — N копий сегмента-прототипа, равномерно распределённых по ширине.
    /// Включённые сегменты (≤ Value) отображаются оригинально, выключенные затемнены.
    /// Drag меняет значение через дельту позиции пальца/мыши, при отпускании — snap.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class SnapSlider : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform _segmentPrototype;

        [SerializeField] private int   _minValue = 0;
        [SerializeField] private int   _maxValue = 5;
        [SerializeField] private int   _value    = 0;
        [SerializeField] private float _spacing  = 8f;

        [Header("Dimming")]
        [SerializeField] private Color _activeColor   = Color.white;
        [SerializeField] private Color _inactiveColor = new Color(0.25f, 0.25f, 0.25f, 1f);

        /// <summary>Срабатывает только при реальной смене значения после snap.</summary>
        public event Action<int> OnValueChanged;

        public int Value
        {
            get => _value;
            set => ApplyValue(math.clamp(value, _minValue, _maxValue));
        }

        // ── приватное состояние ──────────────────────────────────────────────

        private RectTransform              _rectTransform;
        private Canvas                     _canvas;
        private readonly List<RectTransform> _segments = new List<RectTransform>();

        private bool  _isDragging;
        private float _dragAccumulator;   // накопленная дробная дельта в шагах
        private float _pointerPrevX;      // предыдущая X в локальных координатах трека

        private int Steps => math.max(1, _maxValue - _minValue);

#region Unity Lifecycle

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _canvas        = GetComponentInParent<Canvas>();
        }

        private void Start()
        {
            _value = math.clamp(_value, _minValue, _maxValue);
            Rebuild();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!gameObject.activeInHierarchy) return;

            _minValue = math.min(_minValue, _maxValue);
            _value    = math.clamp(_value, _minValue, _maxValue);

            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                Rebuild();
            };
        }
#endif

#endregion

#region Public API

        public void SetLimits(int min, int max)
        {
            _minValue = min;
            _maxValue = math.max(min + 1, max);
            ApplyValue(math.clamp(_value, _minValue, _maxValue));
            Rebuild();
        }

        /// <summary>Пересоздаёт все копии сегментов из прототипа.</summary>
        public void Rebuild()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            DestroySegments();

            if (_segmentPrototype == null) return;

            _segmentPrototype.gameObject.SetActive(false);

            var count    = Steps + 1;
            var cellSize = CalculateCellSize(count);

            for (var i = 0; i < count; i++)
            {
                var copy = Instantiate(_segmentPrototype, _rectTransform);
                copy.name = $"__seg_{i}";
                copy.gameObject.SetActive(true);

                copy.anchorMin        = new Vector2(0.5f, 0.5f);
                copy.anchorMax        = new Vector2(0.5f, 0.5f);
                copy.pivot            = new Vector2(0.5f, 0.5f);
                copy.sizeDelta        = new Vector2(cellSize, cellSize);
                copy.anchoredPosition = SegmentPosition(i, count, cellSize);

                _segments.Add(copy);
            }

            RefreshTint();
        }

#endregion

#region Pointer Events

        public void OnPointerDown(PointerEventData eventData)
        {
            _isDragging      = true;
            _dragAccumulator = _value - _minValue;   // стартуем с текущего целого
            _pointerPrevX    = LocalX(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_isDragging) return;

            var currentX   = LocalX(eventData);
            var deltaX     = currentX - _pointerPrevX;
            _pointerPrevX  = currentX;

            // Переводим пиксели в шаги: весь трек соответствует Steps шагам
            var trackWidth = _rectTransform.rect.width;
            var deltaSteps = deltaX / trackWidth * Steps;

            _dragAccumulator = math.clamp(_dragAccumulator + deltaSteps, 0f, Steps);

            // Превью без снэпа — красим по дробному значению
            RefreshTint(_minValue + _dragAccumulator);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _isDragging = false;

            var snapped = math.clamp(
                _minValue + (int)math.round(_dragAccumulator),
                _minValue,
                _maxValue
            );

            ApplyValue(snapped);
        }

#endregion

#region Layout Math

        private float CalculateCellSize(int count)
        {
            var totalWidth   = _rectTransform.rect.width;
            var totalHeight  = _rectTransform.rect.height;
            var totalSpacing = _spacing * (count - 1);

            var fromWidth  = (totalWidth - totalSpacing) / count;
            var fromHeight = totalHeight;

            return math.min(fromWidth, fromHeight);
        }

        private Vector2 SegmentPosition(int index, int count, float cellSize)
        {
            var totalWidth = cellSize * count + _spacing * (count - 1);
            var startX     = -totalWidth * 0.5f + cellSize * 0.5f;

            return new Vector2(startX + index * (cellSize + _spacing), 0f);
        }

        private float LocalX(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _rectTransform,
                eventData.position,
                _canvas != null && _canvas.renderMode != RenderMode.ScreenSpaceOverlay
                    ? _canvas.worldCamera
                    : null,
                out var local
            );
            return local.x;
        }

#endregion

#region Visuals

        private void ApplyValue(int newValue)
        {
            if (_value == newValue) return;

            _value = newValue;
            RefreshTint();
            OnValueChanged?.Invoke(_value);
        }

        /// <summary>Красит по текущему целому значению.</summary>
        private void RefreshTint() => RefreshTint(_value);

        /// <summary>Красит по дробному значению — для превью во время drag.</summary>
        private void RefreshTint(float floatValue)
        {
            for (var i = 0; i < _segments.Count; i++)
            {
                if (_segments[i] == null) continue;

                // Сегмент i активен если его порядковый номер попадает под floatValue
                var active = (_minValue + i) <= floatValue;
                SetSegmentColor(_segments[i], active ? _activeColor : _inactiveColor);
            }
        }

        private void SetSegmentColor(RectTransform segment, Color color)
        {
            foreach (var graphic in segment.GetComponentsInChildren<Graphic>(true))
                graphic.color = color;
        }

        private void DestroySegments()
        {
            foreach (var seg in _segments)
            {
                if (seg == null) continue;
#if UNITY_EDITOR
                DestroyImmediate(seg.gameObject);
#else
                Destroy(seg.gameObject);
#endif
            }

            _segments.Clear();

            // Подчищаем возможные сироты после undo / hot-reload
            for (var i = _rectTransform.childCount - 1; i >= 0; i--)
            {
                var child = _rectTransform.GetChild(i);
                if (!child.name.StartsWith("__seg_")) continue;
#if UNITY_EDITOR
                DestroyImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif
            }
        }

#endregion
    }
}