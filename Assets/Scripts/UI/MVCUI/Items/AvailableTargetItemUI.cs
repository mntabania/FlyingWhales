﻿using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class AvailableTargetItemUI : MonoBehaviour {

    public Action<AvailableTargetItemUI> onClicked;
    public Action<AvailableTargetItemUI> onHoverOver;
    public Action<AvailableTargetItemUI> onHoverOut;
    public Button myButton;

    public RuinarchText txtName;
    public IStoredTarget target;

    public GameObject goCover;
    public HoverText hoverText;
    public HoverHandler hoverHandler;

	private void Awake() {
        hoverText = goCover.GetComponent<HoverText>();

    }
	public void InitializeItem(IStoredTarget p_target, string p_hoverText = "") {
        if (p_hoverText != string.Empty) {
            hoverText.hoverDisplayText = p_hoverText;
        }
        target = p_target;
        bool isCharacter = target is Character;
        Character targetCharacter = null;
        if (isCharacter) {
            targetCharacter = target as Character;
        }
        if (isCharacter && (target.isTargetted || (targetCharacter.currentStructure.structureType == STRUCTURE_TYPE.KENNEL || targetCharacter.currentStructure.structureType == STRUCTURE_TYPE.TORTURE_CHAMBERS))) {
            ShowCover();
        } else {
            HideCover();
        }
        txtName.text = $"{p_target.iconRichText} {p_target.name}";
    }

    public void SetHoverText(string p_text) {
        hoverText.hoverDisplayText = p_text;
    }

    private void OnEnable() {
        myButton.onClick.AddListener(Click);
        hoverHandler.AddOnHoverOverAction(OnHoverOver);
        hoverHandler.AddOnHoverOutAction(OnHoverOut);
    }

    private void OnDisable() {
        myButton.onClick.RemoveListener(Click);
        hoverHandler.RemoveOnHoverOverAction(OnHoverOver);
        hoverHandler.RemoveOnHoverOutAction(OnHoverOut);
    }

    public void ShowCover() {
        myButton.interactable = false;
        goCover.SetActive(true);
    }
    public void HideCover() {
        myButton.interactable = true;
        goCover.SetActive(false);
    }

    void Click() {
        onClicked?.Invoke(this);
    }
    private void OnHoverOver() {
        onHoverOver?.Invoke(this);
    }
    private void OnHoverOut() {
        onHoverOut?.Invoke(this);
    }
}