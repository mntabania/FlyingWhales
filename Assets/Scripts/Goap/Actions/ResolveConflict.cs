using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;  
using Traits;

public class ResolveConflict : GoapAction {

    public ResolveConflict() : base(INTERACTION_TYPE.RESOLVE_CONFLICT) {
        actionIconString = GoapActionStateDB.Work_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.RATMAN };
        logTags = new[] {LOG_TAG.Social};
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.TARGET_REMOVE_RELATIONSHIP, conditionKey = "Enemy", target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Resolve Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
        return 4;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity =  base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            Character targetCharacter = poiTarget as Character;
            if ((targetCharacter.traitContainer.HasTrait("Hothead") && UnityEngine.Random.Range(0, 2) == 0)
                || targetCharacter.combatComponent.isInCombat
                || (targetCharacter.stateComponent.currentState != null && targetCharacter.stateComponent.currentState.characterState == CHARACTER_STATE.BERSERKED)) {
                goapActionInvalidity.isInvalid = true;
                goapActionInvalidity.stateName = "Resolve Fail";
            } 
        }
        return goapActionInvalidity;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            bool hasEnemy = false;
            if (poiTarget is Character) {
                Character targetCharacter = poiTarget as Character;
                hasEnemy = targetCharacter.relationshipContainer.GetFirstEnemyCharacter() != null;
            }
            return actor != poiTarget && hasEnemy && actor.traitContainer.HasTrait("Diplomatic");
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreResolveSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is Character) {
            Character targetCharacter = goapNode.poiTarget as Character;
            //List<Relatable> allEnemyTraits = targetCharacter.relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_TRAIT.ENEMY);
            Character target = targetCharacter.relationshipContainer.GetFirstEnemyCharacter();
            if (target != null) {
                goapNode.descriptionLog.AddToFillers(target, target.name, LOG_IDENTIFIER.CHARACTER_3);
            } else {
                throw new System.Exception(
                    $"Cannot resolve conflict for {targetCharacter.name} because he/she does not have enemies!");
            }
        }
    }
    public void AfterResolveSuccess(ActualGoapNode goapNode) {
        if (goapNode.poiTarget is Character) {
            Character targetCharacter = goapNode.poiTarget as Character;
            //List<Relatable> allEnemyTraits = targetCharacter.relationshipContainer.GetRelatablesWithRelationship(RELATIONSHIP_TRAIT.ENEMY);
            // Character target = targetCharacter.RelationshipManager.GetEnemyCharacters().First();
            // if (target != null) {
            //     RelationshipManager.Instance.RemoveOneWayRelationship(targetCharacter.currentAlterEgo, target, RELATIONSHIP_TYPE.ENEMY);
            // } else {
            //     throw new System.Exception("Cannot resolve conflict for " + targetCharacter.name + " because he/she does not have enemies!");
            // }
        }
    }
    #endregion
}