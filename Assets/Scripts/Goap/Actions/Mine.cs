using Inner_Maps;
using Inner_Maps.Location_Structures;
using JetBrains.Annotations;
using UnityEngine.Assertions;

public class Mine : GoapAction {
    public Mine() : base(INTERACTION_TYPE.MINE) {
        actionIconString = GoapActionStateDB.Mine_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.CHARACTER };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }
    
    #region Overrides
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
    
    #region State Effects
    [UsedImplicitly]
    public void AfterMineSuccess(ActualGoapNode goapNode) {
        Cave cave = goapNode.targetStructure as Cave;
        Assert.IsNotNull(cave, $"Cave of mine performed by {goapNode.actor.name} is null!");
        string mineResult = cave.resourceYield.PickRandomElementGivenWeights();
        switch (mineResult) {
            case Cave.Yield_Diamond:
                goapNode.actor.gridTileLocation.structure.AddPOI(
                    InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.DIAMOND),
                    goapNode.actor.gridTileLocation);
                break;
            case Cave.Yield_Gold:
                goapNode.actor.gridTileLocation.structure.AddPOI(
                    InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.GOLD),
                    goapNode.actor.gridTileLocation);
                break;
            case Cave.Yield_Metal:
                goapNode.actor.gridTileLocation.structure.AddPOI(
                    InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.METAL_PILE),
                    goapNode.actor.gridTileLocation);
                break;
            case Cave.Yield_Nothing:
                break;
        }
    }
    #endregion
}