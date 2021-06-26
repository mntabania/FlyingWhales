using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Quests;
using Ruinarch;
using Settings;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.UI;

public class PopUpScreensUI : MonoBehaviour {

    [Header("Summary Screen")]
    [SerializeField] private CanvasGroup summaryScreen;
    [SerializeField] private TextMeshProUGUI summaryLbl;
    
    [Header("Start Screen")] 
    [SerializeField] private GameObject startScreen;
    [SerializeField] private Image startMessageWindow;
    [SerializeField] private CanvasGroup startMessageWindowCG;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TextMeshProUGUI startGameButtonLbl;
    [SerializeField] private Toggle skipTutorialsToggle;
    [SerializeField] private GameObject skipTutorialsBG;
    [SerializeField] private TextMeshProUGUI lblStartScreen;
    
    [Header("End Screen")]
    [SerializeField] private GameObject endScreen;
    [SerializeField] private Image bgImage;
    [SerializeField] private Image ruinarchLogo;
    [SerializeField] private RectTransform thankYouWindow;
    [SerializeField] private CanvasGroup endScreenCanvasGroup;
    [SerializeField] private Button btnJoinDiscord;
    [SerializeField] private Button btnSurvey;
    [SerializeField] private Button btnContinue;

    #region Start Screen
    public void ShowStartScreen(string message) {
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);
        InnerMapCameraMove.Instance.DisableMovement();
        InputManager.Instance.SetAllHotkeysEnabledState(false);
        startScreen.gameObject.SetActive(true);
        startMessageWindow.gameObject.SetActive(true);
        
        //set image starting size
        RectTransform startWindowRT = startMessageWindow.rectTransform;
        startWindowRT.anchoredPosition = new Vector2(0f, -100f);

        startMessageWindowCG.alpha = 0;

        skipTutorialsToggle.gameObject.SetActive(WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial);
        skipTutorialsBG.SetActive(WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial);
        skipTutorialsToggle.SetIsOnWithoutNotify(SettingsManager.Instance.settings.skipTutorials);

        lblStartScreen.SetText(message);
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(startWindowRT.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBack));
        sequence.Join(startMessageWindowCG.DOFade(1f, 0.5f).SetEase(Ease.InSine));
        sequence.Play();
    }
    public void OnClickStartGameButton() {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(startMessageWindowCG.DOFade(0f, 0.5f).SetEase(Ease.OutSine));
        sequence.OnComplete(HideStartDemoScreen);
        sequence.Play();
    }
    private void HideStartDemoScreen() {
        startScreen.gameObject.SetActive(false);
        UIManager.Instance.SetSpeedTogglesState(true);
        InnerMapCameraMove.Instance.EnableMovement();
        InputManager.Instance.SetAllHotkeysEnabledState(true);

        // if (WorldSettings.Instance.worldSettingsData.worldType == WorldSettingsData.World_Type.Tutorial) {
        //     TutorialManager.Instance.InstantiateImportantTutorials();
        //     // TutorialManager.Instance.InstantiatePendingBonusTutorials();
        //     QuestManager.Instance.InitializeAfterStartTutorial();    
        // }
    }
    public void OnToggleSkipTutorials(bool state) {
        SettingsManager.Instance.OnToggleSkipTutorials(state);
    }
    #endregion
    
    #region End Screen
    public bool IsShowingEndScreen() {
        return summaryScreen.gameObject.activeInHierarchy || endScreen.activeInHierarchy;
    }
    public void ShowSummaryThenEndScreen(string summary) {
        GameManager.Instance.SetPausedState(true);
        UIManager.Instance.SetSpeedTogglesState(false);
        UIManager.Instance.HideSmallInfo();
        
        summaryScreen.alpha = 0f;
        summaryScreen.gameObject.SetActive(true);
        
        summaryLbl.text = summary;
        
        RectTransform summaryLblRT = summaryLbl.rectTransform;
        summaryLblRT.anchoredPosition = new Vector2(0f, -100f);

        Sequence sequence = DOTween.Sequence();
        sequence.Append(summaryScreen.DOFade(1f, 0.5f));
        sequence.Append(summaryLblRT.DOAnchorPosY(0f, 0.5f).SetEase(Ease.OutBack));
        sequence.Join(DOTween.ToAlpha(() => summaryLbl.color, value => summaryLbl.color = value, 1f, 0.5f));
        sequence.AppendInterval(1.5f);
        sequence.OnComplete(ShowEndScreen);
        sequence.Play();
    }
    
    private void ShowEndScreen() {
        InputManager.Instance.SetCursorTo(InputManager.Cursor_Type.Default);
        endScreen.SetActive(true);
        // //bg image
        // Color fromColor = bgImage.color;
        // fromColor.a = 0f;
        // bgImage.color = fromColor;
        // bgImage.DOFade(1f, 2f).SetEase(Ease.InQuint).OnComplete(ShowLogoAndThankYou);
        endScreenCanvasGroup.alpha = 0f;
        ShowButtonBaseOnGameResult();
        endScreenCanvasGroup.DOFade(1f, 2f).SetEase(Ease.InQuint).OnComplete(ShowLogoAndThankYou);
    }

    private void ShowButtonBaseOnGameResult() {
        if (!PlayerManager.Instance.player.hasAlreadyWon) {
            Vector3 pos = btnJoinDiscord.transform.position;
            pos.y += 80f;
            btnJoinDiscord.transform.position = pos;
            pos = btnSurvey.transform.position;
            pos.y += 80f;
            btnSurvey.transform.position = pos;
            btnContinue.gameObject.SetActive(false);
        } else {
            btnContinue.gameObject.SetActive(true);
        }
    }

    private void ShowLogoAndThankYou() {
        Color fromColor = bgImage.color;
        fromColor.a = 0f;
        //logo
        ruinarchLogo.color = fromColor;
        ruinarchLogo.DOFade(1f, 1f);
        RectTransform logoRT = ruinarchLogo.transform as RectTransform;
        logoRT.anchoredPosition = Vector2.zero;
        logoRT.DOAnchorPosY(221f, 0.5f).SetEase(Ease.OutQuad);
        
        //thank you
        thankYouWindow.anchoredPosition = new Vector2(0f, -300f);
        thankYouWindow.DOAnchorPosY(360f, 1f).SetEase(Ease.OutQuad);
    }

    public void OnClickContinuePlaying() {
        GameManager.Instance.SetPausedState(false);
        UIManager.Instance.SetSpeedTogglesState(true);
        DOTween.Clear(true);
        HideScreens();
    }

    void HideScreens() {
        summaryScreen.gameObject.SetActive(false);
        endScreen.SetActive(false);
    }

    public void OnClickReturnToMainMenu() {
        DOTween.Clear(true);
        LevelLoaderManager.Instance.UpdateLoadingInfo(string.Empty);
        LevelLoaderManager.Instance.LoadLevel("MainMenu");
    }
    public void OnClickWishList() {
        Application.OpenURL("https://store.steampowered.com/app/909320/Ruinarch/");
    }
    public void OnClickLeaveFeedback() {
        Application.OpenURL("https://forms.gle/6QYHiSmU8ySVGSXp7");
    }
    public void OnClickJoinDiscord() {
        Application.OpenURL("http://discord.ruinarch.com/");
    }
    #endregion
}
