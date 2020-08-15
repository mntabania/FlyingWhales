using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataFactionRelationship : SaveData<FactionRelationship> {
    public int faction1ID;
    public int faction2ID;

    public int relationshipStatInt;

    #region Overrides
    public override void Save(FactionRelationship data) {
        faction1ID = data.faction1.id;
        faction2ID = data.faction2.id;
        relationshipStatInt = data.relationshipStatInt;
    }
    public override FactionRelationship Load() {
        Faction faction1 = FactionManager.Instance.GetFactionBasedOnID(faction1ID);
        Faction faction2 = FactionManager.Instance.GetFactionBasedOnID(faction2ID);
        FactionRelationship newRel = new FactionRelationship(faction1, faction2);
        newRel.AdjustRelationshipStatus(relationshipStatInt);
        return newRel;
    }
    #endregion
}
