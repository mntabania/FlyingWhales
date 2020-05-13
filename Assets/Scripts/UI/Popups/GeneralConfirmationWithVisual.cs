using DG.Tweening;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.Video;

public class GeneralConfirmationWithVisual : GeneralConfirmation {
    [SerializeField] private RawImage picture;
    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private RenderTexture _renderTexture;
    [SerializeField] private CanvasGroup _canvasGroup;
    
    public void ShowGeneralConfirmation(string header, string body, [NotNull]Texture sprite, string buttonText = "OK", System.Action onClickOK = null) {
        Assert.IsNotNull(sprite, "Trying to show general confirmation with visual, but no visual was provided");
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowGeneralConfirmation(header, body, sprite, buttonText, onClickOK));
            return;
        }
        base.ShowGeneralConfirmation(header, body, buttonText, onClickOK);
        SetVisual(sprite);
        TweenIn();
    }
    public void ShowGeneralConfirmation(string header, string body, [NotNull]VideoClip videoClip, string buttonText = "OK", System.Action onClickOK = null) {
        Assert.IsNotNull(videoClip, "Trying to show general confirmation with visual, but no visual was provided");
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowGeneralConfirmation(header, body, videoClip, buttonText, onClickOK));
            return;
        }
        base.ShowGeneralConfirmation(header, body, buttonText, onClickOK);
        SetVisual(videoClip);
        TweenIn();
    }

    private void TweenIn() {
        _canvasGroup.alpha = 0;
        RectTransform rectTransform = _canvasGroup.transform as RectTransform; 
        rectTransform.anchoredPosition = new Vector2(0f, -30f);
        
        Sequence sequence = DOTween.Sequence();
        sequence.Append(rectTransform.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBack));
        sequence.Join(DOTween.To(() => _canvasGroup.alpha, x => _canvasGroup.alpha = x, 1f, 0.5f)
            .SetEase(Ease.InSine));
        sequence.PrependInterval(0.2f);
        sequence.Play();
    }

    #region Visual
    private void SetVisual(Texture texture) {
        _videoPlayer.Stop();
        picture.texture = texture;
    }
    private void SetVisual(VideoClip videoClip) {
        _videoPlayer.clip = videoClip;
        _videoPlayer.Play();
        _videoPlayer.targetTexture = _renderTexture;
        picture.texture = _renderTexture;
    }
    #endregion
}
