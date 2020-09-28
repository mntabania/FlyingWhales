using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EZObjectPools;
using Ruinarch.Custom_UI;

public class FactionRegionFilterItem : PooledObject {
    public TextMeshProUGUI nameLbl;
    public RuinarchToggle toggle;

    public Region region { get; private set; }

    public void SetRegion(Region region) {
        this.region = region;
        nameLbl.text = this.region.name;
    }
    public void OnToggle(bool state) {
        if (state) {
            FactionInfoHubUI.Instance.FilterRegion(region);
        } else {
            FactionInfoHubUI.Instance.UnFilterRegion(region);
        }
    }

    #region Object Pool
    public override void Reset() {
        base.Reset();
        region = null;
    }
    #endregion
}