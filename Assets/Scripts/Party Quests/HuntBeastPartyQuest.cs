using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class HuntBeastPartyQuest : PartyQuest {
    public LocationStructure targetStructure { get; private set; }

    #region getters
    public override IPartyQuestTarget target => targetStructure;
    public override System.Type serializedData => typeof(SaveDataHuntBeastPartyQuest);
    public override bool shouldAssignedPartyRetreatUponKnockoutOrKill => true;
    #endregion

    public HuntBeastPartyQuest() : base(PARTY_QUEST_TYPE.Hunt_Beast) {
        minimumPartySize = 3;
        relatedBehaviour = typeof(HuntBeastBehaviour);
    }
    public HuntBeastPartyQuest(SaveDataHuntBeastPartyQuest data) : base(data) {
    }

    #region Overrides
    public override void OnWaitTimeOver() {
        base.OnWaitTimeOver();
        if (targetStructure == null || targetStructure.hasBeenDestroyed || targetStructure.tiles.Count <= 0) {
            EndQuest("Structure is destroyed");
        } else {
            Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        }
    }
    public override IPartyTargetDestination GetTargetDestination() {
        return targetStructure.occupiedArea;
    }
    public override string GetPartyQuestTextInLog() {
        return "Hunt " + targetStructure.name;
    }
    public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState) {
        base.OnAssignedPartySwitchedState(fromState, toState);
        if (toState == PARTY_STATE.Working) {
            SetIsSuccessful(true);
        }
    }
    protected override void OnEndQuest() {
        base.OnEndQuest();
        if (Messenger.eventTable.ContainsKey(StructureSignals.STRUCTURE_DESTROYED)) {
            Messenger.RemoveListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
        }
    }
    #endregion

    #region General
    public void SetTargetStructure(LocationStructure structure) {
        if (targetStructure != structure) {
            targetStructure = structure;
        }
    }
    private void OnStructureDestroyed(LocationStructure structure) {
        if (targetStructure == structure) {
            EndQuest("Structure is destroyed");
        }
    }
    #endregion

    #region Loading
    public override void LoadReferences(SaveDataPartyQuest data) {
        base.LoadReferences(data);
        if (data is SaveDataHuntBeastPartyQuest subData) {
            if (!string.IsNullOrEmpty(subData.targetStructure)) {
                targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(subData.targetStructure);
            }
            if (isWaitTimeOver && assignedParty != null) {
                Messenger.AddListener<LocationStructure>(StructureSignals.STRUCTURE_DESTROYED, OnStructureDestroyed);
            }
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataHuntBeastPartyQuest : SaveDataPartyQuest {
    public string targetStructure;

    #region Overrides
    public override void Save(PartyQuest data) {
        base.Save(data);
        if (data is HuntBeastPartyQuest subData) {
            if (subData.targetStructure != null) {
                targetStructure = subData.targetStructure.persistentID;
            }
        }
    }
    #endregion
}