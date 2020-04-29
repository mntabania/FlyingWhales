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

    public SpellData spellData { get; private set; }

    public void SetSpell(SpellData spellData) {
        this.spellData = spellData;
        UpdateData();
        Messenger.AddListener<SpellData>(Signals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpell);
    }

    private void UpdateData() {
        spellButtonText.text = spellData.name;
    }
    private void OnPlayerNoActiveSpell(SpellData spellData) {
        if(this.spellData == spellData) {
            if (spellToggle.isOn) {
                spellToggle.isOn = false;
            }
        }
    }
    public void OnToggleSpell(bool state) {
        PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
        if (state) {
            PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(spellData);
        }
    }

    public void OnHoverSpell() {
        string hoverShow = spellData.description + "\n" + spellData.GetManaCostChargesCooldownStr();
        UIManager.Instance.ShowSmallInfo(hoverShow);
    }
    public void OnHoverOutSpell() {
        UIManager.Instance.HideSmallInfo();
    }
    public void SetInteractableState(bool interactable) {
        spellToggle.interactable = interactable;
        cover.SetActive(interactable == false);
    }

    public override void Reset() {
        base.Reset();
        SetInteractableState(true);
        spellData = null;
        Messenger.RemoveListener<SpellData>(Signals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpell);
    }
}
