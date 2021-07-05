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
using UnityEngine.Serialization;
using UtilityScripts;
using Counterattack = Quests.Counterattack;
using DivineIntervention = Quests.DivineIntervention;

public class AudioManager : MonoBehaviour {

    public static AudioManager Instance;
    
    private const string MusicVolume = "musicMasterVolume";
    private const string MasterVolume = "masterVolume";
    private const string ThreatMusicVolume = "threatMusicVolume";
    private const int MaxAudioObjects = 30;
    
    public static float Minimum_Volume_Level = -30f;
    public static float Maximum_Volume_Level = -10f;
    
    
    [Header("Mixers")] 
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private AudioMixer musicMixer;

    [Header("Music Audio Sources")] 
    [SerializeField] private AudioSource worldMusic;
    [SerializeField] private AudioSource mainMenuMusic;
    
    [Header("UI Audio Sources")] 
    [SerializeField] private AudioSource buttonClick;
    [SerializeField] private AudioSource toggleClick;
    [SerializeField] private AudioSource questNotificationSound;
    [SerializeField] private AudioSource positiveNotificationSound;
    [SerializeField] private AudioSource negativeNotificationSound;
    [SerializeField] private AudioSource conversationMenuOpened;
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
    [SerializeField] private AudioClip[] placeStructureAudio;
    [SerializeField] private SoundEffectDictionary soundEffectDictionary;
    
    [Header("Combat Audio")]
    [SerializeField] private AudioClip[] bowAndArrowAudio;
    [SerializeField] private AudioClip[] arrowImpactAudio;
    [SerializeField] private AudioClip[] swordFleshCombatAudio;
    [SerializeField] private AudioClip[] swordObjectCombatAudio;
    [SerializeField] private AudioClip[] bluntWeaponAudio;
    [SerializeField] private AudioClip[] punchAudio;

    private bool isPlayingThreatMusic;
    private int _activeAudioObjects;
    
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
        Messenger.RemoveListener<float>(SettingsSignals.MASTER_VOLUME_CHANGED, SetMasterVolume);
        Messenger.RemoveListener<float>(SettingsSignals.MUSIC_VOLUME_CHANGED, SetMusicVolume);
        Messenger.RemoveListener<RuinarchButton>(UISignals.BUTTON_CLICKED, OnButtonClicked);
        Messenger.RemoveListener<RuinarchToggle>(UISignals.TOGGLE_CLICKED, OnToggleClicked);
        Messenger.RemoveListener<string>(UISignals.STARTED_LOADING_SCENE, OnSceneStartedLoading);
    }
    private void Start() {
        SetMasterVolume(SettingsManager.Instance.settings.masterVolume);
        SetMusicVolume(SettingsManager.Instance.settings.musicVolume);
    }

    #region Initialization
    private void Initialize() {
        Messenger.MarkAsPermanent(SettingsSignals.MASTER_VOLUME_CHANGED);
        Messenger.MarkAsPermanent(SettingsSignals.MUSIC_VOLUME_CHANGED);
        Messenger.MarkAsPermanent(UISignals.BUTTON_CLICKED);
        Messenger.MarkAsPermanent(UISignals.TOGGLE_CLICKED);
        Messenger.MarkAsPermanent(UISignals.STARTED_LOADING_SCENE);
        
        Messenger.AddListener<float>(SettingsSignals.MASTER_VOLUME_CHANGED, SetMasterVolume);
        Messenger.AddListener<float>(SettingsSignals.MUSIC_VOLUME_CHANGED, SetMusicVolume);
        Messenger.AddListener<RuinarchButton>(UISignals.BUTTON_CLICKED, OnButtonClicked);
        Messenger.AddListener<RuinarchToggle>(UISignals.TOGGLE_CLICKED, OnToggleClicked);
        Messenger.AddListener<string>(UISignals.STARTED_LOADING_SCENE, OnSceneStartedLoading);
    }
    public void OnGameLoaded() {
        Messenger.AddListener<Quest>(UISignals.QUEST_SHOWN, OnQuestShown);
        Messenger.AddListener<QuestStep>(PlayerQuestSignals.QUEST_STEP_COMPLETED, OnQuestStepCompleted);
        Messenger.AddListener<QuestStep>(PlayerQuestSignals.QUEST_STEP_FAILED, OnQuestStepFailed);
        Messenger.AddListener(PlayerSignals.START_THREAT_EFFECT, OnStartThreatEffect);
        Messenger.AddListener(PlayerSignals.STOP_THREAT_EFFECT, OnStopThreatEffect);
        Messenger.AddListener<IIntel>(PlayerSignals.PLAYER_OBTAINED_INTEL, OnObtainIntel);
        Messenger.AddListener(UISignals.ON_OPEN_CONVERSATION_MENU, OnOpenConversationMenu);
        Messenger.AddListener<ISelectable>(ControlsSignals.SELECTABLE_LEFT_CLICKED, WorldSelectableLeftClicked);
        Messenger.AddListener<Quest>(PlayerQuestSignals.QUEST_ACTIVATED, OnQuestActivated);
        Messenger.AddListener<Quest>(PlayerQuestSignals.QUEST_DEACTIVATED, OnQuestDeactivated);
        // SetCameraParent(InnerMapCameraMove.Instance);
    }
    public void OnLoadoutSelected() {
        if (QuestManager.Instance.IsQuestActive<DivineIntervention>()) {
            //if game already loaded into a state that has the divine intervention quest, then check if threat music should be playing
            CheckThreatMusic();
        }
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
    public void ResetAndPlayMainMenuMusic() {
        mainMenuMusic.Stop();
        mainMenuMusic.Play();
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
    private void OnStopThreatEffect() {
        CheckThreatMusic();
    }
    private void OnStartThreatEffect() {
        CheckThreatMusic();
    }
    private void OnQuestActivated(Quest quest) {
        if (quest is DivineIntervention) {
            CheckThreatMusic();
        }
    }
    private void OnQuestDeactivated(Quest quest) {
        if (quest is DivineIntervention) {
            CheckThreatMusic();
        }
    }
    private void CheckThreatMusic() {
        if (QuestManager.Instance.IsQuestActive<DivineIntervention>() 
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
        // questNotificationSound.Play();
    }
    private void OnQuestStepCompleted(QuestStep questStep) {
        // positiveNotificationSound.Play();
    }
    private void OnQuestStepFailed(QuestStep questStep) {
        // negativeNotificationSound.Play();
    }
    public void OnErrorSoundPlay() {
        negativeNotificationSound.Play();
    }

    public void OnTextPopUpSoundPlay() {
        positiveNotificationSound.Play();
    }
    private void OnObtainIntel(IIntel intel) {
        particleMagnet.Play();
    }
    private void OnOpenConversationMenu() {
        conversationMenuOpened.Play();
    }
    public void PlayParticleMagnet() {
        particleMagnet.Play();
    }
    #endregion

    #region Camera
    private void OnSceneStartedLoading(string sceneName) {
        if (sceneName == "MainMenu") {
            transform.SetParent(null);
        }
    }
    private void WorldSelectableLeftClicked(ISelectable selectable) {
        worldSelectableClick.Play();
    }
    public void SetCameraParent(BaseCameraMove cameraMove) {
        if (cameraMove == null) {
            transform.SetParent(null);
        } else {
            transform.SetParent(cameraMove.transform);
        }
        transform.localPosition = Vector3.zero;
    }
    #endregion

    #region Audio Objects
    public AudioObject TryCreateAudioObject(AudioClip audioClip, LocationGridTile centerTile, int tileRange, bool loopAudio = true, bool followLimit = false) {
        if (followLimit) {
            if (_activeAudioObjects >= MaxAudioObjects) {
                //if maximum number audio objects has been reached and audio should follow limit, then do not create an audio object.
                //This is a possible fix for this: https://trello.com/c/5rsxPv4a/1867-strange-crash
                return null;
            }
        }
        GameObject go = ObjectPoolManager.Instance.InstantiateObjectFromPool(spellAudioObjectPrefab.name,
            centerTile.centeredWorldLocation, Quaternion.identity, centerTile.parentMap.objectsParent, true);
        AudioObject audioObject = go.GetComponent<AudioObject>();
        audioObject.Initialize(audioClip, tileRange, loopAudio);
        _activeAudioObjects++;
        return audioObject;
    }
    public void OnAudioObjectReset() {
        _activeAudioObjects--;
    }
    #endregion

    #region Poison Explosion
    public void CreatePoisonExplosionAudio(LocationGridTile tile) {
        TryCreateAudioObject(CollectionUtilities.GetRandomElement(poisonExplosionAudio), tile, 1, false, true);
    }
    #endregion
    
    #region Frozen Explosion
    public void CreateFrozenExplosionAudio(LocationGridTile tile) {
        TryCreateAudioObject(CollectionUtilities.GetRandomElement(frozenExplosionAudio), tile, 1, false, true);
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

    #region Base Building
    public void CreatePlaceDemonicStructureSound(LocationGridTile tile) {
        TryCreateAudioObject(CollectionUtilities.GetRandomElement(placeStructureAudio), tile, 3, false, true);
    }
    #endregion

    #region SFX
    public void CreateSFXAt(LocationGridTile p_tile, SOUND_EFFECT p_sfx) {
        TryCreateAudioObject(GetRandomAudioClip(p_sfx), p_tile, 1, false, true);
    }
    private AudioClip GetRandomAudioClip(SOUND_EFFECT p_sfx) {
        if (soundEffectDictionary.ContainsKey(p_sfx)) {
            return CollectionUtilities.GetRandomElement(soundEffectDictionary[p_sfx]);
        }
        throw new Exception($"No SFX found for {p_sfx}");
    }
    #endregion

}
