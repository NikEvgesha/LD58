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

    [Header("Плавный спавн готового плана")]
    [SerializeField, Min(1)] private int _spawnPerFrame = 64;

    private System.Random _rng;

    // ==== ПЛАН (без GameObject) ====
    private struct PlanCell
    {
        public LocationTile tile;
        public int rot;           // 0..3
        public Openings mask;     // кэш маски для rot
        public bool hasValue;
    }
    private PlanCell[,] _plan;    // решение без объектов

    // ==== Инстансированный грид ====
    private GameObject[,] _spawned;

    // Кэш поворотов на тайл, перегенерируется на каждую генерацию (для разнообразия)
    private Dictionary<LocationTile, int[]> _rotCache = new();

    public void Init()
    {
        if (_generateOnStart) Generate();
        G.Game.GameEnd.AddListener(Generate);
    }

    [ContextMenu("Generate")]
    public void Generate(bool _ = false)
    {
        _seed = 0;
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

        // Сид
        if (_seed == 0)
            _seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        _rng = new System.Random(_seed);
        if (_log) Debug.Log($"Seed: {_seed}");

        // Чистим детей (видимые объекты)
        if (_clearChildrenOnGenerate)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
                Destroy(transform.GetChild(i).gameObject);
        }

        _plan = new PlanCell[_width, _height];
        _spawned = new GameObject[_width, _height];
        _rotCache.Clear();

        // Подготовка: кэш ротаций
        PrecomputeRotations();

        // 1) Ставим фиксированный старт в центр (только в плане, без Instantiate)
        var (cx, cy) = (_width / 2, _height / 2);
        if (!PlanPlaceStart(cx, cy))
        {
            Debug.LogWarning("Не удалось подобрать поворот для стартового тайла (нужна ровно одна открытая сторона).");
            return;
        }

        // 2) Бэктрекинг по плану (без объектов)
        var tilesInOrder = WeightedOrder(_locationTiles); // разовая взвешенная перестановка
        bool ok = BacktrackPlacePlan(NextCoord(0, 0), tilesInOrder);

        if (!ok)
        {
            Debug.LogWarning("Не удалось собрать карту, попробуй другой сид.");
            return;
        }

        if (_log) Debug.Log("План готов, начинаю спавн по кадрам…");

        // 3) Спавним по плану — батчами за несколько кадров
        StopAllCoroutines();
        StartCoroutine(SpawnPlanCoroutine());
    }

    // -------------------------------------------------
    //                 ПОДГОТОВКА ДАННЫХ
    // -------------------------------------------------
    private void PrecomputeRotations()
    {
        // Для каждого тайла — список разрешённых четвертных поворотов, случайно перемешанный
        foreach (var t in _locationTiles)
        {
            if (!_rotCache.ContainsKey(t))
                _rotCache[t] = BuildShuffledRots(t);
        }
        if (!_rotCache.ContainsKey(_startTile))
            _rotCache[_startTile] = BuildShuffledRots(_startTile);
    }

    private int[] BuildShuffledRots(LocationTile t)
    {
        if (!t.AllowRotation)
            return new[] { 0 };

        int[] rots = { 0, 1, 2, 3 };
        // Фишер-Йетс
        for (int i = rots.Length - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (rots[i], rots[j]) = (rots[j], rots[i]);
        }
        return rots;
    }

    // -------------------------------------------------
    //                   ПЛАНИРОВАНИЕ
    // -------------------------------------------------
    private bool PlanPlaceStart(int x, int y)
    {
        var rots = _rotCache[_startTile];
        for (int i = 0; i < rots.Length; i++)
        {
            int rot = rots[i];
            var mask = _startTile.MaskWithRotation(rot);
            if (CountBits(mask) != 1) continue;

            _plan[x, y] = new PlanCell { tile = _startTile, rot = rot, mask = mask, hasValue = true };
            return true;
        }
        return false;
    }

    private (int x, int y) NextCoord(int x, int y)
    {
        int nextX = (x + 1) % _width;
        int nextY = y + ((x + 1) / _width);
        return (nextX, nextY);
    }

    private bool BacktrackPlacePlan((int x, int y) coord, List<LocationTile> tilesInOrder)
    {
        int x = coord.x;
        int y = coord.y;

        // Конец? — план готов
        if (y >= _height) return true;

        // Пропускаем уже заполненные (например, центр)
        if (_plan[x, y].hasValue)
            return BacktrackPlacePlan(NextCoord(x, y), tilesInOrder);

        // Ограничения по краям в виде маски запретов
        // (если на границе — соответствующая сторона обязана быть закрыта)
        // Мы просто отбрасываем варианты, где маска «смотрит наружу».
        foreach (var tile in tilesInOrder)
        {
            var rots = _rotCache[tile];
            for (int i = 0; i < rots.Length; i++)
            {
                int rot = rots[i];
                var mask = tile.MaskWithRotation(rot);

                // Границы поля
                if (x == 0 && Has(mask, Openings.West)) continue;
                if (x == _width - 1 && Has(mask, Openings.East)) continue;
                if (y == 0 && Has(mask, Openings.South)) continue;
                if (y == _height - 1 && Has(mask, Openings.North)) continue;

                // Согласование с уже поставленными соседями
                if (!MatchesNeighborsPlan(x, y, mask)) continue;

                // Поставили в план
                _plan[x, y] = new PlanCell { tile = tile, rot = rot, mask = mask, hasValue = true };

                if (BacktrackPlacePlan(NextCoord(x, y), tilesInOrder))
                    return true;

                // Откат
                _plan[x, y].hasValue = false;
            }
        }

        return false;
    }

    private bool MatchesNeighborsPlan(int x, int y, Openings here)
    {
        // LEFT
        if (x - 1 >= 0 && _plan[x - 1, y].hasValue)
        {
            var leftMask = _plan[x - 1, y].mask;
            bool needConnect = Has(leftMask, Openings.East);
            if (needConnect != Has(here, Openings.West)) return false;
        }
        // RIGHT
        if (x + 1 < _width && _plan[x + 1, y].hasValue)
        {
            var rightMask = _plan[x + 1, y].mask;
            bool needConnect = Has(rightMask, Openings.West);
            if (needConnect != Has(here, Openings.East)) return false;
        }
        // DOWN
        if (y - 1 >= 0 && _plan[x, y - 1].hasValue)
        {
            var bottomMask = _plan[x, y - 1].mask;
            bool needConnect = Has(bottomMask, Openings.North);
            if (needConnect != Has(here, Openings.South)) return false;
        }
        // UP
        if (y + 1 < _height && _plan[x, y + 1].hasValue)
        {
            var topMask = _plan[x, y + 1].mask;
            bool needConnect = Has(topMask, Openings.South);
            if (needConnect != Has(here, Openings.North)) return false;
        }
        return true;
    }

    // -------------------------------------------------
    //                 ПОСТРОЕНИЕ СЦЕНЫ
    // -------------------------------------------------
    private System.Collections.IEnumerator SpawnPlanCoroutine()
    {
        int spawnedThisFrame = 0;

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                var cell = _plan[x, y];
                if (!cell.hasValue) continue;

                // Инстанцируем
                var go = Instantiate(cell.tile.gameObject, IndexToWorld(x, y), RotationFromQuarterTurns(cell.rot), transform);
                _spawned[x, y] = go;

                // Инициализация тайла (у вас была)
                var lt = go.GetComponent<LocationTile>();
                lt?.SetupRuntime(_rng);

                // Плавный спавн партиями
                if (++spawnedThisFrame >= _spawnPerFrame)
                {
                    spawnedThisFrame = 0;
                    yield return null; // продолжим на следующий кадр
                }
            }
        }

        if (_log) Debug.Log("Готово!");
    }

    // -------------------------------------------------
    //                 УТИЛИТЫ/МИКРООПТИМИЗАЦИИ
    // -------------------------------------------------
    private static bool Has(Openings m, Openings f) => (m & f) != 0;

    private static int CountBits(Openings m)
    {
        int c = 0;
        if ((m & Openings.North) != 0) c++;
        if ((m & Openings.East) != 0) c++;
        if ((m & Openings.South) != 0) c++;
        if ((m & Openings.West) != 0) c++;
        return c;
    }

    private List<LocationTile> WeightedOrder(List<LocationTile> src)
    {
        // Без дублирования и лишних аллокаций:
        // Считаем ключ: key = random^(1/weight), сортируем по убыванию
        var tmp = new List<(LocationTile tile, double key)>(src.Count);
        for (int i = 0; i < src.Count; i++)
        {
            var t = src[i];
            double w = Math.Max(1e-4, t.weight); // защита от 0
            double u = _rng.NextDouble();
            double key = Math.Pow(u, 1.0 / w);
            tmp.Add((t, key));
        }
        tmp.Sort((a, b) => b.key.CompareTo(a.key));
        var res = new List<LocationTile>(src.Count);
        for (int i = 0; i < tmp.Count; i++) res.Add(tmp[i].tile);
        return res;
    }

    private Quaternion RotationFromQuarterTurns(int q) => Quaternion.Euler(0f, 90f * ((q % 4 + 4) % 4), 0f);

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
