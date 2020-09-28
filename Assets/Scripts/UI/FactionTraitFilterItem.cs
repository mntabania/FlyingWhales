using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EZObjectPools;
using Ruinarch.Custom_UI;

public class FactionTraitFilterItem : PooledObject {
    public TextMeshProUGUI nameLbl;
    public RuinarchToggle toggle;

    public string traitName { get; private set; }

    public void SetTraitName(string traitName) {
        this.traitName = traitName;
        nameLbl.text = this.traitName;
    }
    public void OnToggle(bool state) {
        if (state) {
            FactionInfoHubUI.Instance.FilterTrait(traitName);
        } else {
            FactionInfoHubUI.Instance.UnFilterTrait(traitName);
        }
    }

    #region Object Pool
    public override void Reset() {
        base.Reset();
        traitName = null;
    }
    #endregion
}
