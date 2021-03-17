using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class DemonDefendPartyQuest : PartyQuest {

    public LocationStructure targetStructure { get; private set; }
    public Area targetArea { get; private set; }

    #region getters
    public override IPartyQuestTarget target => targetStructure;
    public override System.Type serializedData => typeof(SaveDataDemonDefendPartyQuest);
    #endregion

    public DemonDefendPartyQuest() : base(PARTY_QUEST_TYPE.Demon_Defend) {
        minimumPartySize = 3;
        relatedBehaviour = typeof(DemonDefendBehaviour);
    }
    public DemonDefendPartyQuest(SaveDataDemonDefendPartyQuest data) : base(data) {
    }

    #region Overrides
    public override IPartyTargetDestination GetTargetDestination() {
        return targetStructure;
    }
    public override string GetPartyQuestTextInLog() {
        return "Demon Defend " + targetStructure.name;
    }
    #endregion

    #region General
    public void SetTargetStructure(LocationStructure p_structure) {
        targetStructure = p_structure;
        if(targetStructure != null) {
            targetArea = targetStructure.occupiedArea;
        } else {
            targetArea = null;
        }
    }
    #endregion

    #region Loading
    public override void LoadReferences(SaveDataPartyQuest data) {
        base.LoadReferences(data);
        if (data is SaveDataDemonDefendPartyQuest subData) {
            if (!string.IsNullOrEmpty(subData.targetStructure)) {
                targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(subData.targetStructure);
            }
            if (!string.IsNullOrEmpty(subData.targetArea)) {
                targetArea = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(subData.targetArea);
            }
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataDemonDefendPartyQuest : SaveDataPartyQuest {
    public string targetStructure;
    public string targetArea;

    #region Overrides
    public override void Save(PartyQuest data) {
        base.Save(data);
        if (data is DemonDefendPartyQuest subData) {
            if (subData.targetStructure != null) {
                targetStructure = subData.targetStructure.persistentID;
            }
            if (subData.targetArea != null) {
                targetArea = subData.targetArea.persistentID;
            }
        }
    }
    #endregion
}