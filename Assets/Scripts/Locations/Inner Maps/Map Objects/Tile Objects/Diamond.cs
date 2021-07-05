public class Diamond : TileObject{
    public Diamond() {
        Initialize(TILE_OBJECT_TYPE.DIAMOND);
    }
    public Diamond(SaveDataTileObject data) : base(data) {
        
    }


    #region Reactions
    public override void GeneralReactionToTileObject(Character actor, ref string debugLog) {
        base.GeneralReactionToTileObject(actor, ref debugLog);
        if (actor is Troll) {
            if (actor.homeStructure != null && gridTileLocation.structure != actor.homeStructure && !actor.jobQueue.HasJob(JOB_TYPE.DROP_ITEM)) {
                actor.jobComponent.CreateHoardItemJob(this, actor.homeStructure, true);
            }
        }
    }
    #endregion
}