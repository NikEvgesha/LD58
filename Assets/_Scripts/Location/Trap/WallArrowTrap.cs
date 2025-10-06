using System.Collections;
using UnityEngine;

/// <summary>
/// Настенная ловушка-стреломёт. Стреляет по команде (с плиты).
/// </summary>
public class WallArrowTrap : DungeonTrap
{
    [Header("References")]
    [Tooltip("Точки выстрела; направление выстрела берётся по forward каждого Transform")]
    [SerializeField] private Transform[] _muzzles;
    [Tooltip("Префаб стрелы (должен иметь ArrowProjectile)")]
    [SerializeField] private ArrowProjectile _projectilePrefab;
    [Tooltip("Куда складывать инстансы стрел (опционально)")]
    [SerializeField] private Transform _projectilesParent;

    [Header("Fire Params")]
    [SerializeField, Min(0f)] private float _muzzleVelocity = 25f;
    [SerializeField, Min(0f)] private float _spreadDegrees = 0f;
    [SerializeField, Min(1)] private int _arrowsPerBurst = 1;
    [SerializeField, Min(0f)] private float _shotInterval = 0.06f;   // между стрелами в серии
    [SerializeField, Min(0f)] private float _cooldown = 0.6f;        // между сериями
    [SerializeField, Min(0)] private int _maxBursts = 0;             // 0 = бесконечно

    [Header("FX (optional)")]
    [SerializeField] private AudioSource _audio;
    [SerializeField] private AudioClip _sfxShoot;
    [SerializeField] private ParticleSystem[] _muzzleFX;

    // runtime
    private bool _armed;
    private bool _busy;
    private int _burstsFired;
    private float _lastBurstTime = -999f;

    private AudioSource _audioSource;

    public override void Activate()
    {
        _armed = true;
        _busy = false;
        _burstsFired = 0;
        _lastBurstTime = -999f;
        _audioSource = GetComponent<AudioSource>();
    }

    /// <summary> Вызвать плитой для выстрела. </summary>
    public void Fire()
    {
        if (!_armed) return;
        if (_busy) return;
        if (Time.time - _lastBurstTime < _cooldown) return;
        if (_maxBursts > 0 && _burstsFired >= _maxBursts) return;

        StartCoroutine(FireBurst());
    }

    private IEnumerator FireBurst()
    {
        _busy = true;
        _lastBurstTime = Time.time;
        _burstsFired++;

        for (int n = 0; n < _arrowsPerBurst; n++)
        {
            foreach (var m in _muzzles)
            {
                if (!m || !_projectilePrefab) continue;

                // разброс
                Quaternion spreadRot = Quaternion.identity;
                if (_spreadDegrees > 0f)
                {
                    spreadRot = Quaternion.Euler(
                        Random.Range(-_spreadDegrees, _spreadDegrees),
                        Random.Range(-_spreadDegrees, _spreadDegrees),
                        0f
                    );
                }

                var proj = Instantiate(_projectilePrefab, m.position, m.rotation * spreadRot, _projectilesParent);
                proj.Launch(m.forward, _muzzleVelocity);

                if (_audio && _sfxShoot) _audio.PlayOneShot(_sfxShoot);
                if (_muzzleFX != null)
                {
                    foreach (var fx in _muzzleFX) if (fx) fx.Play();
                }
            }

            if (_shotInterval > 0f && n < _arrowsPerBurst - 1)
                yield return new WaitForSeconds(_shotInterval);
        }

        _audioSource.Play();
        _busy = false;
    }
}
