using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingUI : ManagedBehaviour
{
    [SerializeField] private Slider _musicVolume;
    [SerializeField] private Slider _soundVolume;
    [SerializeField] private Slider _sensivity;
    [SerializeField] private GameObject _panel;
    [SerializeField] private InputAction _menuAction;

    private bool _isOpen;

    private void OnEnable()
    {
        if (_menuAction != null) _menuAction.Enable();
    }

    private void OnDisable()
    {
        G.SoundManager.Ready.RemoveListener(SetValues);
        if (_menuAction != null) _menuAction.Disable();
        //PlayerInput.Instance.APause -= ToggleOpen;
    }


    private void Awake()
    {
        EnsureDefaultBindingsIfEmpty();
    }


    private void EnsureDefaultBindingsIfEmpty()
    {
        if (_menuAction == null || _menuAction.bindings.Count == 0)
        {
            _menuAction = new InputAction("Inventory", InputActionType.Button);
            _menuAction.AddBinding("<Keyboard>/escape");
        }
    }


    private void Start()
    {
        //PlayerInput.Instance.APause += ToggleOpen;
        if (G.SoundManager.IsReady)
        {
            SetValues();
        } else
        {
            G.SoundManager.Ready.AddListener(SetValues);
        }

        if (G.Settings.IsReady)
        {
            SetSensivity();
        }
        else
        {
            G.Settings.Ready += SetSensivity;
        }

    }


    protected override void PausableUpdate()
    {
        if (_menuAction.triggered)
        {
            ToggleOpen();
        }
    }

    public void ToggleOpen()
    {
        _isOpen = !_isOpen;
        _panel.SetActive(_isOpen);
        G.Control.CursorActive = _isOpen;
        //G.IsPaused = _isOpen;

        //if (_isOpen)
        //    PlayerInput.Instance.AOpenWindow?.Invoke(this);

    }

    private void SetValues()
    {
        _musicVolume.value = G.SoundManager.MusicVolume;
        _soundVolume.value = G.SoundManager.SoundVolume;
    }

    private void SetSensivity()
    {
        _sensivity.value = G.Settings.Sensivity;
    }


    public void OnMusicVolumeChanged(float volume)
    {
        G.Settings.MusicVolume(volume);
    }
    
    public void OnSounsVolumeChanged(float volume)
    {
        G.Settings.SoundVolume(volume);
    }
}
