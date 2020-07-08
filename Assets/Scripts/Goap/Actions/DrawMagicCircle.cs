using Inner_Maps;

public class DrawMagicCircle : GoapAction {
    public DrawMagicCircle() : base(INTERACTION_TYPE.DRAW_MAGIC_CIRCLE) {
        actionIconString = GoapActionStateDB.Magic_Icon;
        advertisedBy = new POINT_OF_INTEREST_TYPE[] { POINT_OF_INTEREST_TYPE.TILE_OBJECT };
        racesThatCanDoAction = new RACE[] { RACE.HUMANS, RACE.ELVES, RACE.GOBLIN, RACE.FAERY };
    }
    
    #region Overrides
    public override void Perform(ActualGoapNode goapNode) {
        base.Perform(goapNode);
        SetState("Draw Success", goapNode);
    }
    protected override int GetBaseCost(Character actor, IPointOfInterest target, JobQueueItem job, object[] otherData) {
        string costLog = $"\n{name} {target.nameWithID}: +10(Constant)";
        actor.logComponent.AppendCostLog(costLog);
        return 10;
    }
    // public override LocationGridTile GetTargetTileToGoTo(ActualGoapNode goapNode) {
    //     if (goapNode.otherData != null && goapNode.otherData.Length == 1 && goapNode.otherData[0] is LocationGridTile tile) {
    //         return tile;
    //     }
    //     return base.GetTargetTileToGoTo(goapNode);
    // }
    #endregion
    
    #region State Effects
    public void AfterDrawSuccess(ActualGoapNode goapNode) {
        // goapNode.actor.gridTileLocation.structure.AddPOI(
        //         InnerMapManager.Instance.CreateNewTileObject<TileObject>(TILE_OBJECT_TYPE.MAGIC_CIRCLE), 
        //         goapNode.actor.gridTileLocation
        // );
        if (goapNode.poiTarget is TileObject tileObject) {
            tileObject.SetMapObjectState(MAP_OBJECT_STATE.BUILT);
        }
    }
    #endregion
}