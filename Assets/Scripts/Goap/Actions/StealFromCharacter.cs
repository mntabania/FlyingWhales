﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StealFromCharacter : GoapAction {

    private SpecialToken _targetItem;
    private Character _targetCharacter;

    public StealFromCharacter(Character actor, IPointOfInterest poiTarget) : base(INTERACTION_TYPE.STEAL_CHARACTER, INTERACTION_ALIGNMENT.EVIL, actor, poiTarget) {
        //validTimeOfDays = new TIME_IN_WORDS[] {
        //    TIME_IN_WORDS.EARLY_NIGHT,
        //    TIME_IN_WORDS.LATE_NIGHT,
        //    TIME_IN_WORDS.AFTER_MIDNIGHT,
        //};
        actionIconString = GoapActionStateDB.Hostile_Icon;
        _targetCharacter = poiTarget as Character;
    }

    #region Overrides
    protected override void ConstructRequirement() {
        _requirementAction = Requirement;
    }
    protected override void ConstructPreconditionsAndEffects() {
        if (actor.GetTrait("Kleptomaniac") != null) {
            AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, targetPOI = actor });
        }
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.FULLNESS_RECOVERY, targetPOI = actor });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TIREDNESS_RECOVERY, targetPOI = actor });
    }
    public override void PerformActualAction() {
        if (actor.gridTileLocation == poiTarget.gridTileLocation || actor.gridTileLocation.IsNeighbour(poiTarget.gridTileLocation)) {
            if (_targetCharacter.isHoldingItem) {
                SetState("Steal Success");
            } else {
                SetState("Steal Fail");
            }
        } else {
            SetState("Target Missing");
        }
        base.PerformActualAction();
    }
    protected override int GetCost() {
        if (actor.GetTrait("Kleptomaniac") != null) {
            return 4;
        }
        return 12;
    }
    #endregion

    #region Requirements
    protected bool Requirement() {
        if(poiTarget != actor) {
            return true;
            //if (poiTarget.factionOwner.id != actor.faction.id) {
            //    return true;
            //} else if (actor.faction.id == FactionManager.Instance.neutralFaction.id) {
            //    return true;
            //}
        }
        return false;
    }
    #endregion

    #region State Effects
    private void PreStealSuccess() {
        _targetItem = _targetCharacter.items[UnityEngine.Random.Range(0, _targetCharacter.items.Count)];
        //**Note**: This is a Theft crime
        SetCommittedCrime(CRIME.THEFT);
        currentState.AddLogFiller(_targetItem, _targetItem.name, LOG_IDENTIFIER.ITEM_1);
        currentState.SetIntelReaction(State1Reactions);
    }
    private void AfterStealSuccess() {
        actor.ObtainTokenFrom(_targetCharacter, _targetItem, false);
        if (actor.GetTrait("Kleptomaniac") != null) {
            actor.AdjustHappiness(60);
        }
    }
    #endregion

    #region Intel Reactions
    private List<string> State1Reactions(Character recipient, Intel sharedIntel) {
        List<string> reactions = new List<string>();
        Character targetCharacter = poiTarget as Character;
        //Recipient and Target is the same:
        if (recipient == targetCharacter) {
            //- **Recipient Response Text**: "[Actor Name] stole from me? What a horrible person."
            reactions.Add(string.Format("{0} stole from me? What a horrible person.", actor.name));
            //- **Recipient Effect**: Remove Friend/Lover/Paramour relationship between Actor and Recipient.
            List<RelationshipTrait> traitsToRemove = recipient.GetAllRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE);
            CharacterManager.Instance.RemoveRelationshipBetween(recipient, actor, traitsToRemove);
            //Apply Crime System handling as if the Recipient witnessed Actor commit Theft.
            recipient.ReactToCrime(CRIME.THEFT, actor, null, false);
        }
        //Recipient and Actor have a positive relationship:
        else if (recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.POSITIVE)) {
            //- **Recipient Response Text**: "[Actor Name] may have committed theft but I know that [he/she] is a good person."
            reactions.Add(string.Format("{0} may have committed theft but I know that {1} is a good person.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
            //-**Recipient Effect * *: no effect
        }
        //Recipient and Actor have a negative relationship:
        else if (recipient.HasRelationshipOfEffectWith(actor, TRAIT_EFFECT.NEGATIVE)) {
            //- **Recipient Response Text**: "[Actor Name] committed theft!? Why am I not surprised."
            reactions.Add(string.Format("{0} committed theft!? Why am I not surprised.", actor.name));
            //-**Recipient Effect**: Apply Crime System handling as if the Recipient witnessed Actor commit Theft.
            recipient.ReactToCrime(CRIME.THEFT, actor, null, false);
        }
        //Recipient and Actor have no relationship but are from the same faction:
        else if (!recipient.HasRelationshipWith(actor) && recipient.faction == actor.faction) {
            //- **Recipient Response Text**: "[Actor Name] committed theft!? That's illegal."
            reactions.Add(string.Format("{0} committed theft!? That's illegal.", actor.name));
            //- **Recipient Effect**: Apply Crime System handling as if the Recipient witnessed Actor commit Theft.
            recipient.ReactToCrime(CRIME.THEFT, actor, null, false);
        }
        //Recipient and Actor is the same:
        else if (recipient == actor) {
            //- **Recipient Response Text**: "I know what I did."
            reactions.Add("I know what I did.");
            //-**Recipient Effect**: no effect
        }
        return reactions;
    }
    #endregion
}
