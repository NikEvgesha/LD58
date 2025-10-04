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
    private float _fear = -1;
    public float Fear
    {
        get { return _fear; }
        set
        {
            if (_fear == value) return;
            if (value >= 100)
            {
                _fear = 100;
                DeadFear();
            }
            else
            {
                _fear = value;
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
    public UnityEvent<float> ChangeFear;

    private void Awake()
    {
        if (G.PlayerStatManager == null)
            G.PlayerStatManager = this;
        else
            Destroy(gameObject);

        HP = _hpMax;
        Fear = 0;
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
}

