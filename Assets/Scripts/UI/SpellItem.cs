using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZObjectPools;
using TMPro;
using UnityEngine.UI;

public class SpellItem : NameplateItem<SpellData> {
    [SerializeField] private Image lockedImage;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private TextMeshProUGUI currencyLbl;

    public SpellData spellData { get; private set; }

    public override void SetObject(SpellData spellData) {
        base.SetObject(spellData);
        name = spellData.name;
        this.spellData = spellData;
        UpdateData();
        Messenger.AddListener<SpellData>(Signals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpell);
        Messenger.AddListener<SpellData>(Signals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
        Messenger.AddListener<SpellData>(Signals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
        Messenger.AddListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnExecuteSpell);
    }
    public void UpdateData() {
        mainLbl.text = spellData.name;
        currencyLbl.text = string.Empty;
        if (spellData.hasCharges) {
            currencyLbl.text += $"{UtilityScripts.Utilities.ChargesIcon()}{spellData.charges.ToString()} ";
        }
        if (spellData.hasManaCost) {
            currencyLbl.text += $"{UtilityScripts.Utilities.ManaIcon()}{spellData.manaCost.ToString()} ";
        }
        if (spellData.hasCooldown) {
            currencyLbl.text += $"{UtilityScripts.Utilities.CooldownIcon()}{spellData.cooldown.ToString()} ";
        }
        if (spellData.threat > 0) {
            currencyLbl.text += $"{UtilityScripts.Utilities.ThreatIcon()}{spellData.threat.ToString()} ";
        }
    }

    #region Listeners
    private void OnPlayerNoActiveSpell(SpellData spellData) {
        if(this.spellData == spellData) {
            if (_toggle.isOn) {
                _toggle.isOn = false;
            }
            UpdateData();
            UpdateInteractableState();
        }
    }
    private void OnSpellCooldownStarted(SpellData spellData) {
        if (this.spellData == spellData) {
            if (spellData.hasCharges && spellData.charges <= 0) {
                //if spell uses charges, but has no more, do not show cooldown icon even if it is in cooldown
                SetCooldownState(false);
            } else {
                SetCooldownState(spellData.isInCooldown);
            }
            UpdateData();
            UpdateInteractableState();
        }
    }
    private void OnSpellCooldownFinished(SpellData spellData) {
        if (this.spellData == spellData) {
            SetCooldownState(spellData.isInCooldown);
            UpdateData();
            UpdateInteractableState();
        }
    }
    private void OnExecuteSpell(SpellData spellData) {
        if (this.spellData == spellData) {
            UpdateData();
            UpdateInteractableState();
        }
    }
    #endregion
    
    public void OnToggleSpell(bool state) {
        PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
        if (state) {
            PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(spellData);
        }
    }
    public virtual void OnHoverSpell() {
        PlayerUI.Instance.OnHoverSpell(spellData, PlayerUI.Instance.spellListHoverPosition);
    }
    public void OnHoverOutSpell() {
        PlayerUI.Instance.OnHoverOutSpell(spellData);
    }

    #region Interactability
    public void SetLockedState(bool state) {
        lockedImage.gameObject.SetActive(state);
    }
    private void SetCooldownState(bool state) {
        cooldownImage.gameObject.SetActive(state);
    }
    private void UpdateInteractableState() {
        SetInteractableState(spellData.CanPerformAbility());
    }
    #endregion

    public override void Reset() {
        base.Reset();
        SetInteractableState(true);
        spellData = null;
        Messenger.RemoveListener<SpellData>(Signals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpell);
        Messenger.RemoveListener<SpellData>(Signals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
        Messenger.RemoveListener<SpellData>(Signals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
        Messenger.RemoveListener<SpellData>(Signals.ON_EXECUTE_SPELL, OnExecuteSpell);
    }
}
