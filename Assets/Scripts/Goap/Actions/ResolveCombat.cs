using System.Collections;
using System.Collections.Generic;
using Inner_Maps.Location_Structures;
using UnityEngine;
using Traits;

public class ResolveCombat : GoapAction {

    public ResolveCombat() : base(INTERACTION_TYPE.RESOLVE_COMBAT) {
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        actionIconString = GoapActionStateDB.Hostile_Icon;
        doesNotStopTargetCharacter = true;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER, POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.DEMON, RACE.SPIDER, RACE.SKELETON };
        canBeAdvertisedEvenIfActorIsUnavailable = true;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.STARTS_COMBAT, target = GOAP_EFFECT_TARGET.TARGET }, IsCombatFinished);
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT, conditionKey = "Unconscious", target = GOAP_EFFECT_TARGET.TARGET });
        //AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.CANNOT_MOVE, target = GOAP_EFFECT_TARGET.TARGET });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.HAS_TRAIT_EFFECT, conditionKey = "Negative", target = GOAP_EFFECT_TARGET.TARGET });
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET });
    }
    public override LocationStructure GetTargetStructure(ActualGoapNode node) {
        return node.actor.gridTileLocation.structure;
    }
    public override void Perform(ActualGoapNode actionNode) {
        base.Perform(actionNode);
        SetState("Combat Success", actionNode);
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        string stateName = "Target Missing";
        bool defaultTargetMissing = false;
        GoapActionInvalidity goapActionInvalidity = new GoapActionInvalidity(defaultTargetMissing, stateName);
        //resolve cannot be invalid
        return goapActionInvalidity;
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +50(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 50;
    }
    #endregion

    #region Effects
    public void AfterCombatSuccess(ActualGoapNode goapNode) {

    }
    #endregion

    #region Preconditions
    private bool IsCombatFinished(Character actor, IPointOfInterest target, object[] otherData) {
        if (target is Character) {
            Character targetCharacter = target as Character;
            //target character must be
            // - unable to move
            //to consider the combat as finished
            if (targetCharacter.traitContainer.HasTrait("Unconscious") || targetCharacter.isDead) {
                return true;
            }
        } else {
            return target.gridTileLocation == null;
        }
        return false;
    }
    #endregion
}
