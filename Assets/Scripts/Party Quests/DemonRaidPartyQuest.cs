using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class DemonRaidPartyQuest : PartyQuest {

    public BaseSettlement targetSettlement { get; private set; }
    public bool isRaiding { get; private set; }

    #region getters
    public override IPartyQuestTarget target => targetSettlement;
    public override System.Type serializedData => typeof(SaveDataDemonRaidPartyQuest);
    #endregion

    public DemonRaidPartyQuest() : base(PARTY_QUEST_TYPE.Demon_Raid) {
        minimumPartySize = 3;
        relatedBehaviour = typeof(DemonRaidBehaviour);
    }
    public DemonRaidPartyQuest(SaveDataDemonRaidPartyQuest data) : base(data) {
        isRaiding = data.isRaiding;
    }

    #region Overrides
    public override IPartyTargetDestination GetTargetDestination() {
        return targetSettlement;
    }
    public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState) {
        base.OnAssignedPartySwitchedState(fromState, toState);
        if (toState == PARTY_STATE.Working) {
            StartRaidTimer();
            //Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
        } 
        //else if (fromState == PARTY_STATE.Working) {
        //    Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
        //}
    }
    public override string GetPartyQuestTextInLog() {
        return "Demon Raid " + targetSettlement.name;
    }
    #endregion

    #region General
    //private void OnCharacterCanNoLongerMove(Character character) {
    //    if(assignedParty != null && assignedParty.isActive && assignedParty.currentQuest == this) {
    //        if (character.homeSettlement == targetSettlement) {
    //            if (character.marker) {
    //                for (int i = 0; i < character.marker.inVisionCharacters.Count; i++) {
    //                    Character inVision = character.marker.inVisionCharacters[i];
    //                    if (inVision.marker) {
    //                        if (inVision.partyComponent.isActiveMember) {
    //                            if (inVision.partyComponent.currentParty.currentQuest == this) {
    //                                inVision.marker.AddPOIAsInVisionRange(character);
    //                            }
    //                        }
    //                    }
    //                }
    //            }
    //        }
    //    } else {
    //        Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
    //    }
    //}
    private void ProcessDisbandment() {
        if (assignedParty != null && assignedParty.isActive && assignedParty.currentQuest == this) {
            EndQuest("Finished quest");
        }
    }
    public void SetTargetSettlement(BaseSettlement settlement) {
        if(targetSettlement != settlement) {
            targetSettlement = settlement;
        }
    }
    private bool HasAliveResidentInsideSettlementThatIsHostileWith(Faction faction, BaseSettlement settlement) {
        for (int i = 0; i < settlement.residents.Count; i++) {
            Character resident = settlement.residents[i];
            if (!resident.isDead
                && !resident.isBeingSeized
                && resident.gridTileLocation != null
                && resident.gridTileLocation.IsPartOfSettlement(settlement)
                && (resident.faction == null || faction == null || faction.IsHostileWith(resident.faction))) {
                return true;
            }
        }
        return false;
    }
    #endregion

    #region Raid Timer
    private void StartRaidTimer() {
        if (!isRaiding) {
            isRaiding = true;
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(5));
            //dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30));
            SchedulingManager.Instance.AddEntry(dueDate, DoneRaidTimer, this);
        }
    }
    private void DoneRaidTimer() {
        if (isRaiding) {
            isRaiding = false;
            ProcessDisbandment();
        }
    }
    #endregion

    #region Loading
    public override void LoadReferences(SaveDataPartyQuest data) {
        base.LoadReferences(data);
        if (data is SaveDataDemonRaidPartyQuest subData) {
            if (!string.IsNullOrEmpty(subData.targetSettlement)) {
                targetSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(subData.targetSettlement);
            }
            //if(assignedParty != null && assignedParty.isActive && assignedParty.partyState == PARTY_STATE.Working) {
            //    Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
            //}
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataDemonRaidPartyQuest : SaveDataPartyQuest {
    public string targetSettlement;
    public bool isRaiding;

    #region Overrides
    public override void Save(PartyQuest data) {
        base.Save(data);
        if (data is DemonRaidPartyQuest subData) {
            isRaiding = subData.isRaiding;

            if (subData.targetSettlement != null) {
                targetSettlement = subData.targetSettlement.persistentID;
            }
        }
    }
    #endregion
}