
using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using Traits;
using Inner_Maps;

public class LayEgg : GoapAction {

    public LayEgg() : base(INTERACTION_TYPE.LAY_EGG) {
        actionIconString = GoapActionStateDB.Question_Icon;
        actionLocationType = ACTION_LOCATION_TYPE.IN_PLACE;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] {
            RACE.WOLF, RACE.SPIDER, RACE.DRAGON,
            RACE.GOLEM, RACE.KOBOLD, RACE.LESSER_DEMON,
            RACE.NYMPH, RACE.ELEMENTAL, RACE.ABOMINATION,
        };
        logTags = new[] {LOG_TAG.Life_Changes};
    }

    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Lay Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, OtherData[] otherData) {
#if DEBUG_LOG
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
#endif
        return 10;
    }
#endregion

#region Requirements
    protected override bool AreRequirementsSatisfied(Character actor, IPointOfInterest poiTarget, OtherData[] otherData, JobQueueItem job) {
        bool satisfied = base.AreRequirementsSatisfied(actor, poiTarget, otherData, job);
        if (satisfied) {
            return actor.gridTileLocation != null && actor.gridTileLocation.tileObjectComponent.objHere == null;
        }
        return false;
    }
#endregion

#region State Effects
    public void AfterLaySuccess(ActualGoapNode goapNode) {
        if(goapNode.actor is Summon summon) {
            if(summon.gridTileLocation != null && summon.gridTileLocation.tileObjectComponent.objHere == null) {
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