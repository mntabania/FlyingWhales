using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;

public class Pickpocket : GoapAction {

    public Pickpocket() : base(INTERACTION_TYPE.PICKPOCKET) {
        actionIconString = GoapActionStateDB.Steal_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Crimes};
    }

    #region Overrides
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
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
        SetState("Pickpocket Success", goapNode);
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
            if(target is Character targetCharacter) {
                string opinionLabel = actor.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (actor.moodComponent.moodState == MOOD_STATE.Normal || opinionLabel == RelationshipManager.Acquaintance ||
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
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        string stateName = "Target Missing";
        bool isInvalid = false;
        //pickpocket can never be invalid since requirement handle all cases of invalidity.
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
        if (target is Character targetCharacter) {
            reactions.Add(EMOTION.Disappointment);
            if (targetCharacter.traitContainer.HasTrait("Hothead") || UnityEngine.Random.Range(0, 100) < 35) {
                reactions.Add(EMOTION.Anger);
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
            Character targetCharacter = poiTarget as Character;
            if(otherData != null && otherData.Length == 1 && otherData[0].obj is TileObject tileObject) {
                return targetCharacter.HasItem(tileObject);
            }
            return targetCharacter.items.Count > 0;
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
    public void AfterPickpocketSuccess(ActualGoapNode goapNode) {
        OtherData[] otherData = goapNode.otherData;
        TileObject targetTileObject = null;
        if (otherData != null && otherData.Length == 1 && otherData[0].obj is TileObject tileObject) {
            targetTileObject = tileObject;
        } else {
            Character targetCharacter = goapNode.poiTarget as Character;
            targetTileObject = targetCharacter.GetRandomItem();
        }
        if(targetTileObject != null) {
            goapNode.actor.PickUpItem(targetTileObject);
        }
        if (goapNode.actor.traitContainer.HasTrait("Kleptomaniac")) {
            goapNode.actor.needsComponent.AdjustHappiness(10);
        }
    }
    #endregion
}