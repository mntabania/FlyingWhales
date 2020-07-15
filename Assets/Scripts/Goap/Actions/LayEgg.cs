
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps;

public class LayEgg : GoapAction {

    public LayEgg() : base(INTERACTION_TYPE.LAY_EGG) {
        actionIconString = GoapActionStateDB.Happy_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] {
            RACE.WOLF, RACE.SPIDER, RACE.DRAGON,
            RACE.GOLEM, RACE.KOBOLD, RACE.LESSER_DEMON,
            RACE.NYMPH, RACE.ELEMENTAL, RACE.ABOMINATION,
        };
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Lay Success", goapNode);
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
            return actor.gridTileLocation != null && actor.gridTileLocation.objHere == null;
        }
        return false;
    }
    #endregion

    #region State Effects
    public void AfterLaySuccess(ActualGoapNode goapNode) {
        if(goapNode.actor is Summon summon) {
            if(summon.gridTileLocation != null && summon.gridTileLocation.objHere == null) {
                TILE_OBJECT_TYPE eggType = CharacterManager.Instance.GetEggType(summon.summonType);
                if(eggType != TILE_OBJECT_TYPE.NONE) {
                    MonsterEgg egg = InnerMapManager.Instance.CreateNewTileObject<MonsterEgg>(eggType);
                    egg.SetCharacterThatLay(summon);
                    summon.gridTileLocation.structure.AddPOI(egg, summon.gridTileLocation);
                    summon.behaviourComponent.SetHasLayedAnEgg(true);
                }
            }
        }
    }
    #endregion

}