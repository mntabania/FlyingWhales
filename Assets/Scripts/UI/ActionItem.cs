using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
		name = playerAction.name;
        this.playerAction = playerAction;
        this.playerActionTarget = playerActionTarget;
        UnToggleHighlight();
        actionImg.sprite = PlayerUI.Instance.playerActionsIconDictionary[playerAction.type];
        actionLbl.text = playerAction.GetLabelName(playerActionTarget);
        gameObject.SetActive(true);
        Messenger.AddListener<SpellData>(Signals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
        Messenger.AddListener<SpellData>(Signals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
	}
	public void SetInteractable(bool state) {
        button.interactable = state;
        coverImg.gameObject.SetActive(!state);
        if (coverImg.gameObject.activeSelf) {
	        coverImg.fillAmount = 1;
        }
    }
    public void ToggleHighlight() {
        if (button.interactable) {
	        highlightImg.gameObject.SetActive(true);    
        }
        OnHover();
    }
    public void UnToggleHighlight() {
        if (button.interactable) {
	        highlightImg.gameObject.SetActive(false);    
        }
        OnHoverOut();
    }
    public void OnClickThis() {
        if (playerAction != null) {
	        ToggleHighlight();
            playerAction.Activate(playerActionTarget);
        }
    }
    public void OnHover() {
        PlayerUI.Instance.OnHoverSpell(playerAction, _hoverPosition);
    }
    public void OnHoverOut() {
        PlayerUI.Instance.OnHoverOutSpell(playerAction);
    }

    #region Listeners
    private void OnSpellCooldownStarted(SpellData spellData) {
	    if (this.playerAction == spellData) {
		    // if (spellData.hasCharges && spellData.charges <= 0) {
			   //  //if spell uses charges, but has no more, do not show cooldown icon even if it is in cooldown
			   //  SetCooldownState(false);
		    // } else {
			    SetCooldownState(true);
			    StartCooldownFill();
			    // SetCooldownState(spellData.isInCooldown);
		    // }
	    }
    }
    private void OnSpellCooldownFinished(SpellData spellData) {
	    if (playerAction == spellData) {
		    // SetCooldownState(playerAction.isInCooldown);
		    StopCooldownFill();
	    }
    }
    #endregion

    #region Cooldown
    public void ForceUpdateCooldown() {
	    if (playerAction.isInCooldown) {
		    OnSpellCooldownStarted(playerAction);
	    } else {
		    SetCooldownState(false);    
	    }
    }
    private void SetCooldownState(bool state) {
	    SetInteractable(playerAction.CanPerformAbilityTo(playerActionTarget) && !PlayerManager.Instance.player.seizeComponent.hasSeizedPOI);
	    cooldownImage.gameObject.SetActive(state);
    }
    private void StartCooldownFill() {
	    coverImg.fillAmount = 1f - ((float)playerAction.currentCooldownTick / playerAction.cooldown);
	    Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
    }
    private void PerTickCooldown() {
	    float fillAmount = 1f - ((float)playerAction.currentCooldownTick / playerAction.cooldown);
	    coverImg.DOFillAmount(fillAmount, 0.2f);
    }
    private void StopCooldownFill() {
	    SetCooldownState(false);
	    // coverImg.DOFillAmount(0f, 0.2f).OnComplete(() => SetCooldownState(false));
	    Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
    }
    #endregion

    public override void Reset() {
		base.Reset();
		name = "Action Item";
		button.onClick.RemoveAllListeners();
		if (string.IsNullOrEmpty(expiryKey) == false) {
			SchedulingManager.Instance.RemoveSpecificEntry(expiryKey);
		}
		DOTween.Kill(this);
		coverImg.fillAmount = 1;
		expiryKey = string.Empty;
		SetCooldownState(false);
		SetInteractable(true);
		Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
		Messenger.RemoveListener<SpellData>(Signals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
		Messenger.RemoveListener<SpellData>(Signals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
	}
}
