using UnityEngine;
using UnityEngine.UI;

public class ItemNotification : MonoBehaviour
{
    [SerializeField] private Image _icon;
    private Animator _animator;

    public void Init(Sprite icon)
    {
        _animator = GetComponent<Animator>();
        _icon.sprite = icon;
        _animator.SetTrigger("Play");
    }

    private void OnDisable()
    {
        Destroy(gameObject);
    }


}
