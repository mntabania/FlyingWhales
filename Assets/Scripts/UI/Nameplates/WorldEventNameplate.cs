using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using EZObjectPools;

public class WorldEventNameplate : NameplateItem<Region> {

    #region Overrides
    public override void SetObject(Region r) {
        base.SetObject(r);
        subLbl.text = string.Empty;
    }
    #endregion
}
