using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Locations.Settlements;

public class RaidPartyQuest : PartyQuest {

    public BaseSettlement targetSettlement { get; private set; }
    //public HexTile waitingArea { get; private set; }
    public bool isRaiding { get; private set; }

    #region getters
    public override IPartyQuestTarget target => targetSettlement;
    //public override HexTile waitingHexArea => waitingArea;
    public override System.Type serializedData => typeof(SaveDataRaidPartyQuest);
    public override bool shouldAssignedPartyRetreatUponKnockoutOrKill => true;
    #endregion

    public RaidPartyQuest() : base(PARTY_QUEST_TYPE.Raid) {
        minimumPartySize = 3;
        //waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30);
        relatedBehaviour = typeof(RaidBehaviour);
        //jobQueueOwnerType = JOB_OWNER.FACTION;
    }
    public RaidPartyQuest(SaveDataRaidPartyQuest data) : base(data) {
        isRaiding = data.isRaiding;
    }

    #region Overrides
    public override IPartyTargetDestination GetTargetDestination() {
        return targetSettlement;
    }
    public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState) {
        base.OnAssignedPartySwitchedState(fromState, toState);
        if(toState == PARTY_STATE.Working) {
            SetIsSuccessful(true);
            StartRaidTimer();
            Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
        } else if (fromState == PARTY_STATE.Working) {
            Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
        }
    }
    public override string GetPartyQuestTextInLog() {
        return "Raid " + targetSettlement.name;
    }
    #endregion

    #region General
    private void OnCharacterCanNoLongerMove(Character character) {
        //This is so that when a resident of the settlement being raided can no longer move, the raiders will evaluate the character again, so they can kidnap them
        if(assignedParty != null && assignedParty.isActive && assignedParty.currentQuest == this) {
            if (character.homeSettlement == targetSettlement) {
                if (character.marker) {
                    for (int i = 0; i < character.marker.inVisionCharacters.Count; i++) {
                        Character inVision = character.marker.inVisionCharacters[i];
                        if (inVision.marker) {
                            if (inVision.partyComponent.isActiveMember) {
                                if (inVision.partyComponent.currentParty.currentQuest == this) {
                                    inVision.marker.AddPOIAsInVisionRange(character);
                                }
                            }
                        }
                    }
                }
            }
        } else {
            Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
        }
    }
    private void ProcessRaidOrDisbandment() {
        if (assignedParty != null && assignedParty.isActive && assignedParty.currentQuest == this) {
            assignedParty.GoBackHomeAndEndQuest();
        }
        //if (!HasAliveResidentInsideSettlementThatIsHostileWith(assignedParty.partySettlement.owner, targetSettlement)) {
        //    assignedParty.GoBackHomeAndEndQuest();
        //} else {
        //    StartRaidTimer();
        //}
    }
    public void SetTargetSettlement(BaseSettlement settlement) {
        if(targetSettlement != settlement) {
            targetSettlement = settlement;
            //if (targetSettlement != null) {
            //    SetWaitingArea();
            //}
        }
    }
    //private void SetWaitingArea() {
    //    waitingArea = targetSettlement.GetAPlainAdjacentHextile();
    //}
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
            ProcessRaidOrDisbandment();
        }
    }
    #endregion

    #region Loading
    public override void LoadReferences(SaveDataPartyQuest data) {
        base.LoadReferences(data);
        if (data is SaveDataRaidPartyQuest subData) {
            if (!string.IsNullOrEmpty(subData.targetSettlement)) {
                targetSettlement = DatabaseManager.Instance.settlementDatabase.GetSettlementByPersistentID(subData.targetSettlement);
            }
            if(assignedParty != null && assignedParty.isActive && assignedParty.partyState == PARTY_STATE.Working) {
                Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterCanNoLongerMove);
            }
            //if (!string.IsNullOrEmpty(subData.waitingArea)) {
            //    waitingArea = DatabaseManager.Instance.hexTileDatabase.GetHextileByPersistentID(subData.waitingArea);
            //}
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataRaidPartyQuest : SaveDataPartyQuest {
    public string targetSettlement;
    //public string waitingArea;
    public bool isRaiding;

    #region Overrides
    public override void Save(PartyQuest data) {
        base.Save(data);
        if (data is RaidPartyQuest subData) {
            isRaiding = subData.isRaiding;

            if (subData.targetSettlement != null) {
                targetSettlement = subData.targetSettlement.persistentID;
            }
            //if (subData.waitingArea != null) {
            //    waitingArea = subData.waitingArea.persistentID;
            //}
        }
    }
    #endregion
}