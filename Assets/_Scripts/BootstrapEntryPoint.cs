using UnityEngine;
using UnityEngine.InputSystem;

public class BootstrapEntryPoint : MonoBehaviour 
{
    [SerializeField] private GameLoader _gameLoader;
    [SerializeField] private SoundManager _soundManager;
    [SerializeField] private SaveManager _saveManager;
    [SerializeField] private LocalizationManager _localizationManager;
    [SerializeField] private Settings _settings;
    [SerializeField] private CustomPlayerInput _input;



    private void Start()
    {
        Instantiate(_gameLoader);
        G.GameLoader.ShowLoadingImage(true);


        Instantiate(_soundManager);
        Instantiate(_saveManager);
        Instantiate(_localizationManager);
        Instantiate(_settings);
        Instantiate(_input);


        G.GameLoader.LoadNextScene("Evgesha", false);
    }

}
