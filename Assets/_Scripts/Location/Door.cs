using UnityEngine;

public class Door : ManagedBehaviour
{
    private bool _isOpen;
    private float _time = 4; 
    [SerializeField] private InteractionPanel _interactionPanel;
    public void _OpenDoor()
    {
        if (_isOpen) return;
        _isOpen = true;
    }
    public void _SeeInDoor()
    {
        _interactionPanel.gameObject.SetActive(true);
    }
    public void _DontSeeInDoor()
    {
        _interactionPanel.gameObject.SetActive(false);
    }
    protected override void PausableUpdate()
    {
        if (!_isOpen) return;
        if (_time <= 0) return;
        _time -= Time.deltaTime;
        transform.position = transform.position + Vector3.down * Time.deltaTime;
    }

}
