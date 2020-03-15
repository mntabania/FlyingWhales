using Inner_Maps;
using Traits;

public class MineMetalRegion : GoapAction {
    
    public MineMetalRegion() : base(INTERACTION_TYPE.MINE_METAL_REGION) {
        actionIconString = GoapActionStateDB.Work_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        actionLocationType = ACTION_LOCATION_TYPE.NEAR_TARGET;
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.PRODUCE_METAL, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Mine Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        return 25;
    }
    #endregion

    #region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, object[] otherData) { 
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData);
        if (satisfied) {
            //**Requirements:** Actor has Miner trait. Region has Mines Landmark. Region is owned by Actor's Faction or Actor's Home's Ruling Faction.
            var region = poiTarget.gridTileLocation.parentMap.region.coreTile.region;
            // return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null &&
            //        actor.traitContainer.GetNormalTrait<Trait>("Miner") != null &&
            //        region.mainLandmark.specificLandmarkType == LANDMARK_TYPE.MINES &&
            //        (region.owner == actor.faction || region.owner == actor.homeRegion.owner);
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreMineSuccess(ActualGoapNode goapNode) {
        goapNode.descriptionLog.AddToFillers(goapNode.poiTarget.gridTileLocation.parentMap.region.coreTile.region, goapNode.poiTarget.gridTileLocation.parentMap.region.coreTile.region.name, LOG_IDENTIFIER.LANDMARK_1);
    }
    public void AfterMineSuccess(ActualGoapNode goapNode) {
        //**After Effect 1**: Produce Metal random between 100 - 300
        var random = UnityEngine.Random.Range(100, 301);
        var pile = InnerMapManager.Instance.CreateNewTileObject<MetalPile>(TILE_OBJECT_TYPE.METAL_PILE);
        pile.SetResourceInPile(random);
        goapNode.actor.gridTileLocation.structure.AddPOI(pile);
        goapNode.descriptionLog.AddToFillers(null, random.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    #endregion
}