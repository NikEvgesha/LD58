using System;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralGeneration : ManagedBehaviour
{
    [Header("Параметры генерации")]
    [SerializeField, Min(1)] private int _width = 8;
    [SerializeField, Min(1)] private int _height = 8;
    [SerializeField, Min(0.1f)] private float _cellSize = 10f;

    [Tooltip("Если 0 — возьмём случайное значение. Одинаковый сид = одинаковая карта.")]
    [SerializeField] private int _seed = 0;

    [Header("Префабы тайлов")]
    [SerializeField] private List<LocationTile> _locationTiles = new();

    [Header("Стартовый тайл (обязательный)")]
    [SerializeField] private LocationTile _startTile; // <- добавили сюда

    [Header("Опции")]
    [SerializeField] private bool _clearChildrenOnGenerate = true;
    [SerializeField] private bool _generateOnStart = true;
    [SerializeField] private bool _log = false;

    private System.Random _rng;
    private class Placed { public LocationTile prefab; public int rot; public GameObject go; }
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
        if (_startTile == null)
        {
            Debug.LogError("Не задан Start Tile!");
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

        // 1️⃣ ставим заданный стартовый тайл в центр
        var (cx, cy) = (_width / 2, _height / 2);
        if (!PlaceGivenStartTile(cx, cy))
        {
            Debug.LogWarning("Не удалось поставить стартовый тайл.");
            return;
        }

        // 2️⃣ остальное — обычный бэктрекинг
        bool ok = BacktrackPlace(0, 0);
        if (!ok) Debug.LogWarning("Не удалось собрать карту, попробуй другой сид.");
        else if (_log) Debug.Log("Готово!");
    }

    private bool PlaceGivenStartTile(int x, int y)
    {
        // ищем вариант поворота, где ровно 1 открытая сторона
        foreach (int rot in EnumerateRotations(_startTile))
        {
            var mask = _startTile.MaskWithRotation(rot);
            if (CountBits(mask) != 1) continue;

            var go = Instantiate(_startTile.gameObject, IndexToWorld(x, y), RotationFromQuarterTurns(rot), transform);
            _grid[x, y] = new Placed { prefab = _startTile, rot = rot, go = go };
            return true;
        }
        return false;
    }

    private bool BacktrackPlace(int x, int y)
    {
        if (y >= _height) return true;
        int nextX = (x + 1) % _width;
        int nextY = y + ((x + 1) / _width);

        // пропускаем центр
        if (_grid[x, y] != null) return BacktrackPlace(nextX, nextY);

        var pool = WeightedShuffle(_locationTiles, _rng);
        foreach (var tile in pool)
        {
            foreach (int rot in EnumerateRotations(tile))
            {
                var mask = tile.MaskWithRotation(rot);

                if (x == 0        && mask.HasFlag(Openings.West))  continue;
                if (x == _width-1 && mask.HasFlag(Openings.East))  continue;
                if (y == 0        && mask.HasFlag(Openings.South)) continue;
                if (y == _height-1&& mask.HasFlag(Openings.North)) continue;

                if (!MatchesNeighbors(x, y, mask)) continue;

                var go = Instantiate(tile.gameObject, IndexToWorld(x, y), RotationFromQuarterTurns(rot), transform);
                _grid[x, y] = new Placed { prefab = tile, rot = rot, go = go };

                var lt = go.GetComponent<LocationTile>();
                lt?.SetupRuntime(_rng);

                if (BacktrackPlace(nextX, nextY)) return true;

                DestroyImmediate(go);
                _grid[x, y] = null;
            }
        }

        return false;
    }

    private bool MatchesNeighbors(int x, int y, Openings maskHere)
    {
        if (x - 1 >= 0 && _grid[x - 1, y] != null)
        {
            var leftMask = _grid[x - 1, y].prefab.MaskWithRotation(_grid[x - 1, y].rot);
            bool needConnect = leftMask.HasFlag(Openings.East);
            if (needConnect != maskHere.HasFlag(Openings.West)) return false;
        }
        if (x + 1 < _width && _grid[x + 1, y] != null)
        {
            var rightMask = _grid[x + 1, y].prefab.MaskWithRotation(_grid[x + 1, y].rot);
            bool needConnect = rightMask.HasFlag(Openings.West);
            if (needConnect != maskHere.HasFlag(Openings.East)) return false;
        }
        if (y - 1 >= 0 && _grid[x, y - 1] != null)
        {
            var bottomMask = _grid[x, y - 1].prefab.MaskWithRotation(_grid[x, y - 1].rot);
            bool needConnect = bottomMask.HasFlag(Openings.North);
            if (needConnect != maskHere.HasFlag(Openings.South)) return false;
        }
        if (y + 1 < _height && _grid[x, y + 1] != null)
        {
            var topMask = _grid[x, y + 1].prefab.MaskWithRotation(_grid[x, y + 1].rot);
            bool needConnect = topMask.HasFlag(Openings.South);
            if (needConnect != maskHere.HasFlag(Openings.North)) return false;
        }
        return true;
    }

    private int CountBits(Openings m)
    {
        int c = 0;
        if (m.HasFlag(Openings.North)) c++;
        if (m.HasFlag(Openings.East))  c++;
        if (m.HasFlag(Openings.South)) c++;
        if (m.HasFlag(Openings.West))  c++;
        return c;
    }

    private IEnumerable<int> EnumerateRotations(LocationTile tile)
    {
        if (!tile.AllowRotation)
        {
            yield return 0;
            yield break;
        }
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
                Gizmos.DrawWireCube(IndexToWorld(x, y), new Vector3(_cellSize, 0.05f, _cellSize));
        }
    }
#endif
}
