using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EZObjectPools;
using TMPro;
using UnityEngine.UI;

public class SpellNameplateItem : PooledObject {
    public TextMeshProUGUI spellButtonText;
    public Toggle spellToggle;
    //Toggle Delegates
    private System.Action<SpellData, bool> onToggleNameplate;

    public SpellData spellData { get; private set; }

    public void SetSpell(SpellData spellData) {
        this.spellData = spellData;
        UpdateData();
    }
    public void SetToggleAction(System.Action<SpellData, bool> onToggle) {
        onToggleNameplate = onToggle;
    }

    private void UpdateData() {
        spellButtonText.text = spellData.name;
    }
    public void OnToggleSpell(bool state) {
        if(onToggleNameplate != null) {
            onToggleNameplate(spellData, state);
        }
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
        spellToggle.isOn = false;
    }
}
