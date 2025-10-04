using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SideSettings
{
    public GameObject WallOpen;
    public GameObject WallClose;
    public Renderer WallMaterial;
}

/// <summary>
/// Опция ловушки: на тайле может быть много ловушек как объектов,
/// но активируем ровно одну из списка.
/// </summary>
[Serializable]
public class TrapOption
{
    [Tooltip("Чисто подпись для удобства в инспекторе")]
    public string name;

    [Tooltip("Ссылка на компонент ловушки на этом тайле (в дочерних объектах)")]
    public DungeonTrap trapComponent;
}

/// <summary>
/// Опция награды: тип награды и список её экземпляров на этом тайле.
/// Активируем первые ActiveCount экземпляров из списка (или случайные, если ShuffleBeforeActivate).
/// </summary>
[Serializable]
public class RewardOption
{
    [Tooltip("Подпись для удобства")]
    public string name;

    [Tooltip("Список компонентов наград этого типа, размещённых на тайле (в детях).")]
    public List<DungeonTreasures> instances = new List<DungeonTreasures>();

    [Tooltip("Включать ли этот тип награды на тайле")]
    public bool enabled = false;

    [Tooltip("Сколько экземпляров этого типа активировать (0 = ничего)")]
    [Min(0)] public int activeCount = 1;

    [Tooltip("Перемешивать порядок экземпляров перед активацией (случайные N)")]
    public bool shuffleBeforeActivate = false;
}

public class LocationTile : MonoBehaviour
{
    [Header("Входы/выходы (для базовой ориентации префаба)")]
    [SerializeField] private bool _north;
    [SerializeField] private bool _east;
    [SerializeField] private bool _south;
    [SerializeField] private bool _west;

    [Tooltip("Можно ли поворачивать префаб на 90° шаг?")]
    [SerializeField] private bool _allowRotation = true;

    [Tooltip("Настраиваемый вес при выборе (больше = чаще попадается)")]
    [Range(0f, 10f)] public float weight = 1f;

    [Header("Тест отрисовки стен")]
    [SerializeField] private List<SideSettings> _sides = new List<SideSettings>();
    [SerializeField] private Material _openMat;
    [SerializeField] private Material _closeMat;
    [SerializeField] private bool _test = true;

    [Header("Ловушки (активируем ровно одну)")]
    [Tooltip("Список ловушек, из которых будет выбрана одна активная.")]
    [SerializeField] private List<TrapOption> _trapOptions = new List<TrapOption>();

    [Tooltip("Если true — выбираем случайную ловушку из списка; если false — берём по индексу ниже.")]
    [SerializeField] private bool _chooseTrapRandomly = true;

    [Tooltip("Индекс ловушки в списке, если случайный выбор выключен.")]
    [SerializeField] private int _trapIndex = 0;

    [Header("Награды")]
    [Tooltip("Типы наград на этом тайле и их экземпляры. Для каждого типа можно задать количество активируемых объектов.")]
    [SerializeField] private List<RewardOption> _rewardOptions = new List<RewardOption>();

    public bool AllowRotation => _allowRotation;

    /// <summary> Битовая маска открытых сторон в базовой ориентации. </summary>
    public Openings BaseMask =>
        (_north ? Openings.North : 0) |
        (_east ? Openings.East : 0) |
        (_south ? Openings.South : 0) |
        (_west ? Openings.West : 0);

    /// <summary> Возвращает маску с учетом поворота на quarterTurnsCW*90°. </summary>
    public Openings MaskWithRotation(int quarterTurnsCW)
    {
        return DirectionUtils.Rotate(BaseMask, quarterTurnsCW);
    }

    /// <summary>
    /// Метод вызывается генератором сразу после инстанса префаба.
    /// Включает одну выбранную ловушку и активирует нужные награды.
    /// </summary>
    public void SetupRuntime(System.Random rng)
    {
        ActivateExactlyOneTrap(rng);
        ActivateRewards(rng);
    }

    private void ActivateExactlyOneTrap(System.Random rng)
    {
        if (_trapOptions == null || _trapOptions.Count == 0) return;

        int index = Mathf.Clamp(_trapIndex, 0, _trapOptions.Count - 1);
        if (_chooseTrapRandomly)
        {
            index = rng != null ? rng.Next(_trapOptions.Count) : UnityEngine.Random.Range(0, _trapOptions.Count);
        }

        for (int i = 0; i < _trapOptions.Count; i++)
        {
            var opt = _trapOptions[i];
            if (opt?.trapComponent == null) continue;

            bool shouldActivate = (i == index);
            // Если у ловушки есть явный Enable/Disable — можно использовать его.
            // Здесь активируем только выбранную. Остальные гарантированно выключим.
            opt.trapComponent.gameObject.SetActive(shouldActivate);
            if (shouldActivate)
            {
                try { opt.trapComponent.Activate(); }
                catch (Exception e) { Debug.LogWarning($"Trap Activate exception on {opt.name}: {e.Message}", opt.trapComponent); }
            }
        }
    }

    private void ActivateRewards(System.Random rng)
    {
        if (_rewardOptions == null) return;

        foreach (var ro in _rewardOptions)
        {
            if (ro == null || ro.instances == null) continue;

            // сначала выключим всё
            foreach (var inst in ro.instances)
            {
                if (inst == null) continue;
                inst.gameObject.SetActive(false);
            }

            if (!ro.enabled || ro.activeCount <= 0) continue;
            if (ro.instances.Count == 0) continue;

            // подготовим порядок
            var list = new List<DungeonTreasures>(ro.instances);
            if (ro.shuffleBeforeActivate)
            {
                // Fisher–Yates
                for (int i = list.Count - 1; i > 0; i--)
                {
                    int j = (rng != null) ? rng.Next(i + 1) : UnityEngine.Random.Range(0, i + 1);
                    (list[i], list[j]) = (list[j], list[i]);
                }
            }

            int toActivate = Mathf.Clamp(ro.activeCount, 0, list.Count);
            for (int i = 0; i < toActivate; i++)
            {
                var inst = list[i];
                if (inst == null) continue;
                inst.gameObject.SetActive(true);
                try { inst.Activate(); }
                catch (Exception e) { Debug.LogWarning($"Treasure Activate exception on {ro.name}: {e.Message}", inst); }
            }
        }
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (!_test) return;

        List<bool> sidesUse = new List<bool>
        {
            _north,
            _east,
            _south,
            _west
        };
        if (_sides.Count > 0)
        {
            int sideIndex = 0;
            foreach (SideSettings side in _sides)
            {
                if (side.WallClose != null) side.WallClose.SetActive(!sidesUse[sideIndex]);
                if (side.WallOpen  != null) side.WallOpen .SetActive(sidesUse[sideIndex]);
                if (side.WallMaterial != null)
                    side.WallMaterial.material = sidesUse[sideIndex] ? _openMat : _closeMat;
                sideIndex++;
                if (sideIndex >= sidesUse.Count) break;
            }
        }
    }
#endif
}
