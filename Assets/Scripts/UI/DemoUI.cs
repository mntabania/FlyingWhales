using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Tutorial;
using UnityEngine;
using UnityEngine.UI;

public class DemoUI : MonoBehaviour {

    [Header("Summary Screen")]
    [SerializeField] private CanvasGroup summaryScreen;
    [SerializeField] private TextMeshProUGUI summaryLbl;
    
    [Header("Start Screen")] 
    [SerializeField] private GameObject startScreen;
    [SerializeField] private Image startMessageWindow;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TextMeshProUGUI startGameButtonLbl;
    
    [Header("End Screen")]
    [SerializeField] private GameObject endScreen;
    [SerializeField] private Image bgImage;
    [SerializeField] private Image ruinarchLogo;
    [SerializeField] private RectTransform thankYouWindow;

    #region Start Screen
    public void ShowStartScreen() {
        UIManager.Instance.Pause();
        UIManager.Instance.SetSpeedTogglesState(false);
        WorldMapCameraMove.Instance.DisableMovement();
        InnerMapCameraMove.Instance.DisableMovement();
        startScreen.gameObject.SetActive(true);
        startMessageWindow.gameObject.SetActive(true);
        
        //set image starting size
        RectTransform startWindowRT = startMessageWindow.rectTransform;
        startWindowRT.anchoredPosition = new Vector2(0f, -100f);
        Color color = startMessageWindow.color;
        color.a = 0f;
        startMessageWindow.color = color;
        
        // //set button starting alpha
        // Graphic graphic = startGameButton.targetGraphic; 
        // color = graphic.color;
        // color.a = 0f;
        // graphic.color = new Color(color.r, color.g, color.b, color.a);
        // startGameButtonLbl.alpha = 0f;
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(startWindowRT.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBack));
        sequence.Join(DOTween.ToAlpha(() => startMessageWindow.color, x => startMessageWindow.color = x, 1f, 0.5f)
            .SetEase(Ease.InSine));
        
        // sequence.Append(startGameButton.targetGraphic.DOFade(1f, 2f).SetEase(Ease.InCirc).SetDelay(3f).OnComplete(() => startGameButton.interactable = true));
        // sequence.Join(DOTween.ToAlpha(() => startGameButtonLbl.color, x => startGameButtonLbl.color = x, 1f, 2f).SetEase(Ease.InCirc));
        sequence.Play();
    }
    public void OnClickStartGameButton() {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(startMessageWindow.DOFade(0f, 0.5f).SetEase(Ease.OutSine));
        sequence.Join(startGameButton.targetGraphic.DOFade(0f, 0.5f).SetEase(Ease.OutSine));
        sequence.Join(DOTween.ToAlpha(() => startGameButtonLbl.color, x => startGameButtonLbl.color = x, 0f, 0.5f).SetEase(Ease.OutSine));
        sequence.OnComplete(HideStartDemoScreen);
        sequence.Play();
    }
    private void HideStartDemoScreen() {
        startScreen.gameObject.SetActive(false);
        UIManager.Instance.SetSpeedTogglesState(true);
        WorldMapCameraMove.Instance.EnableMovement();
        InnerMapCameraMove.Instance.EnableMovement();

        TutorialManager.Instance.InstantiatePendingTutorials();
    }
    #endregion
    
    #region End Screen
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
        endScreen.SetActive(true);

        //bg image
        Color fromColor = bgImage.color;
        fromColor.a = 0f;
        bgImage.color = fromColor;
        bgImage.DOFade(1f, 2f).SetEase(Ease.InQuint).OnComplete(ShowLogoAndThankYou);
        
        
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
    
    public void OnClickReturnToMainMenu() {
        DOTween.Clear(true);
        LevelLoaderManager.Instance.UpdateLoadingInfo(string.Empty);
        LevelLoaderManager.Instance.LoadLevel("MainMenu");
        Messenger.Cleanup();
    }
    public void OnClickWishList() {
        Application.OpenURL("https://store.steampowered.com/app/909320/Ruinarch/");
    }
    public void OnClickLeaveFeedback() {
        Application.OpenURL("https://forms.gle/vxD913oe3jBhkCE66");
    }
    public void OnClickJoinDiscord() {
        Application.OpenURL("http://discord.ruinarch.com/");
    }
    #endregion
}
