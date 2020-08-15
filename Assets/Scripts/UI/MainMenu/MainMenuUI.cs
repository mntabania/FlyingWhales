using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Managers;
using Settings;
using TMPro;

public class MainMenuUI : MonoBehaviour {

    public static MainMenuUI Instance = null;

    [SerializeField] private EasyTween buttonsTween;
    [SerializeField] private EasyTween titleTween;

    [SerializeField] private EasyTween glowTween;
    [SerializeField] private EasyTween glow2Tween;

    [SerializeField] private Image bg;

    [Header("Buttons")]
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button invadeButton;
    [SerializeField] private Button researchButton;
    
    //[Header("Archetypes")]
    //[SerializeField] private SkillTreeSelector _skillTreeSelector;
    
    [Header("Demo")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    
    [Header("Steam")]
    [SerializeField] private TextMeshProUGUI steamName;
    
    private void Awake() {
        Instance = this;
    }
    private void Start() {
        newGameButton.interactable = true;
        steamName.text = $"Logged in as: <b>{SteamworksManager.Instance.GetSteamName()}</b>";
    }
    public void ShowMenuButtons() {
        titleTween.OnValueChangedAnimation(true);
        glowTween.OnValueChangedAnimation(true);
        // if (WorldConfigManager.Instance.isTutorialWorld) {
        //     (startButton.transform as RectTransform).DOAnchorPosY(136f, 1f).SetEase(Ease.Linear);
        //     (settingsButton.transform as RectTransform).DOAnchorPosY(92f, 1f).SetEase(Ease.Linear).SetDelay(0.5f);
        // } else {
        buttonsTween.OnValueChangedAnimation(true);
        // }
        
    }
    private void HideMenuButtons() {
        buttonsTween.OnValueChangedAnimation(false);
    }
    public void ExitGame() {
        //TODO: Add Confirmation Prompt
        Application.Quit();
    }
    public void Glow2TweenPlayForward() {
        glow2Tween.OnValueChangedAnimation(true);
    }
    public void Glow2TweenPlayReverse() {
        glow2Tween.OnValueChangedAnimation(false);
    }
    public void GlowTweenPlayForward() {
        glowTween.OnValueChangedAnimation(true);
    }
    public void GlowTweenPlayReverse() {
        glowTween.OnValueChangedAnimation(false);
    }
    public void OnClickPlayGame() {
        WorldSettings.Instance.Open();
    }
    private void OnCompleteBGTween() {
        invadeButton.gameObject.SetActive(true);
        researchButton.gameObject.SetActive(true);
    }
    public void OnClickInvade() {
        //StartNewGame();
        //WorldSettings.Instance.Open();
    }
    public void OnClickResearch() {
        //_skillTreeSelector.Show();
    }
    public void OnClickLoadGame() {
        SaveManager.Instance.useSaveData = true;
        newGameButton.interactable = false;
        loadGameButton.interactable = false;
        AudioManager.Instance.TransitionToLoading();
        MainMenuManager.Instance.LoadMainGameScene();
    }
    public void OnClickSettings() {
        SettingsManager.Instance.OpenSettings();
    }
    public void UpdateLoadButton() {
        loadGameButton.interactable = SaveManager.Instance.hasSavedDataCurrentProgress;
    }
}
