using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UtilityScripts;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class SettlementClassComponent : NPCSettlementComponent {

    public SettlementClassComponent() {
    }
    public SettlementClassComponent(SaveDataSettlementClassComponent data) {

    }

    #region Loading
    public void LoadReferences(SaveDataSettlementClassComponent data) {
    }
    #endregion

}

public class SaveDataSettlementClassComponent : SaveData<SettlementClassComponent> {

#region Overrides
    public override void Save(SettlementClassComponent data) {
    }

    public override SettlementClassComponent Load() {
        SettlementClassComponent component = new SettlementClassComponent(this);
        return component;
    }
#endregion
}
