using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StringPickerItem : ObjectPickerItem<string>, IPointerClickHandler {

    public Action<string> onClickAction;

    private string str;

    public GameObject portraitCover;
    public Image iconImg;

    public string identifier;

    public override string obj { get { return str; } }

    public void SetString(string str, string identifier) {
        this.str = str;
        this.identifier = identifier;
        iconImg.gameObject.SetActive(false);
        UpdateVisuals();
    }

    public override void SetButtonState(bool state) {
        base.SetButtonState(state);
        portraitCover.SetActive(!state);
    }

    private void UpdateVisuals() {
        mainLbl.text = str;
        if(identifier != string.Empty) {
            if (identifier == "trait") {
                if (TraitManager.Instance.HasTraitIcon(str)) {
                    iconImg.sprite = TraitManager.Instance.GetTraitPortrait(str);
                    iconImg.gameObject.SetActive(true);
                } else {
                    iconImg.gameObject.SetActive(false);
                }
            } else if(identifier == "landmark") {
                LandmarkData landmarkData = LandmarkManager.Instance.GetLandmarkData(str);
                iconImg.sprite = landmarkData.defaultLandmarkPortrait;
                iconImg.gameObject.SetActive(true);
            } else if (identifier == "intervention ability") {
                iconImg.sprite = PlayerManager.Instance.GetJobActionSprite(str);
                iconImg.gameObject.SetActive(true);
            } else if (identifier == "minion") {
                iconImg.sprite = CharacterManager.Instance.GetWholeImagePortraitSprite(str);
                iconImg.gameObject.SetActive(true);
            }
            iconImg.SetNativeSize();
            if (iconImg.sprite == null) {
                iconImg.gameObject.SetActive(false);
            }
        }
    }

    private void OnClick() {
        if (onClickAction != null) {
            onClickAction.Invoke(str);
        }
    }

    public void OnPointerClick(PointerEventData eventData) {
        OnClick();
    }
    //public override void OnHoverEnter() {

    //}
    //public override void OnHoverExit() {
    //    //Only set as hide small info for now, but should use on hover exit action
    //    UIManager.Instance.HideSmallInfo();
    //}
}
