using System.Collections;
using System.Collections.Generic;
using Inner_Maps;
using UnityEngine;  
using Traits;

public class MineStone : GoapAction {
    //private const int MAX_SUPPLY = 50;
    //private const int MIN_SUPPLY = 20;

    public MineStone() : base(INTERACTION_TYPE.MINE_STONE) {
        actionIconString = GoapActionStateDB.Mine_Icon;
        
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY, RACE.SKELETON, };
        validTimeOfDays = new TIME_IN_WORDS[] { TIME_IN_WORDS.MORNING, TIME_IN_WORDS.LUNCH_TIME, TIME_IN_WORDS.AFTERNOON, TIME_IN_WORDS.EARLY_NIGHT };
    }

    #region Overrides
    protected override void ConstructBasePreconditionsAndEffects() {
        AddExpectedEffect(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.PRODUCE_STONE, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.ACTOR });
    }
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Mine Success", goapNode);
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
            return poiTarget.IsAvailable() && poiTarget.gridTileLocation != null && actor.traitContainer.HasTrait("Miner");
        }
        return false;
    }
    #endregion

    #region State Effects
    public void PreMineSuccess(ActualGoapNode goapNode) {
        Rock rock = goapNode.poiTarget as Rock;
        goapNode.descriptionLog.AddToFillers(null, rock.yield.ToString(), LOG_IDENTIFIER.STRING_1);
    }
    public void AfterMineSuccess(ActualGoapNode goapNode) {
        Rock rock = goapNode.poiTarget as Rock;
        int stone = rock.yield;
        LocationGridTile tile = rock.gridTileLocation;
        rock.AdjustYield(-stone);

        StonePile stonePile = InnerMapManager.Instance.CreateNewTileObject<StonePile>(TILE_OBJECT_TYPE.STONE_PILE);
        stonePile.SetResourceInPile(stone);
        tile.structure.AddPOI(stonePile, tile);
        stonePile.gridTileLocation.SetReservedType(TILE_OBJECT_TYPE.STONE_PILE);
    }
    #endregion
}