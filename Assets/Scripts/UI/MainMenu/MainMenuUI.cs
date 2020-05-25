using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Settings;

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
    
    [Header("Archetypes")]
    [SerializeField] private SkillTreeSelector _skillTreeSelector;
    
    [Header("Demo")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button settingsButton;
    
    private void Awake() {
        Instance = this;
    }
    private void Start() {
        newGameButton.interactable = true;
        loadGameButton.interactable = false;
    }
    public void ShowMenuButtons() {
        titleTween.OnValueChangedAnimation(true);
        glowTween.OnValueChangedAnimation(true);
        if (WorldConfigManager.Instance.isDemoWorld) {
            (startButton.transform as RectTransform).DOAnchorPosY(136f, 1f).SetEase(Ease.Linear);
            (settingsButton.transform as RectTransform).DOAnchorPosY(92f, 1f).SetEase(Ease.Linear).SetDelay(0.5f);
        } else {
            buttonsTween.OnValueChangedAnimation(true);
        }
        
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
        if (WorldConfigManager.Instance.isDemoWorld) {
            newGameButton.interactable = false;
            loadGameButton.interactable = false;
            StartNewGame();
        } else {
            bg.DOFade(0f, 1f).OnComplete(OnCompleteBGTween);
            newGameButton.interactable = false;
            loadGameButton.interactable = false;
            HideMenuButtons();
            titleTween.OnValueChangedAnimation(false);
        }
    }
    private void OnCompleteBGTween() {
        invadeButton.gameObject.SetActive(true);
        researchButton.gameObject.SetActive(true);
    }
    public void OnClickInvade() {
        StartNewGame();
    }
    public void OnClickResearch() {
        _skillTreeSelector.Show();
    }
    public void OnClickLoadGame() {
        newGameButton.interactable = false;
        loadGameButton.interactable = false;
        AudioManager.Instance.TransitionToLoading();
        MainMenuManager.Instance.LoadMainGameScene();
    }
    public void OnClickSettings() {
        SettingsManager.Instance.OpenSettings();
    }
    public void StartNewGame() {
        //SaveManager.Instance.SetCurrentSave(null);
        newGameButton.interactable = false;
        loadGameButton.interactable = false;
        AudioManager.Instance.TransitionToLoading();
        LevelLoaderManager.Instance.UpdateLoadingInfo("Initializing data...");
        LevelLoaderManager.Instance.UpdateLoadingBar(0.1f, 3f);
        MainMenuManager.Instance.LoadMainGameScene();
    }
}
