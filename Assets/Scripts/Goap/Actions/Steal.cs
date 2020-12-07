﻿using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;

public class Steal : GoapAction {

    public Steal() : base(INTERACTION_TYPE.STEAL) {
        actionIconString = GoapActionStateDB.Steal_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.RATMAN };
        isNotificationAnIntel = true;
        logTags = new[] {LOG_TAG.Crimes};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAS_POI, GOAP_EFFECT_TARGET.ACTOR));
        AddPossibleExpectedEffectForTypeAndTargetMatching(new GoapEffectConditionTypeAndTargetType(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, GOAP_EFFECT_TARGET.ACTOR));

    }
    protected override List<GoapEffect> GetExpectedEffects(Character actor, IPointOfInterest target, OtherData[] otherData, out bool isOverridden) {
        List<GoapEffect> ee = ObjectPoolManager.Instance.CreateNewExpectedEffectsList();
        List<GoapEffect> baseEE = base.GetExpectedEffects(actor, target, otherData, out isOverridden);
        if(baseEE != null && baseEE.Count > 0) {
            ee.AddRange(baseEE);
        }
        TileObject item = target as TileObject;
        ee.Add(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_POI, conditionKey = item.name, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
        if (actor.traitContainer.HasTrait("Kleptomaniac")) {
            ee.Add(new GoapEffect(GOAP_EFFECT_CONDITION.HAPPINESS_RECOVERY, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
        }
        isOverridden = true;
        return ee;
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Steal Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}:";
        if (actor.traitContainer.HasTrait("Enslaved")) {
            if (target.gridTileLocation == null || !target.gridTileLocation.IsInHomeOf(actor)) {
                costLog += $" +2000(Slave, target is not in actor's home)";
                actor.logComponent.AppendCostLog(costLog);
                return 2000;
            }
        }
        int cost = UtilityScripts.Utilities.Rng.Next(300, 351);
        costLog += $" +{cost}(Initial)";
        if (actor.traitContainer.HasTrait("Kleptomaniac")) {
            cost = UtilityScripts.Utilities.Rng.Next(90, 151);
            costLog = " {cost}(Kleptomaniac)";
        } else {
            TileObject item = null;
            if(target is TileObject tileObject) {
                item = tileObject;
            }
            if(item?.characterOwner != null) {
                string opinionLabel = actor.relationshipContainer.GetOpinionLabel(item.characterOwner);
                if(actor.moodComponent.moodState == MOOD_STATE.Normal || opinionLabel == RelationshipManager.Acquaintance || 
                   opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                    cost += 2000;
                    costLog += " +2000(not Kleptomaniac, Friend/Close/Acquaintance)";
                } else if (actor.moodComponent.moodState == MOOD_STATE.Bad) {
                    cost += UtilityScripts.Utilities.Rng.Next(500, 601);
                } else if (actor.moodComponent.moodState == MOOD_STATE.Critical) {
                    cost += UtilityScripts.Utilities.Rng.Next(120, 201);
                }
            }
        }
        actor.logComponent.AppendCostLog(costLog);
        return cost;
    }
    public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is TileObject) {
            TileObject item = goapNode.poiTarget as TileObject;
            if (item.isBeingCarriedBy != null) {
                return item.isBeingCarriedBy; //make the actor follow the character that is carrying the item instead.
            }
        }
        return base.GetTargetToGoTo(goapNode);
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        IPointOfInterest poiTarget = node.poiTarget;
        TileObject token = poiTarget as TileObject;
        if (token.isBeingCarriedBy != null) {
            return token.isBeingCarriedBy.currentStructure;
        }
        return base.GetTargetStructure(node);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool isInvalid = false;
        //steal can never be invalid since requirement handle all cases of invalidity.
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(isInvalid, stateName);
        return goapActionInvalidity;
    }
    public override void PopulateReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateReactionsToActor(reactions, actor, target, witness, node, status);
        if (!witness.traitContainer.HasTrait("Cultist")) {
            reactions.Add(EMOTION.Disapproval);
            if (witness.relationshipContainer.IsFriendsWith(actor)) {
                reactions.Add(EMOTION.Disappointment);
                reactions.Add(EMOTION.Shock);
            }
        } else if (witness == target || (target is TileObject tileObject && tileObject.IsOwnedBy(witness))) {
            reactions.Add(EMOTION.Betrayal);
        }
    }
    public override void PopulateReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateReactionsOfTarget(reactions, actor, target, node, status);
        if (target is TileObject tileObject) {
            Character targetCharacter = tileObject.isBeingCarriedBy;
            if (targetCharacter != null) {
                reactions.Add(EMOTION.Disappointment);
                if (targetCharacter.traitContainer.HasTrait("Hothead") || UnityEngine.Random.Range(0, 100) < 35) {
                    reactions.Add(EMOTION.Anger);
                }
            }
        }
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        return CRIME_TYPE.Theft;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            TileObject item = poiTarget as TileObject;
            if (poiTarget.gridTileLocation != null) {
                return item.characterOwner != null && !item.IsOwnedBy(actor);
            } else {
                return item.isBeingCarriedBy != null && item.characterOwner != null && !item.IsOwnedBy(actor);
            }
        }
        return false;
    }
    #endregion

    #region State Effects
    //public void PreStealSuccess(ActualGoapNode goapNode) {
    //    //**Note**: This is a Theft crime
    //    //GoapActionState currentState = goapNode.action.states[goapNode.currentStateName];
    //    //goapNode.descriptionLog.AddToFillers(goapNode.targetStructure.location, goapNode.targetStructure.GetNameRelativeTo(goapNode.actor), LOG_IDENTIFIER.LANDMARK_1);
    //    //goapNode.descriptionLog.AddToFillers(goapNode.poiTarget as SpecialToken, goapNode.poiTarget.name, LOG_IDENTIFIER.TARGET_CHARACTER);
    //}
    public void AfterStealSuccess(ActualGoapNode goapNode) {
        goapNode.actor.PickUpItem(goapNode.poiTarget as TileObject);
        if(goapNode.actor.traitContainer.HasTrait("Kleptomaniac")) {
            goapNode.actor.needsComponent.AdjustHappiness(10);
        }
    }
    #endregion

    //#region Intel Reactions
    //private List<string> State1Reactions(Character recipient, Intel sharedIntel, SHARE_INTEL_STATUS status) {
    //    List<string> reactions = new List<string>();
    //    SpecialToken stolenItem = poiTarget as SpecialToken;
    //    //Recipient is the owner of the item:
    //    if (recipient == stolenItem.characterOwner) {
    //        //- **Recipient Response Text**: "[Actor Name] stole from me? What a horrible person."
    //        reactions.Add(string.Format("{0} stole from me? What a horrible person.", actor.name));
    //        //- **Recipient Effect**: Remove Friend/Lover/Paramour relationship between Actor and Recipient.Apply Crime System handling as if the Recipient witnessed Actor commit Theft.
    //        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //        List<RELATIONSHIP_TRAIT> traitsToRemove = recipient.relationshipContainer.GetRelationshipDataWith(actor).GetAllRelationshipOfEffect(RELATIONSHIP_EFFECT.POSITIVE);
    //        for (int i = 0; i < traitsToRemove.Count; i++) {
    //            RelationshipManager.Instance.RemoveRelationshipBetween(recipient, actor, traitsToRemove[i]);
    //        }
    //    }

    //    //Recipient and Actor is the same:
    //    else if (recipient == actor) {
    //        //- **Recipient Response Text**: "I know what I did."
    //        reactions.Add("I know what I did.");
    //        //-**Recipient Effect**: no effect
    //    }

    //    //Recipient and Actor have a positive relationship:
    //    else if (recipient.relationshipContainer.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.POSITIVE) {
    //        //- **Recipient Response Text**: "[Actor Name] may have committed theft but I know that [he/she] is a good person."
    //        reactions.Add(string.Format("{0} may have committed theft but I know that {1} is a good person.", actor.name, Utilities.GetPronounString(actor.gender, PRONOUN_TYPE.SUBJECTIVE, false)));
    //        //-**Recipient Effect**: no effect
    //    }
    //    //Recipient and Actor have a negative relationship:
    //    else if (recipient.relationshipContainer.GetRelationshipEffectWith(actor) == RELATIONSHIP_EFFECT.NEGATIVE) {
    //        //- **Recipient Response Text**: "[Actor Name] committed theft!? Why am I not surprised."
    //        reactions.Add(string.Format("{0} committed theft!? Why am I not surprised.", actor.name));
    //        //-**Recipient Effect**: Apply Crime System handling as if the Recipient witnessed Actor commit Theft.
    //        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //    }
    //    //Recipient and Actor have no relationship but are from the same faction:
    //    else if (!recipient.relationshipContainer.HasRelationshipWith(actor.currentAlterEgo) && recipient.faction == actor.faction) {
    //        //- **Recipient Response Text**: "[Actor Name] committed theft!? That's illegal."
    //        reactions.Add(string.Format("{0} committed theft!? That's illegal.", actor.name));
    //        //- **Recipient Effect**: Apply Crime System handling as if the Recipient witnessed Actor commit Theft.
    //        recipient.ReactToCrime(committedCrime, this, actorAlterEgo, status);
    //    }
    //    return reactions;
    //}
    //#endregion
}