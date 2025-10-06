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
    [SerializeField] private LocationTile _startTile;

    [Header("Опции")]
    [SerializeField] private bool _clearChildrenOnGenerate = true;
    [SerializeField] private bool _generateOnStart = true;
    [SerializeField] private bool _log = false; 
    [SerializeField] private bool _allowStartTileInPool = false; // можно ли класть стартовый тайл в обычный пул
    private int _startIdx; // индекс стартового тайла в пуле

    [SerializeField] private bool _instantiateOverFrames = false;   // опция: размазать инстансы по кадрам
    [SerializeField, Min(1)] private int _instantiatesPerFrame = 64;

    private System.Random _rng;

    // ----- Данные для решения без объектов
    private struct Cell { public short tile; public byte rot; public bool filled; }
    private Cell[,] _gridData;             // только индексы и ротации
    private Openings[,] _maskCache;        // [tileIdx, rot]
    private int _tilesCount;

    // ----- Разовое событие Init
    private bool _inited;
    public void Init()
    {
        if (_inited) return;
        _inited = true;

        if (_generateOnStart) Generate();
        G.Game.GameEnd.AddListener(Generate);
    }

    [ContextMenu("Generate")]
    public void Generate(bool _ = false)
    {
        if (_locationTiles == null || _locationTiles.Count == 0)
        {
            Debug.LogError("Нет тайлов в _locationTiles."); return;
        }
        if (_startTile == null)
        {
            Debug.LogError("Не задан Start Tile!"); return;
        }
        // --- добавляем StartTile во внутренний список, если его там нет
        int startIdx = _locationTiles.IndexOf(_startTile);
        if (startIdx < 0)
        {
            _locationTiles.Insert(0, _startTile); // временно добавляем его в пул
            startIdx = 0;
        }
        _startIdx = startIdx;

        if (_seed == 0) _seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        _rng = new System.Random(_seed);
        if (_log) Debug.Log($"Seed: {_seed}");

        if (_clearChildrenOnGenerate)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
        }

        _tilesCount = _locationTiles.Count;
        BuildMaskCache();               // кэшируем все маски поворотов
        _gridData = new Cell[_width, _height];

        // ставим заданный стартовый тайл (центр)
        int cx = _width / 2, cy = _height / 2;
        if (!PlaceStartData(cx, cy))
        {
            Debug.LogWarning("Не удалось поставить стартовый тайл."); return;
        }

        // решаем остальное БЕЗ инстансов
        if (!BacktrackPlaceData(0, 0))
        {
            Debug.LogWarning("Не удалось собрать карту, попробуй другой сид.");
            return;
        }

        // один проход по сетке: создаём объекты
        if (_instantiateOverFrames)
            StartCoroutine(InstantiateAllOverFrames());
        else
            InstantiateAllImmediate();

        if (_log) Debug.Log("Готово!");
    }

    #region Data phase (no GameObjects)
    private void BuildMaskCache()
    {
        _maskCache = new Openings[_tilesCount, 4];
        for (int i = 0; i < _tilesCount; i++)
        {
            var t = _locationTiles[i];
            // если нельзя крутить — копии будут одинаковые
            for (int r = 0; r < 4; r++)
                _maskCache[i, r] = t.MaskWithRotation(r);
        }
    }

    private bool PlaceStartData(int x, int y)
    {
        int idx = _startIdx;

        if (idx < 0) { Debug.LogError("StartTile не найден в списке _locationTiles."); return false; }

        var rots = EnumerateRotationsIndices(_startTile);
        foreach (var r in rots)
        {
            var mask = _maskCache[idx, r];
            if (CountBits(mask) != 1) continue;
            _gridData[x, y] = new Cell { tile = (short)idx, rot = (byte)r, filled = true };
            return true;
        }
        return false;
    }

    private bool BacktrackPlaceData(int x, int y)
    {
        if (y >= _height) return true;
        int nextX = (x + 1) % _width;
        int nextY = y + ((x + 1) / _width);

        // пропускаем занятое (центр)
        if (_gridData[x, y].filled) return BacktrackPlaceData(nextX, nextY);

        // взвешенная случайная перестановка индексов тайлов
        var order = WeightedIndexShuffle(_locationTiles, _rng);
        foreach (int tileIdx in order)
        {
            if (!_allowStartTileInPool && tileIdx == _startIdx) continue;

            var tile = _locationTiles[tileIdx];
            foreach (int rot in EnumerateRotationsIndices(tile))
            {
                // границы
                var mask = _maskCache[tileIdx, rot];
                if (x == 0 && mask.HasFlag(Openings.West)) continue;
                if (x == _width - 1 && mask.HasFlag(Openings.East)) continue;
                if (y == 0 && mask.HasFlag(Openings.South)) continue;
                if (y == _height - 1 && mask.HasFlag(Openings.North)) continue;

                if (!MatchesNeighborsData(x, y, mask)) continue;

                _gridData[x, y] = new Cell { tile = (short)tileIdx, rot = (byte)rot, filled = true };

                if (BacktrackPlaceData(nextX, nextY)) return true;

                _gridData[x, y].filled = false; // откат
            }
        }
        return false;
    }

    private bool MatchesNeighborsData(int x, int y, Openings maskHere)
    {
        if (x - 1 >= 0 && _gridData[x - 1, y].filled)
        {
            var left = _maskCache[_gridData[x - 1, y].tile, _gridData[x - 1, y].rot];
            bool need = left.HasFlag(Openings.East);
            if (need != maskHere.HasFlag(Openings.West)) return false;
        }
        if (x + 1 < _width && _gridData[x + 1, y].filled)
        {
            var right = _maskCache[_gridData[x + 1, y].tile, _gridData[x + 1, y].rot];
            bool need = right.HasFlag(Openings.West);
            if (need != maskHere.HasFlag(Openings.East)) return false;
        }
        if (y - 1 >= 0 && _gridData[x, y - 1].filled)
        {
            var bottom = _maskCache[_gridData[x, y - 1].tile, _gridData[x, y - 1].rot];
            bool need = bottom.HasFlag(Openings.North);
            if (need != maskHere.HasFlag(Openings.South)) return false;
        }
        if (y + 1 < _height && _gridData[x, y + 1].filled)
        {
            var top = _maskCache[_gridData[x, y + 1].tile, _gridData[x, y + 1].rot];
            bool need = top.HasFlag(Openings.South);
            if (need != maskHere.HasFlag(Openings.North)) return false;
        }
        return true;
    }
    #endregion

    #region Instantiate phase
    private void InstantiateAllImmediate()
    {
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                if (!_gridData[x, y].filled) continue;
                var c = _gridData[x, y];
                var prefab = _locationTiles[c.tile];
                var go = Instantiate(prefab.gameObject, IndexToWorld(x, y), RotationFromQuarterTurns(c.rot), transform);

                // без GetComponent через prefab? норм, но здесь ок:
                go.GetComponent<LocationTile>()?.SetupRuntime(_rng);
            }
    }

    private System.Collections.IEnumerator InstantiateAllOverFrames()
    {
        int spawnedThisFrame = 0;
        for (int y = 0; y < _height; y++)
            for (int x = 0; x < _width; x++)
            {
                if (!_gridData[x, y].filled) continue;
                var c = _gridData[x, y];
                var prefab = _locationTiles[c.tile];
                var go = Instantiate(prefab.gameObject, IndexToWorld(x, y), RotationFromQuarterTurns(c.rot), transform);
                go.GetComponent<LocationTile>()?.SetupRuntime(_rng);

                if (++spawnedThisFrame >= _instantiatesPerFrame)
                {
                    spawnedThisFrame = 0;
                    yield return null; // размазываем работу по кадрам
                }
            }
    }
    #endregion

    #region Helpers
    private IEnumerable<int> EnumerateRotationsIndices(LocationTile tile)
    {
        if (!tile.AllowRotation) { yield return 0; yield break; }
        // перетасовка 0..3
        int[] r = { 0, 1, 2, 3 };
        for (int i = 3; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (r[i], r[j]) = (r[j], r[i]);
        }
        yield return r[0]; yield return r[1]; yield return r[2]; yield return r[3];
    }

    private List<int> WeightedIndexShuffle(List<LocationTile> src, System.Random rng)
    {
        // без аллока множества копий — строим alias-like выбор и тасуем индексы по ключу
        var idx = new List<int>(src.Count);
        for (int i = 0; i < src.Count; i++) idx.Add(i);
        // тасуем с весом через компаратор случайного ключа с учётом веса
        idx.Sort((a, b) =>
        {
            double ka = Math.Pow(rng.NextDouble(), 1.0 / Math.Max(0.0001f, src[a].weight));
            double kb = Math.Pow(rng.NextDouble(), 1.0 / Math.Max(0.0001f, src[b].weight));
            return ka.CompareTo(kb);
        });
        return idx;
    }

    private int CountBits(Openings m)
    {
        int c = 0;
        if (m.HasFlag(Openings.North)) c++;
        if (m.HasFlag(Openings.East)) c++;
        if (m.HasFlag(Openings.South)) c++;
        if (m.HasFlag(Openings.West)) c++;
        return c;
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
            for (int x = 0; x < _width; x++)
                Gizmos.DrawWireCube(IndexToWorld(x, y), new Vector3(_cellSize, 0.05f, _cellSize));
    }
#endif
    #endregion
}
