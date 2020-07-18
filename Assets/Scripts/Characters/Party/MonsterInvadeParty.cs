using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class MonsterInvadeParty : Party {

    public LocationStructure targetStructure { get; private set; }
    public HexTile targetHex { get; private set; }

    private bool isInvading;

    #region getters
    public override IPartyTarget target => targetStructure;
    #endregion

    public MonsterInvadeParty() : base(PARTY_TYPE.Monster_Invade) {
        minimumPartySize = 3;
        waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30);
        relatedBehaviour = typeof(MonsterInvadeBehaviour);
        jobQueueOwnerType = JOB_OWNER.FACTION;
    }

    #region Overrides
    public override bool IsAllowedToJoin(Character character) {
        return character.race == leader.race && character.gridTileLocation.collectionOwner.isPartOfParentRegionMap && leader.gridTileLocation.collectionOwner.isPartOfParentRegionMap
            && character.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner == leader.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner;
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
        if (tile.settlementOnTile == target.targetSettlement) {
            if (IsMember(character)) {
                StartInvadeTimer();
            }
        }
    }
    #endregion

    #region Extermination Timer
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
}
