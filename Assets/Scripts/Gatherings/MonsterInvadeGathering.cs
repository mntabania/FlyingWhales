using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class MonsterInvadeGathering : Gathering {

    public LocationStructure targetStructure { get; private set; }
    public HexTile targetHex { get; private set; }

    public HexTile hexForJoining { get; private set; }

    public bool isInvading { get; private set; }

    #region getters
    public override IGatheringTarget target => targetStructure;
    public override System.Type serializedData => typeof(SaveDataMonsterInvadeGathering);
    #endregion

    public MonsterInvadeGathering() : base(GATHERING_TYPE.Monster_Invade) {
        minimumGatheringSize = 3;
        waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30);
        relatedBehaviour = typeof(MonsterInvadeBehaviour);
        jobQueueOwnerType = JOB_OWNER.FACTION;
    }
    public MonsterInvadeGathering(SaveDataMonsterInvadeGathering data) : base(data) {
        isInvading = data.isInvading;
    }

    #region Overrides
    public override bool IsAllowedToJoin(Character character) {
        return character.race == host.race && hexForJoining != null && character.hexTileLocation == hexForJoining;
    }
    protected override void OnWaitTimeOver() {
        base.OnWaitTimeOver();
        Messenger.AddListener<Character, HexTile>(CharacterSignals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
    }
    //protected override void OnAddMember(Character member) {
    //    base.OnAddMember(member);
    //    member.movementComponent.SetEnableDigging(true);
    //}
    //protected override void OnRemoveMember(Character member) {
    //    base.OnRemoveMember(member);
    //    member.movementComponent.SetEnableDigging(false);
    //}
    protected override void OnDisbandGathering() {
        base.OnDisbandGathering();
        if (Messenger.eventTable.ContainsKey(CharacterSignals.CHARACTER_ENTERED_HEXTILE)) {
            Messenger.RemoveListener<Character, HexTile>(CharacterSignals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
        }
    }
    protected override void OnSetHost() {
        base.OnSetHost();
        if (host != null) {
            hexForJoining = host.hexTileLocation;
        }
    }
    #endregion

    #region General
    private void ProcessDisbandment() {
        DisbandGathering();
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
        if (targetStructure != null) {
            isInTargetLocation = tile.settlementOnTile != null && targetStructure.settlementLocation == tile.settlementOnTile;
        } else if (targetHex != null) {
            isInTargetLocation = tile == targetHex;
        }
        if (isInTargetLocation) {
            if (IsAttendee(character)) {
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
    public override void LoadReferences(SaveDataGathering data) {
        base.LoadReferences(data);
        if (data is SaveDataMonsterInvadeGathering subData) {
            if (!string.IsNullOrEmpty(subData.targetStructure)) {
                targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(subData.targetStructure);
            }
            if (!string.IsNullOrEmpty(subData.targetHex)) {
                targetHex = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(subData.targetHex);
            }
            if (!string.IsNullOrEmpty(subData.hexForJoining)) {
                hexForJoining = DatabaseManager.Instance.areaDatabase.GetAreaByPersistentID(subData.hexForJoining);
            }
            if (isWaitTimeOver && !isDisbanded) {
                Messenger.AddListener<Character, HexTile>(CharacterSignals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHexTile);
            }
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataMonsterInvadeGathering : SaveDataGathering {
    public string targetStructure;
    public string targetHex;
    public string hexForJoining;
    public bool isInvading;

    #region Overrides
    public override void Save(Gathering data) {
        base.Save(data);
        if (data is MonsterInvadeGathering subData) {
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