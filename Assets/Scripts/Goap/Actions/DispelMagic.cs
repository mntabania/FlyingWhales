using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;

public class DispelMagic : GoapAction {

    public override ACTION_CATEGORY actionCategory { get { return ACTION_CATEGORY.INDIRECT; } }

    public DispelMagic() : base(INTERACTION_TYPE.DISPEL_MAGIC) {
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
        actionIconString = GoapActionStateDB.FirstAid_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.REMOVE_TRAIT, "Cursed", false, GOAP_EFFECT_TARGET.TARGET ));
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Dispel Magic Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    public override GoapActionInvalidity IsInvalid(ActualGoapNode node) {
        GoapActionInvalidity goapActionInvalidity = base.IsInvalid(node);
        IPointOfInterest poiTarget = node.poiTarget;
        if (goapActionInvalidity.isInvalid == false) {
            if ((poiTarget as Character).IsInOwnParty() == false) {
                goapActionInvalidity.isInvalid = true;
            }
        }
        return goapActionInvalidity;
    }
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            return poiTarget.traitContainer.HasTrait("Reanimated", "Cursed") && actor.currentRegion.GetTileObjectsOfType(TILE_OBJECT_TYPE.MAGIC_CIRCLE).Count > 0;
        }
        return false;
    }
    public override IPointOfInterest GetTargetToGoTo(ActualGoapNode goapNode) {
        List<TileObject> magicCircle = goapNode.actor.currentRegion.GetTileObjectsOfType(TILE_OBJECT_TYPE.MAGIC_CIRCLE);
        TileObject chosen = magicCircle[Random.Range(0, magicCircle.Count)];
        return chosen;
    }
    #endregion

    #region State Effects
    public void PreDispelMagicSuccess(ActualGoapNode goapNode) {
        //**Pre Effect 1**: Prevent movement of Target
        //(poiTarget as Character).marker.pathfindingAI.AdjustDoNotMove(1);

    }
    public void AfterDispelMagicSuccess(ActualGoapNode goapNode) {
        //**After Effect 1**: Reduce all of target's Enchantment type traits
        //goapNode.poiTarget.traitContainer.RemoveAllTraitsByType(goapNode.poiTarget, TRAIT_TYPE.ENCHANTMENT);
        goapNode.poiTarget.traitContainer.RemoveTrait(goapNode.poiTarget, "Cursed");
        if(goapNode.poiTarget is Character) {
            Character target = goapNode.poiTarget as Character;
            target.relationshipContainer.AdjustOpinion(target, goapNode.actor, "Base", 3);
        }
    }
    #endregion
}

public class DispelMagicData : GoapActionData {
    public DispelMagicData() : base(INTERACTION_TYPE.DISPEL_MAGIC) {
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, };
        requirementAction = Requirement;
    }
    private bool Requirement(Character actor, IPointOfInterest poiTarget, object[] otherData) {
        return poiTarget.traitContainer.HasTrait("Reanimated", "Cursed");
    }
}
