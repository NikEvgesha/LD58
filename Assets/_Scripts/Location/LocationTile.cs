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

    [Header("Для теста")]
    [SerializeField] private List<SideSettings> _sides = new List<SideSettings>();
    [SerializeField] private Material _openMat;
    [SerializeField] private Material _closeMat;
    [SerializeField] private bool _test = true;

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
                side.WallClose.SetActive(!sidesUse[sideIndex]);
                side.WallOpen.SetActive(sidesUse[sideIndex]);
                side.WallMaterial.material = sidesUse[sideIndex] ? _openMat: _closeMat;
                sideIndex++;
            }
        }
    }
#endif
}
