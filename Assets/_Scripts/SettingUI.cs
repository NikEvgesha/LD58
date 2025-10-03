using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    [SerializeField] private Slider _musicVolume;
    [SerializeField] private Slider _soundVolume;
    [SerializeField] private Slider _sensivity;
    [SerializeField] private GameObject _panel;

    private bool _isOpen;
    public void ToggleOpen()
    {
        _isOpen = !_isOpen;
        _panel.SetActive(_isOpen);
        G.IsPaused = _isOpen;
        
        //if (_isOpen)
        //    PlayerInput.Instance.AOpenWindow?.Invoke(this);
        
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

    private void OnDisable()
    {
        G.SoundManager.Ready.RemoveListener(SetValues);
        //PlayerInput.Instance.APause -= ToggleOpen;
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
