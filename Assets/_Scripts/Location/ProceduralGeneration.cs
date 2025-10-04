using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ProceduralGeneration : MonoBehaviour
{
    [Header("Параметры генерации")]
    [SerializeField, Min(1)] private int _width = 8;
    [SerializeField, Min(1)] private int _height = 8;
    [SerializeField, Min(0.1f)] private float _cellSize = 10f;

    [Tooltip("Если 0 — возьмём случайное значение. Одинаковый сид = одинаковая карта.")]
    [SerializeField] private int _seed = 0;

    [Header("Префабы тайлов")]
    [SerializeField] private List<LocationTile> _locationTiles = new();

    [Header("Опции")]
    [SerializeField] private bool _clearChildrenOnGenerate = true;
    [SerializeField] private bool _generateOnStart = true;
    [SerializeField] private bool _log = false;

    private System.Random _rng;

    private class Placed
    {
        public LocationTile prefab;
        public int rot; // 0..3 (по 90° CW)
        public GameObject go;
    }

    private Placed[,] _grid;

    private void Start()
    {
        if (_generateOnStart) Generate();
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        if (_locationTiles == null || _locationTiles.Count == 0)
        {
            Debug.LogError("Нет тайлов в _locationTiles.");
            return;
        }

        if (_seed == 0) _seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        _rng = new System.Random(_seed);
        if (_log) Debug.Log($"Seed: {_seed}");

        if (_clearChildrenOnGenerate)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);
        }

        _grid = new Placed[_width, _height];

        bool ok = BacktrackPlace(0, 0);
        if (!ok)
        {
            Debug.LogWarning("Не удалось собрать карту с заданными тайлами/ограничениями. Попробуйте другой сид или набор тайлов.");
        }
        else
        {
            if (_log) Debug.Log("Готово!");
        }
    }

    private bool BacktrackPlace(int x, int y)
    {
        if (y >= _height) return true;

        int nextX = (x + 1) % _width;
        int nextY = y + ((x + 1) / _width);

        // Перемешаем тайлы с учетом веса
        var pool = WeightedShuffle(_locationTiles, _rng);

        foreach (var tile in pool)
        {
            foreach (int rot in EnumerateRotations(tile))
            {
                var mask = tile.MaskWithRotation(rot);

                // Правило: на границе карты не должно быть "дыр"
                if (x == 0        && mask.HasFlag(Openings.West))  continue;
                if (x == _width-1 && mask.HasFlag(Openings.East))  continue;
                if (y == 0        && mask.HasFlag(Openings.South)) continue;
                if (y == _height-1&& mask.HasFlag(Openings.North)) continue;

                // Совместимость с уже поставленными соседями
                if (!MatchesNeighbors(x, y, mask)) continue;

                // Поставить
                var go = Instantiate(tile.gameObject, IndexToWorld(x, y), RotationFromQuarterTurns(rot), transform);
                _grid[x, y] = new Placed { prefab = tile, rot = rot, go = go };

                // Рекурсия дальше
                if (BacktrackPlace(nextX, nextY)) return true;

                // Откат
                DestroyImmediate(go);
                _grid[x, y] = null;
            }
        }

        return false;
    }

    private bool MatchesNeighbors(int x, int y, Openings maskHere)
    {
        // Левый
        if (x - 1 >= 0 && _grid[x - 1, y] != null)
        {
            var leftMask = _grid[x - 1, y].prefab.MaskWithRotation(_grid[x - 1, y].rot);
            bool needConnect = leftMask.HasFlag(Openings.East);
            if (needConnect != maskHere.HasFlag(Openings.West)) return false;
        }

        // Правый
        if (x + 1 < _width && _grid[x + 1, y] != null)
        {
            var rightMask = _grid[x + 1, y].prefab.MaskWithRotation(_grid[x + 1, y].rot);
            bool needConnect = rightMask.HasFlag(Openings.West);
            if (needConnect != maskHere.HasFlag(Openings.East)) return false;
        }

        // Нижний
        if (y - 1 >= 0 && _grid[x, y - 1] != null)
        {
            var bottomMask = _grid[x, y - 1].prefab.MaskWithRotation(_grid[x, y - 1].rot);
            bool needConnect = bottomMask.HasFlag(Openings.North);
            if (needConnect != maskHere.HasFlag(Openings.South)) return false;
        }

        // Верхний
        if (y + 1 < _height && _grid[x, y + 1] != null)
        {
            var topMask = _grid[x, y + 1].prefab.MaskWithRotation(_grid[x, y + 1].rot);
            bool needConnect = topMask.HasFlag(Openings.South);
            if (needConnect != maskHere.HasFlag(Openings.North)) return false;
        }

        return true;
    }

    private IEnumerable<int> EnumerateRotations(LocationTile tile)
    {
        if (!tile.AllowRotation)
        {
            yield return 0;
            yield break;
        }

        // Перемешаем порядок проб
        var rots = new List<int> { 0, 1, 2, 3 };
        for (int i = rots.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (rots[i], rots[j]) = (rots[j], rots[i]);
        }
        foreach (var r in rots) yield return r;
    }

    private List<LocationTile> WeightedShuffle(List<LocationTile> src, System.Random rng)
    {
        // Алгоритм: дублируем в пул по весу (с потолком) и перемешиваем
        var pool = new List<LocationTile>();
        foreach (var t in src)
        {
            int copies = Mathf.Max(1, Mathf.CeilToInt(t.weight));
            for (int i = 0; i < copies; i++) pool.Add(t);
        }
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (pool[i], pool[j]) = (pool[j], pool[i]);
        }
        return pool;
    }

    private Quaternion RotationFromQuarterTurns(int q) =>
        Quaternion.Euler(0f, 90f * ((q % 4 + 4) % 4), 0f);

    private Vector3 IndexToWorld(int x, int y)
    {
        // Центрируем сетку вокруг (0,0,0), Y — вверх
        float offsetX = (_width - 1) * _cellSize * 0.5f;
        float offsetY = (_height - 1) * _cellSize * 0.5f;
        return new Vector3(x * _cellSize - offsetX, 0f, y * _cellSize - offsetY);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
        Gizmos.color = new Color(1, 1, 1, 0.2f);
        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                var p = IndexToWorld(x, y);
                Gizmos.DrawWireCube(p, new Vector3(_cellSize, 0.05f, _cellSize));
            }
        }
    }
#endif
}
