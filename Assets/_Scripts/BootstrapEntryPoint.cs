using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapEntryPoint : MonoBehaviour 
{
    [SerializeField] private GameLoader _gameLoader;
    [SerializeField] private SoundManager _soundManager;
    [SerializeField] private SaveManager _saveManager;
    [SerializeField] private LocalizationManager _localizationManager;
    [SerializeField] private Settings _settings;



    private void Start()
    {
        Instantiate(_gameLoader);
        G.GameLoader.ShowLoadingImage(true);


        Instantiate(_soundManager);
        Instantiate(_saveManager);
        Instantiate(_localizationManager);
        Instantiate(_settings);


        G.GameLoader.LoadNextScene("Evgesha", false);
        G.GameLoader.ShowLoadingImage(false);
    }

}
