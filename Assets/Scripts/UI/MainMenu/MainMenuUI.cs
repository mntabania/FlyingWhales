using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour {

    public static MainMenuUI Instance = null;

    [SerializeField] private EasyTween buttonsTween;
    [SerializeField] private EasyTween titleTween;

    [SerializeField] private EasyTween glowTween;
    [SerializeField] private EasyTween glow2Tween;

    [Header("Buttons")]
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button newGameButton;
    
    [Header("Archetypes")]
    [SerializeField] private ArchetypeSelector _archetypeSelector;
    
    private void Awake() {
        Instance = this;
    }
    private void Start() {
        newGameButton.interactable = true;
        loadGameButton.interactable = false;
    }
    public void ShowMenuButtons() {
        buttonsTween.OnValueChangedAnimation(true);
        titleTween.OnValueChangedAnimation(true);
        glowTween.OnValueChangedAnimation(true);
    }
    public void HideMenuButtons() {
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
       _archetypeSelector.Show();
    }
    public void OnClickLoadGame() {
        newGameButton.interactable = false;
        loadGameButton.interactable = false;
        AudioManager.Instance.TransitionTo("Loading", 10, MainMenuManager.Instance.LoadMainGameScene);
    }

    public void StartNewGame() {
        //SaveManager.Instance.SetCurrentSave(null);
        newGameButton.interactable = false;
        loadGameButton.interactable = false;
        AudioManager.Instance.TransitionTo("Loading", 10);
        MainMenuManager.Instance.LoadMainGameScene();
    }
}
