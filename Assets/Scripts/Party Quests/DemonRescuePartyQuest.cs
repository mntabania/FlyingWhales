using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using UtilityScripts;

public class DemonRescuePartyQuest : PartyQuest, IRescuePartyQuest {
    public Character targetCharacter { get; private set; }
    public DemonicStructure targetDemonicStructure { get; private set; }
    public List<LocationGridTile> targetDemonicStructureTiles { get; private set; }
    public bool isReleasing { get; private set; }
    //public bool isSearching { get; private set; }

    #region getters
    public override IPartyQuestTarget target => targetCharacter;
    public override System.Type serializedData => typeof(SaveDataDemonRescuePartyQuest);
    public override bool waitingToWorkingStateImmediately => true;
    public override bool shouldAssignedPartyRetreatUponKnockoutOrKill => true;
    #endregion

    public DemonRescuePartyQuest() : base(PARTY_QUEST_TYPE.Demon_Rescue) {
        minimumPartySize = 1;
        //waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30);
        relatedBehaviour = typeof(DemonRescueBehaviour);
        //jobQueueOwnerType = JOB_OWNER.FACTION;
        targetDemonicStructureTiles = new List<LocationGridTile>();
    }
    public DemonRescuePartyQuest(SaveDataDemonRescuePartyQuest data) : base(data) {
        targetDemonicStructureTiles = new List<LocationGridTile>();
        isReleasing = data.isReleasing;
        //isSearching = data.isSearching;
    }

    #region Overrides
    //public override void OnAcceptQuest(Party partyThatAcceptedQuest) {
    //    base.OnAcceptQuest(partyThatAcceptedQuest);
    //    Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterNoLongerPerform);
    //    Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterNoLongerMove);
    //}
    //public override void OnAcceptQuestFromSaveData(Party partyThatAcceptedQuest) {
    //    base.OnAcceptQuestFromSaveData(partyThatAcceptedQuest);
    //    Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterNoLongerPerform);
    //    Messenger.AddListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterNoLongerMove);
    //}
    public override IPartyTargetDestination GetTargetDestination() {
        if (!targetDemonicStructure.hasBeenDestroyed) {
            return targetDemonicStructure;
        } else if (targetCharacter.currentStructure != null && targetCharacter.currentStructure.structureType != STRUCTURE_TYPE.WILDERNESS) {
            return targetCharacter.currentStructure;
        } else if (targetCharacter.gridTileLocation != null) {
            return targetCharacter.areaLocation;
        }
        return base.GetTargetDestination();
    }
    public override string GetPartyQuestTextInLog() {
        return "Rescue " + targetCharacter.name;
    }
    protected override void OnEndQuest() {
        base.OnEndQuest();
        //Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_PERFORM, OnCharacterNoLongerPerform);
        //Messenger.RemoveListener<Character>(CharacterSignals.CHARACTER_CAN_NO_LONGER_MOVE, OnCharacterNoLongerMove);
        RemoveAllCombatToDemonicStructure();
    }
    //public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState) {
    //    base.OnAssignedPartySwitchedState(fromState, toState);
    //    if (toState == PARTY_STATE.Working) {
    //        StartSearchTimer();
    //    }
    //}
    #endregion

    #region General
    //private void ProcessDisbandment() {
    //    if (isReleasing) {
    //        StartSearchTimer();
    //        return;
    //    }
    //    if(assignedParty != null && assignedParty.isActive && assignedParty.currentQuest == this) {
    //        assignedParty.GoBackHomeAndEndQuest();
    //    }
    //}
    public void SetTargetCharacter(Character character) {
        targetCharacter = character;
    }
    public void SetTargetDemonicStructure(DemonicStructure p_targetStructure) {
        targetDemonicStructure = p_targetStructure;
        UpdateDemonicStructureTiles();
    }
    public void SetIsReleasing(bool state) {
        if (isReleasing != state) {
            isReleasing = state;
            if (isReleasing) {
                RemoveAllCombatToDemonicStructure();
            }
        }
    }
    private void UpdateDemonicStructureTiles() {
        targetDemonicStructureTiles.Clear();
        if (targetDemonicStructure != null) {
            for (int i = 0; i < targetDemonicStructure.tiles.Count; i++) {
                targetDemonicStructureTiles.Add(targetDemonicStructure.tiles.ElementAt(i));
            }
        }
    }
    private void RemoveAllCombatToDemonicStructure() {
        if (assignedParty != null) {
            for (int i = 0; i < assignedParty.membersThatJoinedQuest.Count; i++) {
                Character member = assignedParty.membersThatJoinedQuest[i];
                if (member.combatComponent.isInCombat) {
                    bool hasRemovedHostile = false;
                    for (int j = 0; j < member.combatComponent.hostilesInRange.Count; j++) {
                        IPointOfInterest hostile = member.combatComponent.hostilesInRange[j];
                        if (hostile is TileObject obj && obj.isDamageContributorToStructure && obj.structureLocation == targetDemonicStructure) {
                            if (member.combatComponent.RemoveHostileInRange(hostile, false)) {
                                hasRemovedHostile = true;
                                j--;
                            }
                        }
                    }
                    if (hasRemovedHostile) {
                        member.combatComponent.SetWillProcessCombat(true);
                    }
                }
            }
        }
    }
    #endregion

    //#region Rescue Timer
    //private void StartSearchTimer() {
    //    if (!isSearching) {
    //        isSearching = true;
    //        GameDate dueDate = GameManager.Instance.Today();
    //        dueDate.AddTicks(GameManager.Instance.GetTicksBasedOnHour(3));
    //        SchedulingManager.Instance.AddEntry(dueDate, DoneSearching, this);
    //    }
    //}
    //private void DoneSearching() {
    //    isSearching = false;
    //    ProcessDisbandment();
    //}
    //#endregion

    #region Listeners
    //private void OnCharacterArrivedAtStructure(Character character, LocationStructure structure) {
    //    if (targetCharacter.currentStructure == structure) {
    //        if (IsMember(character)) {
    //            StartSearchTimer();
    //        }
    //    }
    //}
    private void OnCharacterNoLongerPerform(Character character) {
        if (character.limiterComponent.canMove) {
            //If character can still move even if he/she cannot perform, do not end quest
            //In order for the quest to be ended, character must be both cannot perform and move
            //The reason is so the quest will not end if the character only sleeps or rests
            return;
        }
        if (GameUtilities.RollChance(15)) {
            if (assignedParty != null && assignedParty.membersThatJoinedQuest.Contains(character)) {
                EndQuest(character.name + " is incapacitated");
                return;
            }
        }
        if (assignedParty != null) {
            if (assignedParty.DidMemberJoinQuest(character) && !assignedParty.HasActiveMemberThatJoinedQuest()) {
                EndQuest("Members are incapacitated");
            }
        }
    }
    //private void OnCharacterNoLongerMove(Character character) {
    //    if (character.limiterComponent.canPerform) {
    //        //If character can still perform even if he/she cannot move, do not end quest
    //        //In order for the quest to be ended, character must be both cannot perform and move
    //        //The reason is so the quest will not end if the character only sleeps or rests
    //        return;
    //    }
    //    if (GameUtilities.RollChance(15)) {
    //        if (assignedParty != null && assignedParty.membersThatJoinedQuest.Contains(character)) {
    //            EndQuest(character.name + " is incapacitated");
    //            return;
    //        }
    //    }
    //    if (assignedParty != null) {
    //        if (assignedParty.DidMemberJoinQuest(character) && !assignedParty.HasActiveMemberThatJoinedQuest()) {
    //            EndQuest("Members are incapacitated");
    //        }
    //    }
    //}
    //public override void OnCharacterDeath(Character p_character) {
    //    base.OnCharacterDeath(p_character);
    //    if (GameUtilities.RollChance(25)) {
    //        if (assignedParty != null && assignedParty.membersThatJoinedQuest.Contains(p_character)) {
    //            EndQuest(p_character.name + " died");
    //        }
    //    }
    //}
    #endregion

    #region Loading
    public override void LoadReferences(SaveDataPartyQuest data) {
        base.LoadReferences(data);
        if (data is SaveDataDemonRescuePartyQuest subData) {
            if (!string.IsNullOrEmpty(subData.targetCharacter)) {
                targetCharacter = CharacterManager.Instance.GetCharacterByPersistentID(subData.targetCharacter);
            }
            if (!string.IsNullOrEmpty(subData.targetDemonicStructure)) {
                targetDemonicStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(subData.targetDemonicStructure) as DemonicStructure;
                UpdateDemonicStructureTiles();
            }
            //if (isWaitTimeOver && !isDisbanded) {
            //    Messenger.AddListener<Character, LocationStructure>(Signals.CHARACTER_ARRIVED_AT_STRUCTURE, OnCharacterArrivedAtStructure);
            //}
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataDemonRescuePartyQuest : SaveDataPartyQuest {
    public string targetCharacter;
    public string targetDemonicStructure;
    public bool isReleasing;
    //public bool isSearching;

    #region Overrides
    public override void Save(PartyQuest data) {
        base.Save(data);
        if (data is DemonRescuePartyQuest subData) {
            isReleasing = subData.isReleasing;
            targetDemonicStructure = subData.targetDemonicStructure?.persistentID;
            //isSearching = subData.isSearching;

            if (subData.targetCharacter != null) {
                targetCharacter = subData.targetCharacter.persistentID;
            }
        }
    }
    #endregion
}