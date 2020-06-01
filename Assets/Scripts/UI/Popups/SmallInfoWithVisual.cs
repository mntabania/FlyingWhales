using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.Video;

public class SmallInfoWithVisual : MonoBehaviour {
    
    [Header("Small Info with Visual")]
    [SerializeField] private GameObject smallInfoVisualGO;
    [SerializeField] private RectTransform smallInfoVisualRT;
    [SerializeField] private VideoPlayer smallInfoVideoPlayer;
    [SerializeField] private RenderTexture smallInfoVisualRenderTexture;
    [SerializeField] private RawImage smallInfoVisualImage;
    [SerializeField] private TextMeshProUGUI smallInfoVisualLbl;

    public void ShowSmallInfo(string info, [NotNull]VideoClip videoClip, string header = "", UIHoverPosition pos = null) {
        Assert.IsNotNull(videoClip, "Small info with visual was called but no video clip was provided");
        string message = string.Empty;
        if (!string.IsNullOrEmpty(header)) {
            message = $"<font=\"Eczar-Medium\"><line-height=100%><size=18>{header}</font>\n";
        }
        message = $"{message}<line-height=70%><size=16>{info}";

        message = message.Replace("\\n", "\n");

        smallInfoVisualLbl.text = message;
        if (!UIManager.Instance.IsSmallInfoShowing()) {
            smallInfoVisualGO.transform.SetParent(transform);
            smallInfoVisualGO.SetActive(true);
        }
        if (pos == null) {
            UIManager.Instance.PositionTooltip(smallInfoVisualGO, smallInfoVisualRT, smallInfoVisualRT);    
        } else {
            UIManager.Instance.PositionTooltip(pos, smallInfoVisualGO, smallInfoVisualRT);
        }
        if (smallInfoVisualImage.texture != smallInfoVisualRenderTexture) {
            smallInfoVisualImage.texture = smallInfoVisualRenderTexture;    
        }
        if (smallInfoVideoPlayer.clip != videoClip) {
            smallInfoVideoPlayer.clip = videoClip;
            smallInfoVideoPlayer.Stop();
            smallInfoVideoPlayer.Play();    
        }
    }
    public void ShowSmallInfo(string info, Texture visual, string header = "", UIHoverPosition pos = null) {
        Assert.IsNotNull(visual, "Small info with visual was called but no visual was provided");
        string message = string.Empty;
        if (!string.IsNullOrEmpty(header)) {
            message = $"<font=\"Eczar-Medium\"><line-height=100%><size=18>{header}</font>\n";
        }
        message = $"{message}<line-height=70%><size=16>{info}";

        message = message.Replace("\\n", "\n");

        smallInfoVisualLbl.text = message;
        if (!UIManager.Instance.IsSmallInfoShowing()) {
            smallInfoVisualGO.transform.SetParent(transform);
            smallInfoVisualGO.SetActive(true);
        }
        if (pos == null) {
            UIManager.Instance.PositionTooltip(smallInfoVisualGO, smallInfoVisualRT, smallInfoVisualRT);    
        } else {
            UIManager.Instance.PositionTooltip(pos, smallInfoVisualGO, smallInfoVisualRT);
        }
        smallInfoVisualImage.texture = visual;
    }

    public void Hide() {
        gameObject.SetActive(false);
    }

    #region Monobehaviours
    private void OnDisable() {
        smallInfoVideoPlayer.Stop();
    }
    #endregion
    
}
