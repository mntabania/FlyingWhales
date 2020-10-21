using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveDataFactionRelationship : SaveData<FactionRelationship> {
    public string faction1ID;
    public string faction2ID;

    public int relationshipStatInt;

    #region Overrides
    public override void Save(FactionRelationship data) {
        faction1ID = data.faction1.persistentID;
        faction2ID = data.faction2.persistentID;
        relationshipStatInt = data.relationshipStatInt;
    }
    public override FactionRelationship Load() {
        Faction faction1 = FactionManager.Instance.GetFactionByPersistentID(faction1ID);
        Faction faction2 = FactionManager.Instance.GetFactionByPersistentID(faction2ID);
        FactionRelationship newRel = new FactionRelationship(faction1, faction2, relationshipStatInt);
        return newRel;
    }
    #endregion
}
