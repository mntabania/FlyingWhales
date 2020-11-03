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

    public void ShowGeneralConfirmation(string header, string body, [NotNull]Texture sprite, string buttonText = "OK", System.Action onClickOK = null) {
        Assert.IsNotNull(sprite, "Trying to show general confirmation with visual, but no visual was provided");
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowGeneralConfirmation(header, body, sprite, buttonText, onClickOK));
            return;
        }
        base.ShowGeneralConfirmation(header, body, buttonText, onClickOK);
        SetVisual(sprite);
    }
    public void ShowGeneralConfirmation(string header, string body, [NotNull]VideoClip videoClip, string buttonText = "OK", System.Action onClickOK = null) {
        Assert.IsNotNull(videoClip, "Trying to show general confirmation with visual, but no visual was provided");
        if (PlayerUI.Instance.IsMajorUIShowing()) {
            PlayerUI.Instance.AddPendingUI(() => ShowGeneralConfirmation(header, body, videoClip, buttonText, onClickOK));
            return;
        }
        base.ShowGeneralConfirmation(header, body, buttonText, onClickOK);
        if (!Settings.SettingsManager.Instance.doNotShowVideos) {
            SetVisual(videoClip);
        } else {
            //If there is no video clip to show, leave the left side of the window blank
            picture.texture = null;
        }
    }
    public override void Close() {
        if (_videoPlayer.clip != null) {
            _videoPlayer.Stop();
        }
        base.Close();
    }

    #region Visual
    private void SetVisual(Texture texture) {
        if(_videoPlayer.clip != null) {
            _videoPlayer.Stop();
        }
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
