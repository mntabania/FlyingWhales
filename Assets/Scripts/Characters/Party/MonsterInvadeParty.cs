using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class MonsterInvadeParty : Party {

    public LocationStructure targetStructure { get; private set; }
    public HexTile targetHex { get; private set; }

    public HexTile hexForJoining { get; private set; }

    public bool isInvading { get; private set; }

    #region getters
    public override IPartyTarget target => targetStructure;
    public override System.Type serializedData => typeof(SaveDataMonsterInvadeParty);
    #endregion

    public MonsterInvadeParty() : base(PARTY_TYPE.Monster_Invade) {
        minimumPartySize = 3;
        waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30);
        relatedBehaviour = typeof(MonsterInvadeBehaviour);
        jobQueueOwnerType = JOB_OWNER.FACTION;
    }
    public MonsterInvadeParty(SaveDataParty data) : base(data) {
        if (data is SaveDataMonsterInvadeParty subData) {
            isInvading = subData.isInvading;
        }
    }

    #region Overrides
    public override bool IsAllowedToJoin(Character character) {
        return character.race == leader.race && character.gridTileLocation.collectionOwner.isPartOfParentRegionMap && hexForJoining != null
            && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == hexForJoining;
    }
    protected override void OnWaitTimeOver() {
        base.OnWaitTimeOver();
        Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
    }
    //protected override void OnAddMember(Character member) {
    //    base.OnAddMember(member);
    //    member.movementComponent.SetEnableDigging(true);
    //}
    //protected override void OnRemoveMember(Character member) {
    //    base.OnRemoveMember(member);
    //    member.movementComponent.SetEnableDigging(false);
    //}
    protected override void OnDisbandParty() {
        base.OnDisbandParty();
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_ENTERED_HEXTILE)) {
            Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
        }
    }
    protected override void OnSetLeader() {
        base.OnSetLeader();
        if(leader != null) {
            if (leader.gridTileLocation.collectionOwner.isPartOfParentRegionMap) {
                hexForJoining = leader.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
            }
        }
    }
    #endregion

    #region General
    private void ProcessDisbandment() {
        DisbandParty();
    }
    public void SetTargetStructure(LocationStructure structure) {
        if (targetStructure != structure) {
            targetStructure = structure;
        }
    }
    public void SetTargetHex(HexTile hex) {
        if (targetHex != hex) {
            targetHex = hex;
        }
    }
    private void OnCharacterEnteredHexTile(Character character, HexTile tile) {
        bool isInTargetLocation = false;
        if(targetStructure != null) {
            isInTargetLocation = tile.settlementOnTile != null && targetStructure.settlementLocation == tile.settlementOnTile;
        } else if (targetHex != null) {
            isInTargetLocation = tile == targetHex;
        }
        if (isInTargetLocation) {
            if (IsMember(character)) {
                StartInvadeTimer();
            }
        }
    }
    #endregion

    #region Invade Timer
    private void StartInvadeTimer() {
        if (!isInvading) {
            isInvading = true;
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(3));
            SchedulingManager.Instance.AddEntry(dueDate, DoneInvadeTimer, this);
        }
    }
    private void DoneInvadeTimer() {
        if (isInvading) {
            isInvading = false;
            ProcessDisbandment();
        }
    }
    #endregion

    #region Loading
    public override void LoadReferences(SaveDataParty data) {
        base.LoadReferences(data);
        if (data is SaveDataMonsterInvadeParty subData) {
            if (!string.IsNullOrEmpty(subData.targetStructure)) {
                targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(subData.targetStructure);
            }
            if (!string.IsNullOrEmpty(subData.targetHex)) {
                targetHex = DatabaseManager.Instance.hexTileDatabase.GetHextileByPersistentID(subData.targetHex);
            }
            if (!string.IsNullOrEmpty(subData.hexForJoining)) {
                hexForJoining = DatabaseManager.Instance.hexTileDatabase.GetHextileByPersistentID(subData.hexForJoining);
            }
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataMonsterInvadeParty : SaveDataParty {
    public string targetStructure;
    public string targetHex;
    public string hexForJoining;
    public bool isInvading;

    #region Overrides
    public override void Save(Party data) {
        base.Save(data);
        if (data is MonsterInvadeParty subData) {
            isInvading = subData.isInvading;

            if (subData.targetStructure != null) {
                targetStructure = subData.targetStructure.persistentID;
            }
            if (subData.targetHex != null) {
                targetHex = subData.targetHex.persistentID;
            }
            if (subData.hexForJoining != null) {
                hexForJoining = subData.hexForJoining.persistentID;
            }
        }
    }
    #endregion
}