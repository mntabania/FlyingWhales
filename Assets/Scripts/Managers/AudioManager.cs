using System;
using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using Quests;
using Quests.Steps;
using Ruinarch.Custom_UI;
using Settings;
using Tutorial;
using UnityEngine;
using UnityEngine.Audio;
using UtilityScripts;
using Counterattack = Quests.Counterattack;
using DivineIntervention = Quests.DivineIntervention;

public class AudioManager : MonoBehaviour {

    public static AudioManager Instance;
    
    private const string MusicVolume = "musicMasterVolume";
    private const string MasterVolume = "masterVolume";
    private const string ThreatMusicVolume = "threatMusicVolume";

    public static float Minimum_Volume_Level = -30f;
    public static float Maximum_Volume_Level = -10f;
    
    
    [Header("Mixers")] 
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioMixer musicMixer;

    [Header("Music Audio Sources")] 
    [SerializeField] private AudioSource worldMusic;
    
    [Header("UI Audio Sources")] 
    [SerializeField] private AudioSource buttonClick;
    [SerializeField] private AudioSource toggleClick;
    [SerializeField] private AudioSource questNotificationSound;
    [SerializeField] private AudioSource positiveNotificationSound;
    [SerializeField] private AudioSource negativeNotificationSound;
    [SerializeField] private AudioSource shareIntel;
    [SerializeField] private AudioSource particleMagnet;
    [SerializeField] private AudioSource worldSelectableClick;
    
    [Header("Snapshots")] 
    [SerializeField] private AudioMixerSnapshot mainMenuSnapShot;
    [SerializeField] private AudioMixerSnapshot loadingSnapShot;
    [SerializeField] private AudioMixerSnapshot worldSnapShot;
    [SerializeField] private AudioMixerSnapshot threatSnapShot;

    [Header("Audio Objects")] 
    [SerializeField] private GameObject spellAudioObjectPrefab;

    [Header("Unique Audio")] 
    [SerializeField] private AudioClip[] poisonExplosionAudio;
    [SerializeField] private AudioClip[] zapAudio;
    [SerializeField] private AudioClip[] frozenExplosionAudio;
    
    [Header("Combat Audio")]
    [SerializeField] private AudioClip[] bowAndArrowAudio;
    [SerializeField] private AudioClip[] arrowImpactAudio;
    [SerializeField] private AudioClip[] swordFleshCombatAudio;
    [SerializeField] private AudioClip[] swordObjectCombatAudio;
    [SerializeField] private AudioClip[] bluntWeaponAudio;
    [SerializeField] private AudioClip[] punchAudio;

    private bool isPlayingThreatMusic;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        } else {
            Destroy(gameObject);
        }
    }
    private void OnDestroy() {
        Messenger.RemoveListener<float>(Signals.MASTER_VOLUME_CHANGED, SetMasterVolume);
        Messenger.RemoveListener<float>(Signals.MUSIC_VOLUME_CHANGED, SetMusicVolume);
        Messenger.RemoveListener<RuinarchButton>(Signals.BUTTON_CLICKED, OnButtonClicked);
        Messenger.RemoveListener<RuinarchToggle>(Signals.TOGGLE_CLICKED, OnToggleClicked);
        Messenger.RemoveListener<string>(Signals.STARTED_LOADING_SCENE, OnSceneStartedLoading);
    }
    private void Start() {
        SetMasterVolume(SettingsManager.Instance.settings.masterVolume);
        SetMusicVolume(SettingsManager.Instance.settings.musicVolume);
    }

    #region Initialization
    private void Initialize() {
        Messenger.MarkAsPermanent(Signals.MASTER_VOLUME_CHANGED);
        Messenger.MarkAsPermanent(Signals.MUSIC_VOLUME_CHANGED);
        Messenger.MarkAsPermanent(Signals.BUTTON_CLICKED);
        Messenger.MarkAsPermanent(Signals.TOGGLE_CLICKED);
        Messenger.MarkAsPermanent(Signals.STARTED_LOADING_SCENE);
        
        Messenger.AddListener<float>(Signals.MASTER_VOLUME_CHANGED, SetMasterVolume);
        Messenger.AddListener<float>(Signals.MUSIC_VOLUME_CHANGED, SetMusicVolume);
        Messenger.AddListener<RuinarchButton>(Signals.BUTTON_CLICKED, OnButtonClicked);
        Messenger.AddListener<RuinarchToggle>(Signals.TOGGLE_CLICKED, OnToggleClicked);
        Messenger.AddListener<string>(Signals.STARTED_LOADING_SCENE, OnSceneStartedLoading);
    }
    public void OnGameLoaded() {
        // Messenger.AddListener<Region>(Signals.LOCATION_MAP_OPENED, OnInnerMapOpened);
        // Messenger.AddListener<Region>(Signals.LOCATION_MAP_CLOSED, OnInnerMapClosed);
        Messenger.AddListener<Quest>(Signals.QUEST_SHOWN, OnQuestShown);
        Messenger.AddListener<QuestStep>(Signals.QUEST_STEP_COMPLETED, OnQuestStepCompleted);
        Messenger.AddListener<QuestStep>(Signals.QUEST_STEP_FAILED, OnQuestStepFailed);
        Messenger.AddListener(Signals.THREAT_MAXED_OUT, OnThreatMaxedOut);
        Messenger.AddListener(Signals.THREAT_RESET, OnThreatReset);
        Messenger.AddListener<IIntel>(Signals.PLAYER_OBTAINED_INTEL, OnObtainIntel);
        Messenger.AddListener(Signals.ON_OPEN_SHARE_INTEL, OnOpenShareIntel);
        Messenger.AddListener<ISelectable>(Signals.SELECTABLE_LEFT_CLICKED, WorldSelectableLeftClicked);
        Messenger.AddListener<Quest>(Signals.QUEST_ACTIVATED, OnQuestActivated);
        Messenger.AddListener<Quest>(Signals.QUEST_DEACTIVATED, OnQuestDeactivated);
        // SetCameraParent(InnerMapCameraMove.Instance);
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
    private void PlayThreatMusic() {
        if (isPlayingThreatMusic) { return; } //already playing threat music
        isPlayingThreatMusic = true;
        threatSnapShot.TransitionTo(0.5f);
    }
    private void StopThreatMusic() {
        if (isPlayingThreatMusic == false) { return; } //already not playing threat music
        isPlayingThreatMusic = false;
        worldSnapShot.TransitionTo(0.5f);
    }
    private void OnThreatReset() {
        CheckThreatMusic();
    }
    private void OnThreatMaxedOut() {
        CheckThreatMusic();
    }
    private void OnQuestActivated(Quest quest) {
        if (quest is Quests.Counterattack || quest is DivineIntervention) {
            CheckThreatMusic();
        }
    }
    private void OnQuestDeactivated(Quest quest) {
        if (quest is Quests.Counterattack || quest is DivineIntervention) {
            CheckThreatMusic();
        }
    }
    private void CheckThreatMusic() {
        if (QuestManager.Instance.IsQuestActive<Quests.Counterattack>() 
            || QuestManager.Instance.IsQuestActive<DivineIntervention>() 
            || PlayerManager.Instance.player.threatComponent.threat >= ThreatComponent.MAX_THREAT) {
            //play threat music if threat is at max or counterattack quest is active or divine intervention quest is active
            PlayThreatMusic();
        } else {
            StopThreatMusic();
        }
    }
    #endregion

    #region Volume
    public void SetMasterVolume(float volume) {
        if (Mathf.Approximately(volume, Minimum_Volume_Level)) {
            volume = -80f; //mute the master volume.
        }
        masterMixer.SetFloat(MasterVolume, volume);
    }
    public void SetMusicVolume(float volume) {
        if (Mathf.Approximately(volume, Minimum_Volume_Level)) {
            volume = -80f; //mute the music mixer.
        }
        masterMixer.SetFloat(MusicVolume, volume);
    }
    #endregion

    #region UI
    private void OnButtonClicked(RuinarchButton button) {
        buttonClick.Play();
    }
    private void OnToggleClicked(RuinarchToggle toggle) {
        toggleClick.Play();
    }
    private void OnQuestShown(Quest quest) {
        questNotificationSound.Play();
    }
    private void OnQuestStepCompleted(QuestStep questStep) {
        // positiveNotificationSound.Play();
    }
    private void OnQuestStepFailed(QuestStep questStep) {
        negativeNotificationSound.Play();
    }
    private void OnObtainIntel(IIntel intel) {
        particleMagnet.Play();
    }
    private void OnOpenShareIntel() {
        shareIntel.Play();
    }
    public void PlayParticleMagnet() {
        particleMagnet.Play();
    }
    #endregion

    #region Camera
    private void OnInnerMapOpened(Region region) {
        SetCameraParent(InnerMapCameraMove.Instance);
    }
    private void OnInnerMapClosed(Region region) {
        SetCameraParent(WorldMapCameraMove.Instance);
    }
    private void OnSceneStartedLoading(string sceneName) {
        if (sceneName == "MainMenu") {
            transform.SetParent(null);
        }
    }
    private void WorldSelectableLeftClicked(ISelectable selectable) {
        worldSelectableClick.Play();
    }
    public void SetCameraParent(BaseCameraMove cameraMove) {
        transform.SetParent(cameraMove.transform);
        transform.localPosition = Vector3.zero;
    }
    #endregion

    #region Spells
    public AudioObject CreateAudioObject(AudioClip audioClip, LocationGridTile centerTile, int tileRange, bool loopAudio = true) {
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(spellAudioObjectPrefab.name,
            centerTile.centeredWorldLocation, Quaternion.identity, centerTile.parentMap.objectsParent, true);
        AudioObject audioObject = go.GetComponent<AudioObject>();
        audioObject.Initialize(audioClip, tileRange, loopAudio);
        return audioObject;
    }
    #endregion

    #region Poison Explosion
    public void CreatePoisonExplosionAudio(LocationGridTile tile) {
        CreateAudioObject(CollectionUtilities.GetRandomElement(poisonExplosionAudio), tile, 1, false);
    }
    #endregion
    
    #region Frozen Explosion
    public void CreateFrozenExplosionAudio(LocationGridTile tile) {
        CreateAudioObject(CollectionUtilities.GetRandomElement(frozenExplosionAudio), tile, 1, false);
    }
    #endregion

    #region Zap
    public AudioClip GetRandomZapAudio() {
        return CollectionUtilities.GetRandomElement(zapAudio);
    }
    #endregion

    #region Combat
    public AudioClip GetRandomBowAndArrowAudio() {
        return CollectionUtilities.GetRandomElement(bowAndArrowAudio);
    }
    public AudioClip GetRandomArrowImpactAudio() {
        return CollectionUtilities.GetRandomElement(arrowImpactAudio);
    }
    public AudioClip GetRandomSwordAgainstFleshAudio() {
        return CollectionUtilities.GetRandomElement(swordFleshCombatAudio);
    }
    public AudioClip GetRandomSwordAgainstObjectAudio() {
        return CollectionUtilities.GetRandomElement(swordObjectCombatAudio);
    }
    public AudioClip GetRandomPunchAudio() {
        return CollectionUtilities.GetRandomElement(punchAudio);
    }
    public AudioClip GetRandomBluntWeaponAudio() {
        return CollectionUtilities.GetRandomElement(bluntWeaponAudio);
    }
    #endregion

}
