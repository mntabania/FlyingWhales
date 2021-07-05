using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;  
using Traits;

public class Strangle : GoapAction {

    public Strangle() : base(INTERACTION_TYPE.STRANGLE) {
        actionIconString = GoapActionStateDB.Anger_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.RANDOM_LOCATION;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Life_Changes};
    }

    #region Override
    public override bool ShouldActionBeAnIntel(ActualGoapNode node) {
        return true;
    }
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Strangle Success", goapNode);
    }
    public override void AddFillersToLog(Log log, ActualGoapNode node) {
        base.AddFillersToLog(log, node);
        if (node.otherData != null && node.otherData.Length == 1) {
            string reason = (string)node.otherData[0].obj;
            log.AddToFillers(null, reason, LOG_IDENTIFIER.STRING_1);    
        }
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        Character actor = node.actor;
        if (actor.homeStructure != null) {
            return actor.homeStructure;
        } else {
            return actor.currentRegion.wilderness;
        }
    }
    public override void PopulateEmotionReactionsToActor(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToActor(reactions, actor, target, witness, node, status);
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (actor != targetCharacter) {
                if (witness.traitContainer.HasTrait("Coward")) {
                    reactions.Add(EMOTION.Fear);
                } else {
                    string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                    if (opinionLabel == RelationshipManager.Rival) {
                        reactions.Add(EMOTION.Approval);
                    } else if (opinionLabel == RelationshipManager.Friend || opinionLabel == RelationshipManager.Close_Friend) {
                        reactions.Add(EMOTION.Anger);
                        reactions.Add(EMOTION.Threatened);
                    } else {
                        reactions.Add(EMOTION.Shock);
                        reactions.Add(EMOTION.Disapproval);
                    }
                }
            } else {
                reactions.Add(EMOTION.Disapproval);
                reactions.Add(EMOTION.Shock);
                if (witness.traitContainer.HasTrait("Psychopath") || witness.relationshipContainer.IsEnemiesWith(actor)) {
                    reactions.Add(EMOTION.Scorn);
                } else {
                    //reactions.Add(EMOTION.Disapproval);
                }
            }
        }
    }
    public override void PopulateEmotionReactionsToTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, Character witness, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsToTarget(reactions, actor, target, witness, node, status);
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (actor != targetCharacter) {
                string opinionLabel = witness.relationshipContainer.GetOpinionLabel(targetCharacter);
                if (opinionLabel == RelationshipManager.Rival) {
                    reactions.Add(EMOTION.Scorn);
                } else {
                    reactions.Add(EMOTION.Concern);
                }
            }
        }
    }
    public override void PopulateEmotionReactionsOfTarget(List<EMOTION> reactions, Character actor, IPointOfInterest target, ActualGoapNode node, REACTION_STATUS status) {
        base.PopulateEmotionReactionsOfTarget(reactions, actor, target, node, status);
        if (target is Character) {
            Character targetCharacter = target as Character;
            if (actor != targetCharacter) {
                reactions.Add(EMOTION.Anger);
                if (targetCharacter.relationshipContainer.IsFriendsWith(actor) && !targetCharacter.traitContainer.HasTrait("Psychopath")) {
                    reactions.Add(EMOTION.Betrayal);
                }
            }
        }
    }
    public override REACTABLE_EFFECT GetReactableEffect(ActualGoapNode node, Character witness) {
        return REACTABLE_EFFECT.Negative;
    }
    public override CRIME_TYPE GetCrimeType(Character actor, IPointOfInterest target, ActualGoapNode crime) {
        if(actor == target) {
            return CRIME_TYPE.None;
        }
        return CRIME_TYPE.Murder;
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return poiTarget == actor && poiTarget.IsAvailable() && poiTarget.gridTileLocation != null;
        }
        return false;
    }
#endregion

#region State Effects
    public void PerTickStrangleSuccess(ActualGoapNode goapNode) {
        goapNode.actor.AdjustHP(-(int)(goapNode.actor.maxHP * 0.18f), ELEMENTAL_TYPE.Normal, showHPBar: true);
    }
    public void AfterStrangleSuccess(ActualGoapNode goapNode) {
        //Character target = goapNode.poiTarget as Character;
        //string deathReason = string.Empty;
        //if(target == goapNode.actor) {
        //    deathReason = "suicide";
        //} else {
        //    deathReason = "murder";
        //}
        //target.Death("suicide", goapNode, _deathLog: goapNode.action.states[goapNode.currentStateName].descriptionLog);
        Character responsibleCharacter = null;
        if(goapNode.actor != goapNode.poiTarget) {
            //Only put responsible character if target is strangled by another character
            //If suicide, do not put the responsible character as himself
            //because this might cause problems if the responsible character for death is also himself
            responsibleCharacter = goapNode.actor;
        }
        goapNode.actor.Death("suicide", goapNode, responsibleCharacter: responsibleCharacter, _deathLog: goapNode.descriptionLog);

    }
#endregion
}