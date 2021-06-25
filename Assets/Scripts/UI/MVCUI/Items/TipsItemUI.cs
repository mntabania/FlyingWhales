using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UtilityScripts;

public class TipsItemUI : MonoBehaviour {

    public Action<TIPS> onClickTip;

    public TIPS tip;
    [SerializeField] private TextMeshProUGUI lblTipsName;
    [SerializeField] private TextMeshProUGUI lblTipsDescription;

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

	public void SetName(string p_name) {
        lblTipsName.text = p_name;
    }
    public void SetDescription(string p_description) {
        lblTipsDescription.text = p_description;
    }
}
