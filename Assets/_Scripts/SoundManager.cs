using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

public class SoundManager : MonoBehaviour
{
    private const float minVolume = -80f;
    private const float maxVolume = 0f;

    [SerializeField] private AudioMixerGroup _mixer;
    [SerializeField] private AnimationCurve _curve;
    [SerializeField] private AudioSource _music;
    [SerializeField] private AudioSource _uiClick;
    [SerializeField] private AudioClip _lose;
    [SerializeField] private AudioClip _win;
    [SerializeField] private AudioClip _inDungeons;
    [SerializeField] private AudioClip _inShop;



    private static SoundManager _instance;
    public static SoundManager Instance => _instance;

    private float _soundVolume;
    private float _musicVolume;

    // �������� ���������� � AudioMixer
    private readonly string _soundName = "SoundVolume";
    private readonly string _musicName = "MusicVolume";

    public bool IsReady { get; private set; }
    public UnityEvent Ready;

    /// <summary>
    /// ���������� ������-��������� ����� MirraSDK
    /// </summary>
    //public float MasterVolume
    //{
    //    get => MirraSDK.Audio.Volume;
    //    set
    //    {
    //        MirraSDK.Audio.Volume = Mathf.Clamp01(value);
    //        //Debug.Log($"Master volume set to: {MirraSDK.Audio.Volume}");
    //    }
    //}

    /// <summary>
    /// ��������� �������� �������� (SFX)
    /// </summary>
    public float SoundVolume
    {
        get => _soundVolume;
        set
        {
            _soundVolume = Mathf.Clamp01(value);
            float db = Mathf.Lerp(minVolume, maxVolume, _curve.Evaluate(_soundVolume));
            _mixer.audioMixer.SetFloat(_soundName, db);
            // ��������� ���������� ���������
            //MasterVolume = _soundVolume;
            //SaveManager.Instance.SaveSoundVolume(_soundVolume);
            //Debug.Log($"Sound SFX volume set to: {_soundVolume} (db={db})");
        }
    }

    /// <summary>
    /// ��������� ������� ������
    /// </summary>
    public float MusicVolume
    {
        get => _musicVolume;
        set
        {
            _musicVolume = Mathf.Clamp01(value);
            float db = Mathf.Lerp(minVolume, maxVolume, _curve.Evaluate(_musicVolume));
            _mixer.audioMixer.SetFloat(_musicName, db);
            //MasterVolume = _musicVolume;
            //SaveManager.Instance.SaveMusicVolume(_musicVolume);
            //Debug.Log($"Music volume set to: {_musicVolume} (db={db})");
        }
    }

    private void Awake()
    {
        if (G.SoundManager == null)
        {
            G.SoundManager = this;
            DontDestroyOnLoad(this);
        }      
        else
        {
            Destroy(gameObject);
        }
            
    }


    private void Start()
    {
            StartGame();
        G.Game.GameStart.AddListener(()=>ChangeMusic(true));
        G.Game.GameEnd.AddListener((bool win) => ChangeMusic(false));
    }
    private void StartGame()
    {
        // ��������� ����������� ���������
        //float[] volume = SaveManager.Instance.GetVolume();
        MusicVolume = 0.5f; //volume[0];
        SoundVolume = 0.5f; //volume[1];

        IsReady = true;
        Ready?.Invoke();
        PlayMusicLoop();
    }
    private void ChangeMusic(bool inDungeons)
    {
        _music.clip = inDungeons ? _inDungeons:_inShop;
        PlayMusicLoop();
    }
    public void OnPauseAudioChanged(bool paused)
    {
        // ���������� ������ ����� MirraSDK ��� �����
        //MirraSDK.Audio.Pause = paused;
    }

    public void PlayUIClick()
    {
        if (_uiClick != null)
            _uiClick.Play();
    }

    public void PlayMusicLoop()
    {
        if (_music != null)
        {
            _music.loop = true;
            _music.Play();
        }
    }

    public void PlayLose()
    {
        if (_music != null && _lose != null)
        {
            _music.loop = false;
            _music.clip = _lose;
            _music.Play();
        }
    }

    public void PlayWin()
    {
        if (_music != null && _win != null)
        {
            _music.loop = false;
            _music.clip = _win;
            _music.Play();
        }
    }

}