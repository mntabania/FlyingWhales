using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;

public class CounterattackPartyQuest : PartyQuest {

    public LocationStructure targetStructure { get; private set; }

    #region getters
    public override IPartyQuestTarget target => targetStructure;
    public override System.Type serializedData => typeof(SaveDataCounterattackPartyQuest);
    #endregion

    public CounterattackPartyQuest() : base(PARTY_QUEST_TYPE.Counterattack) {
        minimumPartySize = 3;
        //waitTimeInTicks = GameManager.Instance.GetTicksBasedOnHour(1) + GameManager.Instance.GetTicksBasedOnMinutes(30);
        relatedBehaviour = typeof(AttackDemonicStructureBehaviour);
        //jobQueueOwnerType = JOB_OWNER.FACTION;
    }
    public CounterattackPartyQuest(SaveDataCounterattackPartyQuest data) : base(data) {
    }

    #region Overrides
    //public override bool IsAllowedToJoin(Character character) {
    //    return ((character.characterClass.IsCombatant() && character.characterClass.identifier == "Normal") || character.characterClass.className == "Noble") && !character.isAlliedWithPlayer;
    //}
    public override void OnWaitTimeOver() {
        base.OnWaitTimeOver();
        if (targetStructure is DemonicStructure demonicStructure) {
            Messenger.Broadcast(PartySignals.CHARACTERS_ATTACKING_DEMONIC_STRUCTURE, assignedParty.membersThatJoinedQuest, demonicStructure);
        }
        for (int i = 0; i < assignedParty.membersThatJoinedQuest.Count; i++) {
            Character member = assignedParty.membersThatJoinedQuest[i];
            member.traitContainer.AddTrait(member, "Fervor");
        }
        //for (int i = 0; i < members.Count; i++) {
        //    Character member = members[i];
        //    member.traitContainer.AddTrait(member, "Travelling");
        //}
    }
    public override IPartyTargetDestination GetTargetDestination() {
        return targetStructure;
    }
    public override string GetPartyQuestTextInLog() {
        return "Attack " + targetStructure.name;
    }
    //protected override void OnAddMember(Character member) {
    //    base.OnAddMember(member);
    //    member.movementComponent.SetEnableDigging(true);
    //    member.traitContainer.AddTrait(member, "Fervor");
    //}
    //protected override void OnRemoveMember(Character member) {
    //    base.OnRemoveMember(member);
    //    member.movementComponent.SetEnableDigging(false);
    //    member.traitContainer.RemoveTrait(member, "Fervor");
    //    member.traitContainer.RemoveTrait(member, "Travelling");
    //}
    //protected override void OnRemoveMemberOnDisband(Character member) {
    //    base.OnRemoveMemberOnDisband(member);
    //    member.movementComponent.SetEnableDigging(false);
    //    member.traitContainer.RemoveTrait(member, "Fervor");
    //    member.traitContainer.RemoveTrait(member, "Travelling");
    //}
    public override void OnRemoveMemberThatJoinedQuest(Character character) {
        base.OnRemoveMemberThatJoinedQuest(character);
        character.traitContainer.RemoveTrait(character, "Fervor");
    }
    //protected override void OnEndQuest() {
    //    base.OnEndQuest();
    //    for (int i = 0; i < assignedParty.membersThatJoinedQuest.Count; i++) {
    //        Character member = assignedParty.membersThatJoinedQuest[i];
    //        member.traitContainer.RemoveTrait(member, "Fervor");
    //    }
    //}
    public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState) {
        base.OnAssignedPartySwitchedState(fromState, toState);
        if (toState == PARTY_STATE.Working) {
            CultistBetrayalProcessing();
        }
    }
    #endregion

    #region General
    public void SetTargetStructure(LocationStructure structure) {
        if(targetStructure != structure) {
            targetStructure = structure;
            //if (targetStructure != null) {
            //    SetWaitingArea();
            //}
        }
    }
    //private void SetWaitingArea() {
    //    List<HexTile> hexes = targetStructure.occupiedHexTile.hexTileOwner.ValidTilesNoSettlementWithinRegion;
    //    if(hexes != null && hexes.Count > 0) {
    //        waitingArea = UtilityScripts.CollectionUtilities.GetRandomElement(hexes);
    //    } else {
    //        waitingArea = targetStructure.settlementLocation.GetAPlainAdjacentHextile();
    //    }
    //}
    private void CultistBetrayalProcessing() {
        if(assignedParty != null) {
            List<Character> membersAlliedWithPlayer = ObjectPoolManager.Instance.CreateNewCharactersList();
            for (int i = 0; i < assignedParty.membersThatJoinedQuest.Count; i++) {
                Character member = assignedParty.membersThatJoinedQuest[i];
                if (member.isAlliedWithPlayer) {
                    membersAlliedWithPlayer.Add(member);
                }
            }
            for (int i = 0; i < membersAlliedWithPlayer.Count; i++) {
                Character memberAlliedWithPlayer = membersAlliedWithPlayer[i];
                MembersAreBetrayedByThis(memberAlliedWithPlayer);
                memberAlliedWithPlayer.interruptComponent.TriggerInterrupt(INTERRUPT.Leave_Party, memberAlliedWithPlayer, "Betrayed party members");
            }
            ObjectPoolManager.Instance.ReturnCharactersListToPool(membersAlliedWithPlayer);
        }
    }
    private void MembersAreBetrayedByThis(Character character) {
        if (assignedParty != null) {
            for (int i = 0; i < assignedParty.membersThatJoinedQuest.Count; i++) {
                Character member = assignedParty.membersThatJoinedQuest[i];
                if (member != character && !member.isAlliedWithPlayer) {
                    Betrayed betrayed = member.traitContainer.GetTraitOrStatus<Betrayed>("Betrayed");
                    if(betrayed != null) {
                        betrayed.AddCharacterResponsibleForTrait(character);
                    } else {
                        member.traitContainer.AddTrait(member, "Betrayed", characterResponsible: character);
                    }
                }
            }
        }
    }
    #endregion

    #region Loading
    public override void LoadReferences(SaveDataPartyQuest data) {
        base.LoadReferences(data);
        if(data is SaveDataCounterattackPartyQuest subData) {
            if (!string.IsNullOrEmpty(subData.targetStructure)) {
                targetStructure = DatabaseManager.Instance.structureDatabase.GetStructureByPersistentID(subData.targetStructure);
            }
        }
    }
    #endregion
}

[System.Serializable]
public class SaveDataCounterattackPartyQuest : SaveDataPartyQuest {
    public string targetStructure;

    #region Overrides
    public override void Save(PartyQuest data) {
        base.Save(data);
        if(data is CounterattackPartyQuest subData) {
            if(subData.targetStructure != null) {
                targetStructure = subData.targetStructure.persistentID;
            }
        }
    }
    #endregion
}