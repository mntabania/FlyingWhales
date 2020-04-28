using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DemoUI : MonoBehaviour {

    [SerializeField] private Image bgImage;
    [SerializeField] private Image ruinarchLogo;
    [SerializeField] private RectTransform thankYouWindow;

    public void Show() {
        GameManager.Instance.SetPausedState(true);
        UIManager.Instance.SetSpeedTogglesState(false);
        
        gameObject.SetActive(true);

        //bg image
        Color fromColor = bgImage.color;
        fromColor.a = 0f;
        bgImage.color = fromColor;
        bgImage.DOFade(1f, 4f).SetEase(Ease.InQuint).OnComplete(ShowLogoAndThankYou);
    }

    private void ShowLogoAndThankYou() {
        Color fromColor = bgImage.color;
        fromColor.a = 0f;
        //logo
        ruinarchLogo.color = fromColor;
        ruinarchLogo.DOFade(1f, 1f);
        RectTransform logoRT = ruinarchLogo.transform as RectTransform;
        logoRT.anchoredPosition = Vector2.zero;
        logoRT.DOAnchorPosY(-112.5f, 0.5f).SetEase(Ease.OutQuad);
        
        //thank you
        thankYouWindow.anchoredPosition = new Vector2(0f, -145f);
        thankYouWindow.DOAnchorPosY(145f, 2f).SetEase(Ease.OutQuad);
    }
    
    public void OnClickReturnToMainMenu() {
        DOTween.Clear(true);
        LevelLoaderManager.Instance.LoadLevel("MainMenu");
    }
    public void OnClickWishList() {
        Application.OpenURL("https://store.steampowered.com/app/909320/Ruinarch/");
    }
}
