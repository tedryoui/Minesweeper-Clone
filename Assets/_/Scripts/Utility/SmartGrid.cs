using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace _.Scripts.Utility
{
    [ExecuteAlways]
    public class SmartGrid : MonoBehaviour
    {
        [SerializeField] private GameObject _prefab;
        [SerializeField] private int _columns = 3;
        [SerializeField] private int _rows = 3;
        [SerializeField] private float _spacing = 8f;

        private RectTransform _rectTransform;
        private readonly List<RectTransform> _cells = new();

#region Unity Lifecycle

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            Rebuild();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!gameObject.activeInHierarchy)
                return;

            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this == null) return;
                Rebuild();
            };
        }
#endif

#endregion

#region Public API

        public void Rebuild()
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();

            ClearCells();

            if (_prefab == null || _columns <= 0 || _rows <= 0)
                return;

            var cellSize = CalculateCellSize();

            SpawnCells(cellSize);
        }

#endregion

#region Grid Building

        private float2 CalculateCellSize()
        {
            var parentSize = new float2(
                _rectTransform.rect.width,
                _rectTransform.rect.height
            );

            var totalSpacingX = _spacing * (_columns - 1);
            var totalSpacingY = _spacing * (_rows - 1);

            var availableWidth  = parentSize.x - totalSpacingX;
            var availableHeight = parentSize.y - totalSpacingY;

            var cellFromWidth  = availableWidth  / _columns;
            var cellFromHeight = availableHeight / _rows;

            // Pick the smaller side to maintain 1:1 aspect ratio
            var cellSide = math.min(cellFromWidth, cellFromHeight);

            return new float2(cellSide, cellSide);
        }

        private void SpawnCells(float2 cellSize)
        {
            var gridSize = new float2(
                cellSize.x * _columns + _spacing * (_columns - 1),
                cellSize.y * _rows    + _spacing * (_rows    - 1)
            );

            var originX = -gridSize.x * 0.5f + cellSize.x * 0.5f;
            var originY =  gridSize.y * 0.5f - cellSize.y * 0.5f;

            for (var row = 0; row < _rows; row++)
            {
                for (var col = 0; col < _columns; col++)
                {
                    var cell = SpawnCell();

                    cell.sizeDelta = new Vector2(cellSize.x, cellSize.y);
                    cell.anchoredPosition = new Vector2(
                        originX + col * (cellSize.x + _spacing),
                        originY - row * (cellSize.y + _spacing)
                    );

                    _cells.Add(cell);
                }
            }
        }

        private RectTransform SpawnCell()
        {
            var instance = Instantiate(_prefab, _rectTransform);
            var rect = instance.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot     = new Vector2(0.5f, 0.5f);

            return rect;
        }

        private void ClearCells()
        {
            foreach (var cell in _cells)
            {
                if (cell == null) continue;

#if UNITY_EDITOR
                DestroyImmediate(cell.gameObject);
#else
                Destroy(cell.gameObject);
#endif
            }

            _cells.Clear();

            // Catch any orphaned children that weren't tracked (e.g. after undo)
            for (var i = _rectTransform.childCount - 1; i >= 0; i--)
            {
                var child = _rectTransform.GetChild(i);

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