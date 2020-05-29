
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class RaiseCorpse : GoapAction {

    public RaiseCorpse() : base(INTERACTION_TYPE.RAISE_CORPSE) {
        actionIconString = GoapActionStateDB.Magic_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER, POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, RACE.WOLF, RACE.SPIDER, RACE.DRAGON, RACE.DEMON };
        canBeAdvertisedEvenIfTargetIsUnavailable = true;
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.RAISE_CORPSE, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Raise Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            if (poiTarget is Character targetCharacter) {
                return targetCharacter.isDead && !(targetCharacter is Summon);
            } else if (poiTarget is Tombstone tombstone) {
                return tombstone.gridTileLocation != null && tombstone.mapObjectVisual && !(tombstone.character is Summon);
            }
        }
        return false;
    }
    #endregion

    #region State Effects
    public void AfterRaiseSuccess(ActualGoapNode goapNode) {
        IPointOfInterest targetPOI = goapNode.poiTarget;
        Character target = null;
        if (targetPOI is Character) {
            target = targetPOI as Character;
        } else if (targetPOI is Tombstone) {
            target = (targetPOI as Tombstone).character;
        }
        CharacterManager.Instance.RaiseFromDeath(target, FactionManager.Instance.undeadFaction, className: target.characterClass.className);
    }
    #endregion

}