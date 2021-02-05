﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;

public abstract class CharacterBehaviourComponent {

    private List<Character> _isDisabledFor;
    protected BEHAVIOUR_COMPONENT_ATTRIBUTE[] attributes;
    public int priority { get; protected set; }

    public abstract bool TryDoBehaviour(Character character, ref string log, out JobQueueItem producedJob);

    /// <summary>
    /// Contains stuff to do to character when they ADD this behaviour to their list of behaviours.
    /// </summary>
    /// <param name="character">The character concerned.</param>
    public virtual void OnAddBehaviourToCharacter(Character character) { }
    /// <summary>
    /// Contains stuff to do to character when they REMOVE this behaviour from their list of behaviours.
    /// </summary>
    /// <param name="character">The character concerned.</param>
    public virtual void OnRemoveBehaviourFromCharacter(Character character) { }

    #region Loading
    /// <summary>
    /// Contains stuff to do to character when they LOAD this behaviour to their list of behaviours.
    /// </summary>
    /// <param name="character">The character concerned.</param>
    public virtual void OnLoadBehaviourToCharacter(Character character) { }
    #endregion
    
    #region Enabling/Disabling
    private void DisableFor(Character character) {
        if (_isDisabledFor == null) { _isDisabledFor = new List<Character>(); }
        _isDisabledFor.Add(character);
    }
    private void EnableFor(Character character) {
        _isDisabledFor.Remove(character);
    }
    public bool IsDisabledFor(Character character) {
        if (_isDisabledFor != null) {
            return _isDisabledFor.Contains(character);
        }
        return false;
    }
    public bool CanDoBehaviour(Character character) {
        if(HasAttribute(BEHAVIOUR_COMPONENT_ATTRIBUTE.WITHIN_HOME_SETTLEMENT_ONLY)) { //character.specificLocation.region.area.areaMap - will be changed after specificLocation rework
            //if character is not at a npcSettlement map, and the current behaviour requires the character to be at a npcSettlement map, then character cannot do this behaviour
            //EDIT: New mechanic for INSIDE_SETTLEMENT_ONLY - now this attribute simply means "whenever the character is inside his/her home"
            if (!character.IsInHomeSettlement()) {
                return false;
            }
        }
        //else if (character.isAtHomeRegion && HasAttribute(BEHAVIOUR_COMPONENT_ATTRIBUTE.OUTSIDE_SETTLEMENT_ONLY)) {
        //    //if character is at a npcSettlement map, and the current behaviour requires the character to NOT be at a npcSettlement map, then character cannot do this behaviour
        //    //EDIT: New mechanic for OUTSIDE_SETTLEMENT_ONLY - now this attribute simply means "whenever the character is outside his/her home"
        //    return false;
        //}
        return true;
    }
    public bool WillContinueProcess() {
        return HasAttribute(BEHAVIOUR_COMPONENT_ATTRIBUTE.DO_NOT_SKIP_PROCESSING);
    }
    public void PostProcessAfterSuccessfulDoBehaviour(Character character) {
        if (HasAttribute(BEHAVIOUR_COMPONENT_ATTRIBUTE.ONCE_PER_DAY)) {
            DisableFor(character);

            //schedule enable for start of next day
            GameDate today = GameManager.Instance.Today();
            GameDate nextDay = today.AddDays(1);
            nextDay.SetTicks(1);
            SchedulingManager.Instance.AddEntry(nextDay, () => EnableFor(character), this);
        }
    }
    protected bool HasAttribute(params BEHAVIOUR_COMPONENT_ATTRIBUTE[] passedAttributes) {
        if(attributes != null) {
            for (int i = 0; i < attributes.Length; i++) {
                for (int j = 0; j < passedAttributes.Length; j++) {
                    if (attributes[i] == passedAttributes[j]) {
                        return true;
                    }
                }
            }
        }
        return false;
    }
    #endregion

    #region Utilities
    protected WeightedDictionary<Character> GetCharacterToVisitWeights(Character actor) {
        WeightedDictionary<Character> weights = new WeightedDictionary<Character>();
        List<Character> positiveRelatables = actor.relationshipContainer.GetFriendCharacters();
        for (int i = 0; i < positiveRelatables.Count; i++) {
            Character character = positiveRelatables[i];
            if (character.isDead || character.homeStructure == null || 
                character.movementComponent.HasPathToEvenIfDiffRegion(character.homeStructure.GetRandomTile()) == false ||
                (character.faction != null && actor.faction != null && character.faction.GetRelationshipWith(actor.faction)?.relationshipStatus == FACTION_RELATIONSHIP_STATUS.Hostile)) {
                continue; //skip
            }
            int weight = 10;
            if (character.homeSettlement == actor.homeSettlement) {
                weight += 100;
            }
            weights.AddElement(character, weight);
        }
        return weights;
    }
    protected bool CanCharacterBeRecruited(Character targetCharacter, Character recruiter) {
        if (recruiter.faction == null || targetCharacter.faction == recruiter.faction
            || targetCharacter.race == RACE.TRITON) {
            //Tritons cannot be tamed/recruited
            return false;
        }
        // if (targetCharacter.faction?.factionType.type == FACTION_TYPE.Undead) {
        //     return false;
        // }
        if (!targetCharacter.traitContainer.HasTrait("Restrained")) {
            return false;
        }
        if (targetCharacter.HasJobTargetingThis(JOB_TYPE.RECRUIT)) {
            return false;
        }
        if (!recruiter.faction.ideologyComponent.DoesCharacterFitCurrentIdeologies(targetCharacter)) {
            //Cannot recruit characters that does not fit faction ideologies
            return false;
        }
        if (recruiter.faction.IsCharacterBannedFromJoining(targetCharacter)) {
            //Cannot recruit banned characters
            return false;
        }
        Prisoner prisoner = targetCharacter.traitContainer.GetTraitOrStatus<Prisoner>("Prisoner");
        if (prisoner == null || !prisoner.IsFactionPrisonerOf(recruiter.faction)) {
            //Only recruit characters that are prisoners of the recruiters faction.
            //This was added because sometimes vampire lords will recruit their imprisoned blood sources
            return false;
        }
        return true;
    }
    #endregion

}
