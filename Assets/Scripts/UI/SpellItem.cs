using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZObjectPools;
using TMPro;
using UnityEngine.UI;

public class SpellItem : PooledObject {
    public TextMeshProUGUI spellButtonText;
    public Toggle spellToggle;

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
        //else {
        //    PlayerManager.Instance.player.SetCurrentlyActivePlayerSpell(null);
        //}
    }

    public void OnHoverSpell() {
        UIManager.Instance.ShowSmallInfo(spellData.description);
    }
    public void OnHoverOutSpell() {
        UIManager.Instance.HideSmallInfo();
    }

    public override void Reset() {
        base.Reset();
        spellData = null;
        Messenger.RemoveListener<SpellData>(Signals.PLAYER_NO_ACTIVE_SPELL, OnPlayerNoActiveSpell);
    }
}
