using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using EZObjectPools;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;

public class ActionItem : PooledObject {

	public PlayerAction playerAction { get; private set; }
    public IPlayerActionTarget playerActionTarget { get; private set; }

    [SerializeField] private Button button;
	[SerializeField] private Image actionImg;
    [SerializeField] private Image coverImg;
    [SerializeField] private Image cooldownCoverImg;
    [SerializeField] private Image highlightImg;
    [SerializeField] private TextMeshProUGUI actionLbl;
    [SerializeField] private UIHoverPosition _hoverPosition;
    [SerializeField] private Image cooldownImage;
	private string expiryKey;
	
	public void SetAction(PlayerAction playerAction, IPlayerActionTarget playerActionTarget) {
		name = playerAction.localizedName;
        this.playerAction = playerAction;
        this.playerActionTarget = playerActionTarget;
        UnToggleHighlight();
        actionImg.sprite = PlayerSkillManager.Instance.GetScriptableObjPlayerSkillData<PlayerSkillData>(playerAction.type).playerActionIcon;
        actionLbl.text = playerAction.GetLabelName(playerActionTarget);
        gameObject.SetActive(true);
        Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
        Messenger.AddListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
        Messenger.AddListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnPlayerAdjustedMana);
    }
    public void SetInteractable(bool state) {
        button.interactable = state;
        coverImg.gameObject.SetActive(!state);
        //UpdateCooldown();
    }
    private void UpdateInteractableState() {
        SetInteractable(playerAction.CanPerformAbility());
    }
    private void UpdateCooldown() {
        cooldownCoverImg.gameObject.SetActive(playerAction.isInCooldown);
        if (cooldownCoverImg.gameObject.activeSelf) {
            cooldownCoverImg.fillAmount = 0f;
        }
    }
    public void ToggleHighlight() {
        if (button.interactable) {
            SetHighlightState(true);    
        }
        OnHover();
    }
    public void UnToggleHighlight() {
        if (button.interactable) {
            bool toggle = false;
            if (playerAction != null && PlayerManager.Instance != null) {
                if (playerAction.type == PLAYER_SKILL_TYPE.SPAWN_EYE_WARD || playerAction.type == PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_EYE_LEVEL
                    || playerAction.type == PLAYER_SKILL_TYPE.UPGRADE_BEHOLDER_RADIUS_LEVEL) {
                    toggle = PlayerManager.Instance.player.currentActivePlayerSpell == playerAction;
                }
            }
            SetHighlightState(toggle);    
        }
        OnHoverOut();
    }
    public void SetHighlightState(bool p_state) {
        highlightImg.gameObject.SetActive(p_state);
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
    private void OnSpellCooldownStarted(SkillData spellData) {
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
    private void OnSpellCooldownFinished(SkillData spellData) {
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
        cooldownCoverImg.gameObject.SetActive(state);
        //cooldownImage.gameObject.SetActive(state);
    }
    private void StartCooldownFill() {
        cooldownCoverImg.fillAmount = ((float)playerAction.currentCooldownTick / playerAction.cooldown);
	    Messenger.AddListener(Signals.TICK_STARTED, PerTickCooldown);
    }
    private void PerTickCooldown() {
#if DEBUG_PROFILER
	    Profiler.BeginSample($"Action Item Per Tick Cooldown");
#endif
	    float fillAmount = ((float)playerAction.currentCooldownTick / playerAction.cooldown);
        cooldownCoverImg.DOFillAmount(fillAmount, 0.4f);
#if DEBUG_PROFILER
        Profiler.EndSample();
#endif
    }
    private void StopCooldownFill() {
        SetCooldownState(false);
	    // coverImg.DOFillAmount(0f, 0.2f).OnComplete(() => SetCooldownState(false));
	    Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
    }
    private void OnPlayerAdjustedMana(int adjusted, int mana) {
        UpdateInteractableState();
    }
#endregion

    public override void Reset() {
		base.Reset();
		name = "Action Item";
		button.onClick.RemoveAllListeners();
		if (string.IsNullOrEmpty(expiryKey) == false) {
			SchedulingManager.Instance.RemoveSpecificEntry(expiryKey);
		}
        playerAction = null;
		DOTween.Kill(this);
        cooldownCoverImg.fillAmount = 0f;
		expiryKey = string.Empty;
		cooldownCoverImg.gameObject.SetActive(false);
		SetInteractable(true);
		Messenger.RemoveListener(Signals.TICK_STARTED, PerTickCooldown);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
		Messenger.RemoveListener<SkillData>(PlayerSkillSignals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
        Messenger.RemoveListener<int, int>(PlayerSignals.PLAYER_ADJUSTED_MANA, OnPlayerAdjustedMana);
    }
}
