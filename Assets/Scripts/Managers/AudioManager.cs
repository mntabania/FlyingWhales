using System.Collections;
using System.Collections.Generic;
using Settings;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {

    public static AudioManager Instance;
    
    private const string MusicVolume = "musicMasterVolume";
    private const string MasterVolume = "masterVolume";
    
    [Header("Mixers")] 
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioMixer musicMixer;

    [Header("Audio Sources")] 
    [SerializeField] private AudioSource worldMusic;
    
    [Header("Snapshots")] 
    [SerializeField] private AudioMixerSnapshot mainMenuSnapShot;
    [SerializeField] private AudioMixerSnapshot loadingSnapShot;
    [SerializeField] private AudioMixerSnapshot worldSnapShot;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        } else {
            Destroy(this.gameObject);
        }
    }

    private void Start() {
        Initialize();
    }
    
    #region Initialization
    private void Initialize() {
        Messenger.MarkAsPermanent(Signals.MASTER_VOLUME_CHANGED);
        Messenger.MarkAsPermanent(Signals.MUSIC_VOLUME_CHANGED);
        Messenger.AddListener<float>(Signals.MASTER_VOLUME_CHANGED, SetMasterVolume);
        Messenger.AddListener<float>(Signals.MUSIC_VOLUME_CHANGED, SetMusicVolume);
        
        SetMasterVolume(SettingsManager.Instance.settings.masterVolume);
        SetMusicVolume(SettingsManager.Instance.settings.musicVolume);
    }
    #endregion
    

    #region Music
    public void TransitionToLoading() {
        loadingSnapShot.TransitionTo(2f);
    }
    public void TransitionToWorld() {
        worldSnapShot.TransitionTo(2f);
        ResetAndPlayWorldMusic();
    }
    public void TransitionToMainMenu() {
        mainMenuSnapShot.TransitionTo(1f);
    }
    private void ResetAndPlayWorldMusic() {
        worldMusic.Stop();
        worldMusic.Play();
    }
    #endregion

    #region Volume
    private void SetMasterVolume(float volume) {
        masterMixer.SetFloat(MasterVolume, volume);
    }
    private void SetMusicVolume(float volume) {
        masterMixer.SetFloat(MusicVolume, volume);
    }
    #endregion
    

   

}
