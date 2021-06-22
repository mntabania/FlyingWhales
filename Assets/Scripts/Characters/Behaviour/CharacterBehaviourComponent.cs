﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using UtilityScripts;

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
    public bool StopsBehaviourLoop() {
        return HasAttribute(BEHAVIOUR_COMPONENT_ATTRIBUTE.STOPS_BEHAVIOUR_LOOP);
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
        List<Character> positiveRelatables = RuinarchListPool<Character>.Claim();
        actor.relationshipContainer.PopulateFriendCharacters(positiveRelatables);
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
        RuinarchListPool<Character>.Release(positiveRelatables);
        return weights;
    }
    #endregion

    #region Party
    //Returns true or false if the job is produced
    protected bool DoPartyJobsInPartyJobBoard(Character p_character, Party p_party, ref JobQueueItem producedJob) {
        if (p_character.limiterComponent.canTakeJobs) {
            JobQueueItem jobToAssign = p_party.jobBoard.GetFirstJobBasedOnVision(p_character);
            if (jobToAssign != null) {
                producedJob = jobToAssign;
                return true;
            } else {
                jobToAssign = p_party.jobBoard.GetFirstUnassignedJobToCharacterJob(p_character);
                if (jobToAssign != null) {
                    producedJob = jobToAssign;
                    return true;
                }
            }
        }
        return false;
    }
    #endregion
}
