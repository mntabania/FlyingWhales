﻿using System;
using System.Collections;
using System.Collections.Generic;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionItem : PooledObject {

	public PlayerAction playerAction { get; private set; }
    public IPlayerActionTarget playerActionTarget { get; private set; }

    [SerializeField] private Button button;
	[SerializeField] private Image actionImg;
	[SerializeField] private Image coverImg;
    [SerializeField] private Image highlightImg;
    [SerializeField] private TextMeshProUGUI actionLbl;
    [SerializeField] private UIHoverPosition _hoverPosition;
    [SerializeField] private Image cooldownImage;
	private string expiryKey;
	
	public void SetAction(PlayerAction playerAction, IPlayerActionTarget playerActionTarget) {
        this.playerAction = playerAction;
        this.playerActionTarget = playerActionTarget;
        UnToggleHighlight();
        actionImg.sprite = PlayerUI.Instance.playerActionsIconDictionary[playerAction.type];
        actionLbl.text = playerAction.GetLabelName(playerActionTarget);
		SetAsClickable();
		SetCooldownState(playerAction.isInCooldown);
        Messenger.AddListener<SpellData>(Signals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
        Messenger.AddListener<SpellData>(Signals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
	}
	public void SetAsUninteractableUntil(int ticks) {
		GameDate date = GameManager.Instance.Today();
		date = date.AddTicks(ticks);
		SetAsUninteractableUntil(date);
	}
	public void SetAsUninteractableUntil(GameDate date) {
        SetInteractable(false);
        expiryKey = SchedulingManager.Instance.AddEntry(date, SetAsClickable, this);
	}
	private void SetAsClickable() {
        SetInteractable(true);
	}
    public void SetInteractable(bool state) {
        button.interactable = state;
        coverImg.gameObject.SetActive(!state);
    }
    public void ToggleHighlight() {
        //if (!playerAction.isInstant) {
        if (button.interactable) {
	        highlightImg.gameObject.SetActive(true);    
        }
        OnHover();
            // UpdateState();
        //}
    }
    public void UnToggleHighlight() {
        //if (!playerAction.isInstant) {
        if (button.interactable) {
	        highlightImg.gameObject.SetActive(false);    
        }
        OnHoverOut();
            // UpdateState();
        //}
    }
    public void OnClickThis() {
        if (playerAction != null) {
            ToggleHighlight();
            playerAction.Activate(playerActionTarget);
        }
    }
    public void OnHover() {
        PlayerUI.Instance.OnHoverSpell(playerAction, _hoverPosition);
        //UIManager.Instance.ShowSmallInfo(playerAction.description + "\n" + playerAction.GetManaCostChargesCooldownStr());
    }
    public void OnHoverOut() {
        //UIManager.Instance.HideSmallInfo();
        PlayerUI.Instance.OnHoverOutSpell(playerAction);
    }

    #region Listeners
    private void ListenUntoggleHighlight(PlayerAction action) {
        if(action == playerAction) {
            UnToggleHighlight();
        }
    }
    private void OnSpellCooldownStarted(SpellData spellData) {
	    if (this.playerAction == spellData) {
		    if (spellData.hasCharges && spellData.charges <= 0) {
			    //if spell uses charges, but has no more, do not show cooldown icon even if it is in cooldown
			    SetCooldownState(false);
		    } else {
			    SetCooldownState(spellData.isInCooldown);
		    }
	    }
    }
    private void OnSpellCooldownFinished(SpellData spellData) {
	    if (playerAction == spellData) {
		    SetCooldownState(playerAction.isInCooldown);
	    }
    }
    #endregion

    #region Cooldown
    private void SetCooldownState(bool state) {
	    cooldownImage.gameObject.SetActive(state);
    }
    #endregion

    public override void Reset() {
		base.Reset();
		button.onClick.RemoveAllListeners();
		if (string.IsNullOrEmpty(expiryKey) == false) {
			SchedulingManager.Instance.RemoveSpecificEntry(expiryKey);
		}
		expiryKey = string.Empty;
		Messenger.RemoveListener<SpellData>(Signals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
		Messenger.RemoveListener<SpellData>(Signals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
	}
}
