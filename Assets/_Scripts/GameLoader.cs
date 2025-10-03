using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLoader : MonoBehaviour
{

    [SerializeField] private GameObject _loadingImage;
    [SerializeField] private LoadingProgressBarUI _progressBar;
    private string _currentSceneName;
    private AsyncOperation _asyncOperation;

    private bool _startLoadingFinished = false;
    public Action OnSceneLoaded;


    private void Awake()
    {
        if (G.GameLoader == null)
        {
            G.GameLoader = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public void ShowLoadingImage(bool visible)
    {
        _loadingImage.SetActive(visible);
    }

    public void LoadNextScene(string SceneName, bool asyncMode)
    {
        _currentSceneName = SceneName;

        if (asyncMode)
        {
            _loadingImage.SetActive(true);
            StartCoroutine("SceneLoad", _currentSceneName);
        } else
        {
            SceneManager.LoadScene(_currentSceneName);
        }

        
    }


    private IEnumerator SceneLoad(string sceneName)
    {
        float loadingProgress;
        _asyncOperation = SceneManager.LoadSceneAsync(sceneName);
        while (_asyncOperation.progress < 0.95f)
        {
            loadingProgress = Mathf.Clamp01(_asyncOperation.progress / 0.95f);
            _progressBar.Progress(_asyncOperation.progress);
            yield return true;
        }

        _loadingImage.SetActive(false);
        OnSceneLoaded?.Invoke();
    }


}
