using System;
using UnityEngine;
using UnityEngine.Events;

public class Settings : MonoBehaviour
{
    [HideInInspector]
    public UnityEvent<float> ChangeMouseSensitivity;
    [HideInInspector]
    public UnityEvent<float> ChangeVolume;

    public bool IsReady;
    public Action Ready;

    public float Sensivity;


    private void Awake()
    {
        if (G.Settings == null)
        {
            G.Settings = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnGameStart()
    {
        //Sensitivity(SaveManager.Instance.LoadSensivity());
        IsReady = true;
        Ready?.Invoke();
    }


    public void Sensitivity(float sens)
    {
        Sensivity = sens;
        ChangeMouseSensitivity?.Invoke(sens);
        //SaveManager.Instance.SaveSensivity(sens);
    }

    public void SoundVolume(float volume)
    {
        G.SoundManager.SoundVolume = volume;
    }
    public void MusicVolume(float volume)
    {
        G.SoundManager.MusicVolume = volume;
    }

}
