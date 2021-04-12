using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EZObjectPools;
using Ruinarch.Custom_UI;
using Locations.Settlements;

public class FactionRegionFilterItem : PooledObject {
    public TextMeshProUGUI nameLbl;
    public RuinarchToggle toggle;

    public BaseSettlement village { get; private set; }

    public void SetVillage(BaseSettlement village) {
        this.village = village;
        nameLbl.text = village.name;
    }
    public void OnToggle(bool state) {
        if (state) {
            FactionInfoHubUI.Instance.FilterRegion(village);
        } else {
            FactionInfoHubUI.Instance.UnFilterRegion(village);
        }
    }

    #region Object Pool
    public override void Reset() {
        base.Reset();
        village = null;
    }
    #endregion
}