using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class TipsItemUI : MonoBehaviour {

    public Action<TIPS> onClickTip;

    public TIPS tip;
    [SerializeField] private TextMeshProUGUI lblTipsDescription;
    [SerializeField] private EnvelopContentUnityUI envelopContent;
    [SerializeField] private CanvasGroup canvasGroup;

    public Button btnClick;

	private void OnEnable() {
        btnClick.onClick.AddListener(BtnClick);
    }

	private void OnDisable() {
        btnClick.onClick.RemoveListener(BtnClick);
    }

    public void BtnClick() {
        onClickTip?.Invoke(tip);
    }
    public void SetDescription(string p_description) {
        lblTipsDescription.text = p_description;
        envelopContent.Execute();
    }
    public void PlayIntroAnimation() {
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, 1f);
    }
}
