using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
public enum PlayerStats
{
    HP,
    Fear,
}
public class PlayerStatManager : MonoBehaviour
{
    [SerializeField] private float _hpMax = 100;
    [SerializeField] private float _fearLimit = 100;

    [Header("Ёфекты")]
    [Header("”рон HP")]
    [SerializeField] private float _damageShakePower = 0.3f;
    [SerializeField] private float _damageShakeTime = 0.25f;
    [SerializeField] private float _damageRednessPower = 0.7f;
    [SerializeField] private float _damageRednessTime = 0.3f;

    [Header("”рон ћенталки")]
    [SerializeField] private float _damageFreezePower = 0.8f;
    [SerializeField] private float _damageFreezeTime = 0.8f;


    private float _fearAdd = 0;
    private float _fear = -1;
    public float Fear
    {
        get { return _fear; }
        set
        {
            value = value + _fearAdd;
            if (_fear == value) return;
            if (_fear + 5 < value)
            {
            }
            if (value >= _fearLimit)
            {
                _fear = _fearLimit;
                DeadFear();
            }
            else
            {
                _fear = value ;
            }
            ChangeFear?.Invoke(_fear);
        }
    }

    private float _hp;
    public float HP
    {
        get { return _hp; }
        set
        {
            if (_hp == value) return;
            if (_hp > value)
            {
                if (G.EffectsControllerTimedUI.red.current == 0)
                {
                    G.EffectsControllerTimedUI.AddRedImpulse(_damageRednessPower);
                    G.EffectsControllerTimedUI.Shake(_damageShakePower, _damageShakeTime);
                    ChangeHPOne?.Invoke();
                }
            }

            if (value <= 0)
            {
                _hp = 0;
                Dead();
            }
            else
            {
                _hp = value;
            }
            ChangeHP?.Invoke(_hp);

        }
    }
    public UnityEvent<float> ChangeHP;
    public UnityEvent ChangeHPOne;
    public UnityEvent<float> ChangeFear;
    public UnityEvent ChangeFearOne;

    private void Awake()
    {
        if (G.PlayerStatManager == null)
            G.PlayerStatManager = this;
        else
            Destroy(gameObject);

        HP = _hpMax;
        Fear = 0;
    }
    private void OnDestroy()
    {
        if (G.PlayerStatManager == this)
            G.PlayerStatManager = null;
    }
    public void Damage(int damage,PlayerStats stat = PlayerStats.HP)
    {
        switch (stat)
        {
            case PlayerStats.HP:
                HP -= damage;
                break;
            case PlayerStats.Fear:
                Fear += damage;
                break;
        }
    }
    public void FearDamage(int damage)
    {
        Fear -= damage;
    }
    private void Dead()
    {
        var current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }
    private void DeadFear()
    {
        Damage(1);
        //Dead();
    }
    public void AddMultipliFear()
    {
        G.EffectsControllerTimedUI.SetFreezeTarget(_damageFreezePower);
        _fearAdd++;
        ChangeFearOne?.Invoke();
    }
    public void RemoveMultipliFear()
    {
        G.EffectsControllerTimedUI.SetFreezeTarget(0);
        _fearAdd--;
    }
}

