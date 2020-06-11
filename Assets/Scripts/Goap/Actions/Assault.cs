using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

//will be branded criminal if anybody witnesses or after combat
public class Assault : GoapAction {

    //private Character winner;
    private Character loser;

    public Assault() : base(INTERACTION_TYPE.ASSAULT) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER, POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] {
            RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON,
            RACE.GOLEM, RACE.KOBOLD, RACE.LESSER_DEMON, RACE.MIMIC, RACE.PIG, RACE.SHEEP, RACE.ENT, RACE.WISP,
            RACE.GHOST, RACE.NYMPH, RACE.SLIME, RACE.SLUDGE, RACE.CHICKEN, RACE.ELEMENTAL, RACE.ABOMINATION, RACE.ANGEL
        }; isNotificationAnIntel = true;
        doesNotStopTargetCharacter = true;
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.STARTS_COMBAT, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        Character actor = node.actor;
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if (actor.IsHealthCriticallyLow()) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    public override void Perform(ActualGoapNode actionNode) {
        base.Perform(actionNode);
        SetState("Combat Start", actionNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +50(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 50;
    }
    //public override string ReactionToActor(Character witness, ActualGoapNode node, REACTION_STATUS status) {
    //    string response = base.ReactionToActor(witness, node, status);
    //    Character actor = node.actor;
    //    IPointOfInterest target = node.poiTarget;
    //    if (!witness.IsHostileWith(actor)) {
    //        if (target is Character targetCharacter) {
    //            string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
    //            if (opinionLabel == RelationshipManager.Enemy || opinionLabel == RelationshipManager.Rival) {
    //                response += CharacterManager.Instance.TriggerEmotion(EMOTION.Approval, witness, actor, status, node);
    //            } else if (node.associatedJobType != JOB_TYPE.APPREHEND) {
    //                if (opinionLabel == RelationshipManager.Acquaintance) {
    //                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
    //                } else if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
    //                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disapproval, witness, actor, status, node);
    //                    response += CharacterManager.Instance.TriggerEmotion(EMOTION.Anger, witness, actor, status, node);
    //                }
    //            }
    //            if (node.associatedJobType != JOB_TYPE.APPREHEND && !actor.IsHostileWith(targetCharacter)) {
    //                CrimeManager.Instance.ReactToCrime(witness, actor, node, node.associatedJobType, CRIME_TYPE.MISDEMEANOR);
    //            }
    //        }
    //    }
    //    return response;
    //}
    //public override string ReactionToTarget(Character witness, ActualGoapNode node, REACTION_STATUS status) {
    //    string response = base.ReactionToTarget(witness, node, status);
    //    Character actor = node.actor;
    //    IPointOfInterest target = node.poiTarget;
    //    if (node.associatedJobType == JOB_TYPE.APPREHEND) {
    //        Character targetCharacter = target as Character;
    //        string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
    //        if (opinionLabel == RelationshipManager.Acquaintance) {
    //            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, targetCharacter, status, node);
    //        } else if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
    //            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disappointment, witness, targetCharacter, status, node);
    //            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Shock, witness, targetCharacter, status, node);
    //        } else {
    //            response += CharacterManager.Instance.TriggerEmotion(EMOTION.Disgust, witness, targetCharacter, status, node);
    //        }
    //    }
    //    return response;
    //}
    public override void OnStoppedInterrupt(ActualGoapNode node) {
        base.OnStoppedInterrupt(node);
        node.actor.combatComponent.RemoveHostileInRange(node.poiTarget);
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        if (node.associatedJobType == JOB_TYPE.APPREHEND || node.associatedJobType == JOB_TYPE.KNOCKOUT) {
            return REACTABLE_EFFECT.Neutral;
        } else {
            return REACTABLE_EFFECT.Negative;
        }
    }
    public override bool IsInvalidOnVision(ActualGoapNode node) {
        return false;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget is TileObject tileObject) {
                return tileObject.gridTileLocation != null;
            }
            return !actor.IsHealthCriticallyLow();
        }
        return false;
    }
    #endregion

    #region Effects
    public void AfterCombatStart(ActualGoapNode goapNode) {
        Debug.Log($"{goapNode.actor} will start combat towards {goapNode.poiTarget.name}");
        string combatReason = CombatManager.Action;
        bool isLethal = goapNode.associatedJobType.IsJobLethal();
        if(goapNode.associatedJobType == JOB_TYPE.DEMON_KILL) {
            combatReason = CombatManager.Demon_Kill;
        }
        //goapNode.actor.combatComponent.SetActionAndJobThatTriggeredCombat(goapNode, goapNode.actor.currentJob as GoapPlanJob);
        goapNode.actor.combatComponent.Fight(goapNode.poiTarget, combatReason, connectedAction: goapNode, isLethal: isLethal);
        // if(goapNode.poiTarget is Character) {
        //     Character targetCharacter = goapNode.poiTarget as Character;
        //     if (goapNode.associatedJobType != JOB_TYPE.APPREHEND && !goapNode.actor.IsHostileWith(targetCharacter)) {
        //         CrimeManager.Instance.ReactToCrime(targetCharacter, goapNode.actor, goapNode, goapNode.associatedJobType, CRIME_TYPE.MISDEMEANOR);
        //     }
        // }
    }
    #endregion
}

public class AssaultData : GoapActionData {
    public AssaultData() : base(INTERACTION_TYPE.ASSAULT) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
        requirementAction = Requirement;
    }

    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        if (poiTarget is Character && actor != poiTarget) {
            Character target = poiTarget as Character;
            if (target.canPerform) { //!target.traitContainer.HasTraitOf(TRAIT_TYPE.DISABLER, TRAIT_EFFECT.NEGATIVE)
                return true;
            }
        }
        return false;
    }
}