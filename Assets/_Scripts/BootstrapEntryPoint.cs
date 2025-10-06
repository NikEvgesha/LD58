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
    [SerializeField] private GameManager _gameManager;
    [SerializeField] private ControlManager _controlManager;
    [SerializeField] private CurrencyManager _cuurencyManager;



    [SerializeField] private GameScene _gameScene;




    private void Start()
    {
        Instantiate(_gameLoader);
        G.GameLoader.ShowLoadingImage(true);


        Instantiate(_soundManager);
        Instantiate(_saveManager);
        Instantiate(_localizationManager);
        Instantiate(_settings);
        Instantiate(_input);
        Instantiate(_gameManager);
        Instantiate(_controlManager);
        Instantiate(_cuurencyManager);


        G.GameLoader.LoadNextScene(_gameScene.ToString(), false);
    }

}
