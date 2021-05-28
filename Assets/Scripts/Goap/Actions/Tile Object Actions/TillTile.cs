
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Traits;
using Inner_Maps;

public class TillTile : GoapAction {

    public int m_amountProducedPerTick = 1;

    public TillTile() : base(INTERACTION_TYPE.TILL_TILE) {
        actionIconString = GoapActionStateDB.Work_Icon;
        //advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.ELVES, RACE.HUMANS, RACE.RATMAN, };
        logTags = new[] { LOG_TAG.Work };
    }

    #region Overrides
    //protected override void ConstructBasePreconditionsAndEffects() {
    //    AddPrecondition(new GoapEffect() { conditionType = GOAP_EFFECT_CONDITION.DEATH, conditionKey = string.Empty, isKeyANumber = false, target = GOAP_EFFECT_TARGET.TARGET }, IsTargetDead);
    //    AddExpectedEffect(new GoapEffect(GOAP_EFFECT_CONDITION.ABSORB_LIFE, string.Empty, false, GOAP_EFFECT_TARGET.ACTOR));
    //}
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Till Tile Success", goapNode);
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

        return satisfied;
    }
    #endregion

    #region State Effects
    public void AfterTillTileSuccess(ActualGoapNode goapNode) {
        //TileObject currentTileObject = p_targetTile.tileObjectComponent.hiddenObjHere;
        LocationGridTile tileLocation = goapNode.target.gridTileLocation;
        if (tileLocation.tileObjectComponent.objHere != null) {
            tileLocation.structure.RemovePOI(tileLocation.tileObjectComponent.objHere);
        }

        //add crop to tile.
        TILE_OBJECT_TYPE tileObjectType = GetCropToCreate(tileLocation);
        tileLocation.structure.AddPOI(InnerMapManager.Instance.CreateNewTileObject<TileObject>(tileObjectType), tileLocation);

        //TileObject tileLot = goapNode.target as TileObject;
        //tileLot.gridTileLocation.tileObjectComponent.objHere
        //LocationGridTile tileToTill = goapNode.target.gridTileLocation;
    }
    private TILE_OBJECT_TYPE GetCropToCreate(LocationGridTile p_tile) {
        if (p_tile.specificBiomeTileType == Biome_Tile_Type.Grassland) {
            return TILE_OBJECT_TYPE.CORN_CROP;
        } else if (p_tile.specificBiomeTileType == Biome_Tile_Type.Jungle) {
            return TILE_OBJECT_TYPE.POTATO_CROP;
        } else if (p_tile.specificBiomeTileType == Biome_Tile_Type.Desert || 
                   p_tile.specificBiomeTileType == Biome_Tile_Type.Oasis) {
            return TILE_OBJECT_TYPE.PINEAPPLE_CROP;
        } else if (p_tile.specificBiomeTileType == Biome_Tile_Type.Snow || 
                   p_tile.specificBiomeTileType == Biome_Tile_Type.Taiga || 
                   p_tile.specificBiomeTileType == Biome_Tile_Type.Tundra) {
            return TILE_OBJECT_TYPE.ICEBERRY_CROP;
        }
        throw new Exception($"No crop production case for {p_tile.specificBiomeTileType.ToString()}");
    }
    #endregion
}