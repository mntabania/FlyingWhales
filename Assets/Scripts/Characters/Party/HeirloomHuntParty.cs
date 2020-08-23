using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;

public class HeirloomHuntParty : Party {

    public Heirloom targetHeirloom { get; private set; }
    public HexTile targetHex { get; private set; }
    public bool foundHeirloom { get; private set; }
    public Region regionToSearch { get; private set; }

    private bool isSearching;
    private int currentChance;

    #region getters
    public override IPartyTarget target => targetHeirloom;
    //public override HexTile waitingHexArea => targetHex;
    #endregion

    public HeirloomHuntParty() : base(PARTY_TYPE.Heirloom_Hunt) {
        minimumPartySize = 4;
        waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30);
        relatedBehaviour = typeof(HeirloomHuntBehaviour);
        jobQueueOwnerType = JOB_OWNER.FACTION;
    }

    #region Overrides
    public override bool IsAllowedToJoin(Character character) {
        return (character.characterClass.IsCombatant() && character.characterClass.identifier == "Normal") || character.characterClass.className == "Noble";
    }
    protected override void OnWaitTimeOver() {
        base.OnWaitTimeOver();
        currentChance = 100;
        ProcessSettingTargetHex();
        Messenger.AddListener<Character, HexTile>(Signals.CHARACTER_ENTERED_HEXTILE, OnCharacterEnteredHex);
        for (int i = 0; i < members.Count; i++) {
            Character member = members[i];
            member.traitContainer.AddTrait(member, "Travelling");
        }
    }
    protected override void OnAddMember(Character member) {
        base.OnAddMember(member);
        member.movementComponent.SetEnableDigging(true);
    }
    protected override void OnRemoveMember(Character member) {
        base.OnRemoveMember(member);
        member.movementComponent.SetEnableDigging(false);
        member.traitContainer.RemoveTrait(member, "Travelling");
    }
    protected override void OnRemoveMemberOnDisband(Character member) {
        base.OnRemoveMemberOnDisband(member);
        member.movementComponent.SetEnableDigging(false);
        member.traitContainer.RemoveTrait(member, "Travelling");
    }
    protected override void OnDisbandParty() {
        base.OnDisbandParty();
        if (Messenger.eventTable.ContainsKey(Signals.CHARACTER_ARRIVED_AT_STRUCTURE)) {
            Messenger.RemoveListener<Character, HexTile>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterEnteredHex);
        }
    }
    #endregion

    #region General
    private void ProcessFoundHeirloom() {
        if(foundHeirloom) {

        } else {
            //if(targetHeirloom.isBeingCarriedBy != null || !targetHeirloom.gridTileLocation.collectionOwner.isPartOfParentRegionMap
            //    || targetHeirloom.gridTileLocation.collectionOwner.partOfHextile.hexTileOwner.biomeType != BIOMES.DESERT) {
            //    DisbandParty();
            //} else {
                if (UnityEngine.Random.Range(0, 100) < currentChance) {
                    ProcessSettingTargetHex();
                } else {
                    DisbandParty();
                }
            //}
        }
    }
    public void SetTargetHeirloom(Heirloom heirloom) {
        targetHeirloom = heirloom;
    }
    public void SetRegionToSearch(Region region) {
        regionToSearch = region;
    }
    public void SetFoundHeirloom(bool state) {
        foundHeirloom = state;
        if (foundHeirloom) {
            currentChance = 100;
        }
    }
    private void ProcessSettingTargetHex() {
        HexTile chosenHex = regionToSearch.GetRandomHexThatMeetCriteria(h => h.elevationType != ELEVATION.WATER && h != targetHex);
        if (chosenHex == null) {
            DisbandParty();
        } else {
            targetHex = chosenHex;
        }
    }
    public Character GetMemberCarryingHeirloom() {
        for (int i = 0; i < members.Count; i++) {
            Character character = members[i];
            if (character.HasItem("Heirloom")) {
                return character;
            }
        }
        return null;
    }
    #endregion

    #region Timer
    private void StartSearchTimer() {
        if (!isSearching) {
            isSearching = true;
            GameDate dueDate = GameManager.Instance.Today();
            dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(2));
            SchedulingManager.Instance.AddEntry(dueDate, DoneSearching, this);
        }
    }
    private void DoneSearching() {
        if (isSearching) {
            isSearching = false;
            currentChance -= 25;
            ProcessFoundHeirloom();
        }
    }
    #endregion

    #region Listeners
    private void OnCharacterEnteredHex(Character character, HexTile hex) {
        if (targetHex == hex) {
            if (IsMember(character)) {
                StartSearchTimer();
            }
        }
    }
    #endregion
}
