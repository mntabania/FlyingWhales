using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZObjectPools;
using TMPro;
using UnityEngine.UI;

public class SpellItem : PooledObject {
    public TextMeshProUGUI spellButtonText;
    public Toggle spellToggle;
    public GameObject cover;

    [SerializeField] private Image lockedImage;
    [SerializeField] private Image cooldownImage;

    public SpellData spellData { get; private set; }

    public void SetSpell(SpellData spellData) {
        this.spellData = spellData;
        UpdateData();
        Messenger.AddListener<SpellData>(Signals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpell);
        Messenger.AddListener<SpellData>(Signals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
        Messenger.AddListener<SpellData>(Signals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
    }
    private void UpdateData() {
        spellButtonText.text = spellData.name;
    }

    #region Listeners
    private void OnPlayerNoActiveSpell(SpellData spellData) {
        if(this.spellData == spellData) {
            if (spellToggle.isOn) {
                spellToggle.isOn = false;
            }
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
            
            SetInteractableState(spellData.CanPerformAbility());
        }
    }
    private void OnSpellCooldownFinished(SpellData spellData) {
        if (this.spellData == spellData) {
            SetCooldownState(spellData.isInCooldown);
            SetInteractableState(spellData.CanPerformAbility());
        }
    }
    #endregion
    public void OnToggleSpell(bool state) {
        PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
        if (state) {
            PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(spellData);
        }
    }

    public void OnHoverSpell() {
        //string hoverShow = spellData.description + "\n" + spellData.GetManaCostChargesCooldownStr();
        //UIManager.Instance.ShowSmallInfo(hoverShow);
        PlayerUI.Instance.OnHoverSpell(spellData, PlayerUI.Instance.spellListHoverPosition);
    }
    public void OnHoverOutSpell() {
        //UIManager.Instance.HideSmallInfo();
        PlayerUI.Instance.OnHoverOutSpell(spellData);
    }
    public void SetInteractableState(bool interactable) {
        spellToggle.interactable = interactable;
        cover.SetActive(interactable == false);
    }
    public void SetLockedState(bool state) {
        lockedImage.gameObject.SetActive(state);
    }
    public void SetCooldownState(bool state) {
        cooldownImage.gameObject.SetActive(state);
    }

    public override void Reset() {
        base.Reset();
        SetInteractableState(true);
        spellData = null;
        Messenger.RemoveListener<SpellData>(Signals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpell);
        Messenger.RemoveListener<SpellData>(Signals.SPELL_COOLDOWN_STARTED, OnSpellCooldownStarted);
        Messenger.RemoveListener<SpellData>(Signals.SPELL_COOLDOWN_FINISHED, OnSpellCooldownFinished);
    }
}
