using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Inner_Maps;
using Inner_Maps.Location_Structures;
using Traits;
using UtilityScripts;

public class CounterattackPartyQuest : PartyQuest {

    public LocationStructure targetStructure { get; private set; }
    public int currentChanceToEndQuest { get; private set; }

    #region getters
    public override IPartyQuestTarget target => targetStructure;
    public override System.Type serializedData => typeof(SaveDataCounterattackPartyQuest);
    public override bool waitingToWorkingStateImmediately => true;
    public override bool shouldAssignedPartyRetreatUponKnockoutOrKill => true;
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
    public override void OnAcceptQuest(Party partyThatAcceptedQuest) {
        base.OnAcceptQuest(partyThatAcceptedQuest);
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.AddListener<LocationStructure, Character>(StructureSignals.STRUCTURE_DESTROYED_BY, OnStructureDestroyedBy);
    }
    public override void OnAcceptQuestFromSaveData(Party partyThatAcceptedQuest) {
        base.OnAcceptQuestFromSaveData(partyThatAcceptedQuest);
        Messenger.AddListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.AddListener<LocationStructure, Character>(StructureSignals.STRUCTURE_DESTROYED_BY, OnStructureDestroyedBy);
    }
    protected override void OnEndQuest() {
        base.OnEndQuest();
        RemoveAllCombatToDemonicStructure();
        Messenger.RemoveListener<Character, Trait>(CharacterSignals.CHARACTER_TRAIT_ADDED, OnCharacterGainedTrait);
        Messenger.RemoveListener<LocationStructure, Character>(StructureSignals.STRUCTURE_DESTROYED_BY, OnStructureDestroyedBy);
    }
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
    public override void OnAssignedPartySwitchedState(PARTY_STATE fromState, PARTY_STATE toState) {
        base.OnAssignedPartySwitchedState(fromState, toState);
        if (toState == PARTY_STATE.Working) {
            bool hasEndQuest = false;
            CultistBetrayalProcessing(ref hasEndQuest);
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
    private void RemoveAllCombatToDemonicStructure() {
        if (assignedParty != null) {
            for (int i = 0; i < assignedParty.membersThatJoinedQuest.Count; i++) {
                Character member = assignedParty.membersThatJoinedQuest[i];
                if (member.combatComponent.isInCombat) {
                    bool hasRemovedHostile = false;
                    for (int j = 0; j < member.combatComponent.hostilesInRange.Count; j++) {
                        IPointOfInterest hostile = member.combatComponent.hostilesInRange[j];
                        if (hostile is TileObject obj && obj.isDamageContributorToStructure && obj.structureLocation.structureType.IsPlayerStructure()) {
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

    #region Listeners
    private void OnCharacterGainedTrait(Character character, Trait trait) {
        if (assignedParty != null && trait.name == "Unconscious") {
            if(character.partyComponent.IsAMemberOfParty(assignedParty) && character.partyComponent.isMemberThatJoinedQuest) {
                AdjustChanceToEndQuest(20);
            }
        }
    }
    public override void OnCharacterDeath(Character p_character) {
        base.OnCharacterDeath(p_character);
        if (p_character.partyComponent.IsAMemberOfParty(assignedParty) && p_character.partyComponent.isMemberThatJoinedQuest) {
            AdjustChanceToEndQuest(20);
        } else if (p_character.faction != null && p_character.faction.isPlayerFaction && assignedParty != null) {
            Dead dead = p_character.traitContainer.GetTraitOrStatus<Dead>("Dead");
            if (dead != null && dead.responsibleCharacter != null) {
                if (dead.responsibleCharacter.partyComponent.IsAMemberOfParty(assignedParty)) {
                    if (dead.responsibleCharacter.partyComponent.isMemberThatJoinedQuest) {
                        AdjustChanceToEndQuest(3);
                    }
                }
            }
        }
    }
    private void OnStructureDestroyedBy(LocationStructure p_structure, Character p_responsibleCharacter) {
        if (assignedParty != null && p_structure.structureType.IsPlayerStructure()) {
            if (p_responsibleCharacter.partyComponent.IsAMemberOfParty(assignedParty) && p_responsibleCharacter.partyComponent.isMemberThatJoinedQuest) {
                AdjustChanceToEndQuest(30);
            }
        }
    }
    #endregion

    #region Chance to End Quest
    public void AdjustChanceToEndQuest(int amount) {
        currentChanceToEndQuest += amount;
#if DEBUG_LOG
        Debug.Log("CURRENT CHANCE TO END COUNTER ATTACK QUEST OF " + assignedParty.name + " IS " + currentChanceToEndQuest);
#endif
        if (GameUtilities.RollChance(currentChanceToEndQuest) && assignedParty != null) {
            EndQuest("Finished quest");
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